﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MissileLauncher : NetworkBehaviour, IComponent
{

    public GameObject Missile;
    public float FiringRate;
    float _nextFire;

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


    void Update () {
        var parent = transform.parent;

        var controllerName = parent.name.Replace("(Clone)", "");

        var fire2 = Input.GetButtonDown(controllerName + "Fire2");
        var fire3 = Input.GetButtonDown(controllerName + "Fire3");

        if ((fire2 || fire3) && Time.time > _nextFire)
        {
            _nextFire = Time.time + FiringRate;

            transform.parent.gameObject.SendMessage("LockRotation", .5);

            var missile = Instantiate(Missile, transform.position, transform.rotation) as GameObject;

            missile.tag = controllerName;
            missile.SendMessage("SetMissileType", fire2 ? MissleType.Smart : MissleType.Homing);

            missile.SendMessage("SetTarget", "Player1");

            var bulletRigidBody = missile.GetComponent<Rigidbody>();
            var shipRigidBody = parent.GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity; // Base speed on the ship's velocity

            Destroy(missile, fire3 ? 10f : 10f);
        }
    }
}
