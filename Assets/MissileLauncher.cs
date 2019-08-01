using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour, IComponent
{

    float _armour = 1;

    void Start ()
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
