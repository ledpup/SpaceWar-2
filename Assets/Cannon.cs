using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Cannon : NetworkBehaviour, IComponent
{
    //public GameObject CannonObject;
    public GameObject Shot;
    public float ShotForce;
    public float FiringRate;
    float nextFire;
    bool _manualFireButtonValid;
    // Use this for initialization
    [SyncVar] float _armour;

    void Start()
    {
        var ni = transform.GetComponent<NetworkIdentity>();
        if (ni != null && !isLocalPlayer)
        {
            Destroy(this);
            return;
        }

        var parent = transform.parent;
        try
        {
            GetButton(parent.name, "Fire1");
            
            _manualFireButtonValid = true;
        }
        catch (Exception ex)
        {
            _manualFireButtonValid = false;
        }
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


    private bool GetButton(string controllerName, string buttonName)
    {
        controllerName = controllerName.Replace("(Clone)", "");
        return Input.GetButton(controllerName + buttonName);
    }

    // Update is called once per frame
    void Update()
    {
        if (_manualFireButtonValid && GetButton(transform.parent.name, "Fire1"))
        {
            CmdFireCannon();
        }
        else
        {
            var targetingParent = transform.parent.GetComponentInParent<ITargeting>();
            if (targetingParent != null && targetingParent.Target != null)
            {
                var distanceToTarget = Vector3.Distance(targetingParent.Target.transform.position, transform.position);
                if (distanceToTarget < 15)
                {
                    CmdFireCannon();
                }
            }
        }
    }

    [Command]
    internal void CmdFireCannon()
    {
        if (Time.time > nextFire)
        {
            nextFire = Time.time + FiringRate;

            var shot = Instantiate(Shot, transform.position, transform.rotation) as GameObject;

            var shotRigidBody = shot.GetComponent<Rigidbody>();

            var shipRigidBody = transform.parent.GetComponent<Rigidbody>();

            shotRigidBody.velocity = shipRigidBody.velocity;
            shotRigidBody.AddForce(transform.forward * ShotForce);
            shipRigidBody.AddForce(transform.forward * (-ShotForce * .1f)); // Unrealistic recoil (for fun!)

            NetworkServer.Spawn(shot);
        }
    }
}
