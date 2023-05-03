using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BasicSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPf;

    [SerializeField] private Transform spawnPositionA;
    [SerializeField] private Transform spawnPositionB;

    private bool nextSpawnB;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += ServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void ServerStarted()
    {
        SpawnPlayerServerRpc(NetworkManager.LocalClientId);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsOwner) return;
        SpawnPlayerServerRpc(clientId);
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(ulong clientId)
    {
        if (!nextSpawnB)
        {
            GameObject player = Instantiate(playerPf, spawnPositionA.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        }
        else
        {
            GameObject player = Instantiate(playerPf, spawnPositionB.position, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        }
        nextSpawnB = !nextSpawnB;
    }

}
