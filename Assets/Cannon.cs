using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour, IComponent
{
    float _armour = 3;

    void Start()
    {
    }

    public bool TakeDamage(float amount)
    {
        _armour -= amount;

        if (_armour <= 0)
        {
            _armour = 0;
        }
        return _armour <= 0;
    }
}
