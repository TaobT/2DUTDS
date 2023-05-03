using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PlayerInputActions playerActions;

    public static Vector2 moveDirection;

    //Mouse
    public static Vector2 mouseDelta;

    //Movement
    public static event UnityAction OnDashPerformed;

    //Fire
    public static event UnityAction OnFirePerformed;
    public static event UnityAction OnFireCanceled;

    public static event UnityAction OnReloadPerformed;

    private void Awake()
    {
        playerActions = new PlayerInputActions();
        playerActions.Keyboard.Enable();
    }

    private void OnEnable()
    {
        //Movement
        playerActions.Keyboard.Move.performed += OnMove;
        playerActions.Keyboard.Move.canceled += OnMove;

        playerActions.Keyboard.Dash.performed += OnDash;

        //Mouse
        playerActions.Keyboard.MouseDelta.performed += OnMouseDelta;
        playerActions.Keyboard.MouseDelta.canceled += OnMouseDelta;

        //Fire
        playerActions.Keyboard.Fire.performed += OnFire;
        playerActions.Keyboard.Fire.canceled += OnFire;

        playerActions.Keyboard.Reload.performed += OnReload;
    }

    private void OnDisable()
    {
        //Movement
        playerActions.Keyboard.Move.performed -= OnMove;
        playerActions.Keyboard.Move.canceled -= OnMove;

        playerActions.Keyboard.Dash.performed -= OnDash;

        //Mouse
        playerActions.Keyboard.MouseDelta.performed -= OnMouseDelta;
        playerActions.Keyboard.MouseDelta.canceled -= OnMouseDelta;

        //Fire
        playerActions.Keyboard.Fire.performed -= OnFire;
        playerActions.Keyboard.Fire.canceled -= OnFire;

        playerActions.Keyboard.Reload.performed -= OnReload;
    }

    #region Movement
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveDirection = ctx.ReadValue<Vector2>();
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnDashPerformed?.Invoke();
    }
    #endregion

    #region Mouse
    private void OnMouseDelta(InputAction.CallbackContext ctx)
    {
        mouseDelta = ctx.ReadValue<Vector2>();
    }
    #endregion

    #region Fire
    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnFirePerformed?.Invoke();
        if (ctx.canceled) OnFireCanceled?.Invoke();
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnReloadPerformed?.Invoke();
    }
    #endregion
}
