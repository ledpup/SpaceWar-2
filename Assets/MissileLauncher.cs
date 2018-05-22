using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MissileLauncher : NetworkBehaviour, IComponent
{

    [SyncVar] float _armour = 1;

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
