using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidpointCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform virtualMouse;
    [SerializeField] private Vector2 minCameraPosition;
    [SerializeField] private Vector2 maxCameraPosition;
    [SerializeField] private float smoothing = 5.0f;

    void FixedUpdate()
    {
        Vector3 midpoint = (target.position + (Vector3)virtualMouse.position) / 2.0f;
        midpoint.z = transform.position.z;

        midpoint.x = Mathf.Clamp(midpoint.x, minCameraPosition.x + target.position.x, maxCameraPosition.x + target.position.x);
        midpoint.y = Mathf.Clamp(midpoint.y, minCameraPosition.y + target.position.y, maxCameraPosition.y + target.position.y);

        transform.position = Vector3.Lerp(transform.position, midpoint, smoothing * Time.deltaTime);
    }
}
