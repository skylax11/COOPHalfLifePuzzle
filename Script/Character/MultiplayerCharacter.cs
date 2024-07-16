using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerCharacter : NetworkBehaviour , IDamagable
{
    private int _health = 100;
    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
        }
    }
    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
    
}
