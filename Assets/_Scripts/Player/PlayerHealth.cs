using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if(currentHealth - damage > 0)
        {
            currentHealth -= damage;
        }
        else
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Revive()
    {
        gameObject.SetActive(true);
        currentHealth = maxHealth;
    }

    private void Die()
    {
        Invoke("Revive", 2f);
        gameObject.SetActive(false);
    }
    
}
