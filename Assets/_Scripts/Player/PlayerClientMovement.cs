using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
[Serializable]
public struct InputPayload : INetworkSerializable
{
    public int tick;
    public float delta;
    public Vector2 inputDirection;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if(serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            if(!reader.TryBeginRead(sizeof(int) + sizeof(float) + sizeof(float) + sizeof(float)))
            {
                throw new OverflowException("Not enough space in the buffer");
            }

            reader.ReadValue(out tick);
            reader.ReadValue(out delta);
            reader.ReadValue(out inputDirection.x);
            reader.ReadValue(out inputDirection.y);

        }

        if(serializer.IsWriter)
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            if (!writer.TryBeginWrite(sizeof(int) + sizeof(float) + sizeof(float) + sizeof(float)))
            {
                throw new OverflowException("Not enough space in the buffer");
            }

            writer.WriteValue(tick);
            writer.WriteValue(delta);
            writer.WriteValue(inputDirection.x);
            writer.WriteValue(inputDirection.y);
        }
    }
}
[Serializable]
public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector2 velocity;
    public Vector3 position;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            FastBufferReader reader = serializer.GetFastBufferReader();
            if (!reader.TryBeginRead(sizeof(int) + sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float)))
            {
                throw new OverflowException("Not enough space in the buffer");
            }

            reader.ReadValue(out tick);
            reader.ReadValue(out velocity.x);
            reader.ReadValue(out velocity.y);
            reader.ReadValue(out position.x);
            reader.ReadValue(out position.y);

        }

        if (serializer.IsWriter)
        {
            FastBufferWriter writer = serializer.GetFastBufferWriter();
            if (!writer.TryBeginWrite(sizeof(int) + sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float)))
            {
                throw new OverflowException("Not enough space in the buffer");
            }

            writer.WriteValue(tick);
            writer.WriteValue(velocity.x);
            writer.WriteValue(velocity.y);
            writer.WriteValue(position.x);
            writer.WriteValue(position.y);
        }
    }
}
public class PlayerClientMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    private float timer;
    private int tickNumber;

    //Network
    private const int BUFFER_SIZE = 1024;
    private InputPayload[] inputBuffer = new InputPayload[BUFFER_SIZE];
    private StatePayload[] stateBuffer = new StatePayload[BUFFER_SIZE];
    
    private NetworkVariable<bool> isReconciliating = new NetworkVariable<bool>();

    //Client
    private Queue<StatePayload> client_ServerStates = new Queue<StatePayload>();

    //Server
    private Queue<InputPayload> server_ClientInputQueue = new Queue<InputPayload>();
    private NetworkVariable<Vector2> server_PlayerPosition = new NetworkVariable<Vector2>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        server_PlayerPosition.Value = rb.position;
        server_PlayerPosition.OnValueChanged += OnPlayerPositionChanged;
    }

    private void OnPlayerPositionChanged(Vector2 previousValue, Vector2 newValue)
    {
        if (!IsOwner)
        {
            transform.position = newValue;
        }
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            if(!isReconciliating.Value)
            {
                timer += Time.deltaTime;
                while (timer >= Time.fixedDeltaTime)
                {
                    timer -= Time.fixedDeltaTime;

                    int bufferIndex = tickNumber % BUFFER_SIZE;

                    InputPayload input = new InputPayload();
                    input.inputDirection = InputManager.moveDirection;
                    input.tick = tickNumber;
                    input.delta = Time.fixedDeltaTime;

                    //Send input to server
                    SendInputToServerRpc(input);

                    inputBuffer[bufferIndex] = input;
                    StatePayload clientState = new StatePayload()
                    {
                        tick = tickNumber,
                        position = rb.position,
                        velocity = rb.velocity
                    };
                    stateBuffer[bufferIndex] = clientState;

                    MovePlayer(input);

                    Physics2D.Simulate(input.delta);

                    ++tickNumber;

                }
            }

            #region Server Reconciliation

            if (client_ServerStates.Count() > 0)
            {
                StatePayload serverState = client_ServerStates.Dequeue();
                while (client_ServerStates.Count() > 0)
                {
                    serverState = client_ServerStates.Dequeue();
                }

                int stateIndex = serverState.tick % BUFFER_SIZE;

                StatePayload calculatedState = stateBuffer[stateIndex];

                float error = Vector2.Distance(serverState.position, calculatedState.position);

                if (error > 0.1f)
                {
                    StartReconciliationServerRpc(true);
                    Debug.Log("Correcting for error at tick " + serverState.tick + " (rewinding " + (tickNumber - serverState.tick) + " ticks)");
                    //Debug.Log("Reconciliation Needed. Position error: " + error);
                    //Debug.Log("Index: " + stateIndex + "||Client Position: " + calculatedState.position + "|| Server Position: " + serverState.position);

                    rb.position = serverState.position;
                    rb.velocity = serverState.velocity;

                    int rewindTick = serverState.tick;

                    while (rewindTick < tickNumber)
                    {
                        Debug.Log("Rewinding. Ticks left: " + (tickNumber - rewindTick));
                        int bufferIndex = rewindTick % BUFFER_SIZE;

                        MovePlayer(inputBuffer[bufferIndex]);

                        stateBuffer[bufferIndex].position = rb.position;
                        stateBuffer[bufferIndex].velocity = rb.velocity;

                        ++rewindTick;
                    }

                    Physics2D.Simulate(0);

                    Debug.Log("Simulation ended. Client Position: " + rb.position + " Velocity: " + rb.velocity + " Server Position: " + serverState.position + " Velocity: " + serverState.velocity);

                    StartReconciliationServerRpc(false);
                }
            }
            #endregion
        }

        if (IsServer && !IsOwnedByServer)
        {
            if (isReconciliating.Value)
            {
                server_ClientInputQueue.Clear();
            }
            else
            {
                while (server_ClientInputQueue.Count() > 0)
                {
                    InputPayload input = server_ClientInputQueue.Dequeue();
                    MovePlayer(input);
                    Physics2D.Simulate(input.delta);

                    StatePayload statePayload = new StatePayload();
                    statePayload.tick = input.tick + 1;
                    statePayload.position = rb.position;
                    statePayload.velocity = rb.velocity;

                    //ClientRpcParams clientRpcParams = new ClientRpcParams()
                    //{
                    //    Send = new ClientRpcSendParams()
                    //    {
                    //        TargetClientIds = new ulong[] { OwnerClientId }
                    //    }
                    //};

                    SendStateToClientRpc(statePayload);

                }
            }
        }

        if(IsServer)
        {
            server_PlayerPosition.Value = rb.position;
        }
    }


    private void MovePlayer(InputPayload input)
    {
        rb.MovePosition(rb.position + 8 * input.delta * input.inputDirection);
    }

    [ClientRpc]
    private void SendStateToClientRpc(StatePayload statePayload)
    {
        client_ServerStates.Enqueue(statePayload);
    }

    [ServerRpc]
    private void SendInputToServerRpc(InputPayload input)
    {
        server_ClientInputQueue.Enqueue(input);
    }

    [ServerRpc]
    private void StartReconciliationServerRpc(bool start)
    {
        isReconciliating.Value = start;
    }

    #region Intento 1
    //public override void OnNetworkSpawn()
    //{
    //    NetworkManager.NetworkTickSystem.Tick += Tick;
    //    if(IsServer) ServerState.Value = GetPlayerState(NetworkManager.LocalTime.Tick);
    //    ServerState.OnValueChanged += OnServerStateValueChanged;
    //}

    //private void OnServerStateValueChanged(PlayerStatePayload previouState, PlayerStatePayload newState)
    //{
    //    if (IsOwner && !isPosSync) return;
    //    transform.position = newState.position;
    //    isPosSync = true;
    //}

    //private void FixedUpdate()
    //{

    //}

    //private void Tick()
    //{
    //    if (IsClient && IsOwner)
    //    {
    //        //Verify if server states are equal to client states
    //        while (client_ServerStates.Count > 0)
    //        {
    //            PlayerStatePayload serverState = client_ServerStates.Dequeue();

    //            //Check if local buffer state still has the tick that server sends
    //            if (stateBuffer.Any(localState => localState.idPrediction == serverState.idPrediction))
    //            {
    //                //Check if states are equal
    //                PlayerStatePayload clientState = stateBuffer.First(localState => localState.idPrediction == serverState.idPrediction);
    //                float error = Vector2.Distance(clientState.position, serverState.position);

    //                if (error > 0.1f)
    //                {
    //                    Debug.Log("Position Error! Need to reconciliate. Error: " + error);
    //                    Debug.Log("Client Position: " + clientState.position + " Server Position: " + serverState.position);
    //                    transform.position = serverState.position;
    //                }
    //            }
    //            else//If not, that tick is missing in local buffer state
    //            {
    //                Debug.Log("Missing tick in local buffer. Tick (Id Prediction) " + serverState.idPrediction);
    //            }
    //        }

    //        int bufferIndex = NetworkManager.LocalTime.Tick % BUFFER_SIZE;

    //        InputPayload newInput = new InputPayload()
    //        {
    //            idPrediction = NetworkManager.ServerTime.Tick,
    //            delta = Time.fixedDeltaTime,
    //            inputDirection = InputManager.moveDirection
    //        };

    //        MovePlayer(newInput);
    //        inputBuffer[bufferIndex] = newInput;
    //        stateBuffer[bufferIndex] = GetPlayerState(NetworkManager.ServerTime.Tick);

    //        if (newInput.inputDirection != Vector2.zero)
    //        {
    //            SendInputToServerRpc(newInput);
    //        }
    //    }

    //    if (IsServer)
    //    {
    //        while (server_ClientInputQueue.Count() > 0)
    //        {
    //            InputPayload input = server_ClientInputQueue.Dequeue();
    //            MovePlayer(input);

    //            ClientRpcParams clientRpcParams = new ClientRpcParams()
    //            {
    //                Send = new ClientRpcSendParams()
    //                {
    //                    TargetClientIds = new ulong[] { OwnerClientId }
    //                }
    //            };

    //            PlayerStatePayload server_playerState = GetPlayerState(input.idPrediction);

    //            ServerState.Value = server_playerState;

    //            SendStatePayloadToClientRpc(server_playerState, clientRpcParams);
    //        }
    //    }

    //}

    //private void MovePlayer(InputPayload input)
    //{
    //    Physics.autoSimulation = false;
    //    rb.MovePosition(transform.position + 8 * input.delta * (Vector3)input.inputDirection);
    //}

    //private PlayerStatePayload GetPlayerState(int newTick)
    //{
    //    return new PlayerStatePayload()
    //    {
    //        idPrediction = newTick,
    //        delta = Time.deltaTime,
    //        position = transform.position
    //    };
    //}

    //#region Client
    //[ClientRpc]
    //private void SendStatePayloadToClientRpc(PlayerStatePayload serverState, ClientRpcParams clientRpcParams = default)
    //{
    //    client_ServerStates.Enqueue(serverState);
    //}
    //#endregion

    //#region Server
    //[ServerRpc]
    //private void SendInputToServerRpc(InputPayload input)
    //{
    //    server_ClientInputQueue.Enqueue(input);
    //}
    //#endregion
    #endregion

    #region Codigo Original
    //private Rigidbody2D rb;
    //[Header("Movement")]
    //[SerializeField] private float maxSpeed;
    //[SerializeField] private float acceleration;
    //[SerializeField] private float deceleration;

    //[Space]

    //[Header("Dash")]
    //[SerializeField] private float dashCooldown;
    //[SerializeField] private float dashDistance;
    //[SerializeField] private float dashDuration;


    //private float currentSpeed;
    //private Vector2 lastDirection;

    //private bool isDashing;
    //private float dashTimer;
    //private float nextDashTime;

    //private void OnEnable()
    //{
    //    InputManager.OnDashPerformed += StartDashServerAuth;
    //}

    //private void OnDisable()
    //{
    //    InputManager.OnDashPerformed -= StartDashServerAuth;
    //}

    //private void Awake()
    //{
    //    playerServerMovement = GetComponent<PlayerServerMovement>();
    //    rb = GetComponent<Rigidbody2D>();
    //}

    //private void Start()
    //{
    //    clientStateBuffer = new StatePayload[BUFFER_SIZE];
    //    clientInputBuffer = new InputPayload[BUFFER_SIZE];
    //}

    //private void FixedUpdate()
    //{
    //    if (!IsOwner) return;

    //    //MovePlayer(InputManager.moveDirection);
    //}

    //private void HandleTick()
    //{
    //    int bufferIndex = NetworkManager.Singleton.ServerTime.Tick % BUFFER_SIZE;

    //    InputPayload inputPayload = new InputPayload();
    //    inputPayload.tick = NetworkManager.Singleton.ServerTime.Tick;
    //    inputPayload.inputDirection = InputManager.moveDirection;
    //    clientInputBuffer[bufferIndex] = inputPayload;

    //    clientStateBuffer[bufferIndex] = MovePlayer(inputPayload);

    //    //Send input to server
    //    MovePlayerServerRpc(inputPayload);
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void MovePlayerServerRpc(InputPayload input)
    //{
    //    if (!isDashing)
    //    {
    //        if (input.inputDirection != Vector2.zero)
    //        {
    //            Accelerate();
    //            lastDirection = input.inputDirection;
    //        }
    //        else
    //        {
    //            Decelerate();
    //        }

    //        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

    //    }
    //    else
    //    {
    //        currentSpeed = dashDistance / dashDuration;
    //        isDashing = !(Time.time > dashTimer);
    //    }

    //    rb.MovePosition(transform.position + (Vector3)lastDirection * currentSpeed * Time.fixedDeltaTime);
    //}

    //private void StartDashServerAuth()
    //{
    //    StartDashServerRpc();
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void StartDashServerRpc()
    //{
    //    if (Time.time < nextDashTime) return;
    //    isDashing = true;
    //    nextDashTime = Time.time + dashCooldown;
    //    dashTimer = Time.time + dashDuration;
    //}

    //private StatePayload MovePlayer(InputPayload input)
    //{
    //    if (!isDashing)
    //    {
    //        if (input.inputDirection != Vector2.zero)
    //        {
    //            Accelerate();
    //            lastDirection = input.inputDirection;
    //        }
    //        else
    //        {
    //            Decelerate();
    //        }

    //        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

    //    }
    //    else
    //    {
    //        currentSpeed = dashDistance / dashDuration;
    //        isDashing = !(Time.time > dashTimer);
    //    }

    //    rb.MovePosition(transform.position + (Vector3)lastDirection * currentSpeed * Time.fixedDeltaTime);

    //    return new StatePayload()
    //    {
    //        tick = input.tick,
    //        position = rb.position
    //    };
    //}
    //private void StartDash()
    //{
    //    if (Time.time < nextDashTime) return;
    //    isDashing = true;
    //    nextDashTime = Time.time + dashCooldown;
    //    dashTimer = Time.time + dashDuration;
    //}


    //private void Accelerate()
    //{
    //    currentSpeed += Time.fixedDeltaTime * acceleration;
    //}

    //private void Decelerate()
    //{
    //    currentSpeed -= Time.fixedDeltaTime * deceleration;
    //}
    #endregion
}
