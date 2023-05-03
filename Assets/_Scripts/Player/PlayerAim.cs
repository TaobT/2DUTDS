using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAim : NetworkBehaviour
{
    [SerializeField] private Transform virtualMouse;

    private void Update()
    {
        if (!IsOwner) return;
        LookAtMouse(virtualMouse.localPosition);
    }

    private void LookAtMouse(Vector2 virtualMousePosition)
    {
        // Get Angle in Radians
        float AngleRad = Mathf.Atan2(virtualMousePosition.y - transform.localPosition.y, virtualMousePosition.x - transform.localPosition.x);
        // Get Angle in Degrees
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        // Rotate Object
        transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
    }
}
