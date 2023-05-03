using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerVirtualMouse : NetworkBehaviour
{
    [SerializeField][Range(0f, 0.02f)] private float sensibility;
    [Header("Virtual Mouse Bounds")]
    [SerializeField] private Vector2 minVirtualMousePosition;
    [SerializeField] private Vector2 maxVirtualMousePosition;
    [SerializeField] private Transform anchor; //Player position
    private void Start()
    {
        if (!IsOwner) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        VirtualMouseMovement();
    }

    private void VirtualMouseMovement()
    {
        Vector3 movement = transform.position + (Vector3)InputManager.mouseDelta * sensibility;
        movement.x = Mathf.Clamp(movement.x, minVirtualMousePosition.x + anchor.position.x, maxVirtualMousePosition.x + anchor.position.x);
        movement.y = Mathf.Clamp(movement.y, minVirtualMousePosition.y + anchor.position.y, maxVirtualMousePosition.y + anchor.position.y);
        transform.position = movement;
        transform.rotation = Quaternion.Euler(transform.parent.GetChild(0).eulerAngles + new Vector3(0, 0, -90f));
    }
}
