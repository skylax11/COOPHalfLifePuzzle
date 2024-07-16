using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour , IDamagable
{
    private int _health;
    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            value = _health;
        }
    }
    public void TakeDamage(int damage)
    {
        _health -= damage;
        print(_health);
    }
}
