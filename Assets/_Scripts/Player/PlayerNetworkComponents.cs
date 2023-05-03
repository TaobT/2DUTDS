using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkComponents : NetworkBehaviour
{
    [Header("Delete On Other Players")]
    [SerializeField] private List<GameObject> componentsToDelete = new List<GameObject>();

    private void Start()
    {
        if (!IsOwner)
        {
            foreach (GameObject g in componentsToDelete)
            {
                Destroy(g);
            }
        }
    }
}
