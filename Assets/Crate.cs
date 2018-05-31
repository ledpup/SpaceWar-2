using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Crate : NetworkBehaviour
{
	public enum CrateType
    {
        Armour,
        Fuel,
        Missile,
        Shot,
        
        Cannon, // Qualatative
        MissileLauncher, // Qualatative
    }

    CrateType _crateType;
    int _quantity;

    void Start ()
    {
        var random = new System.Random();
        _crateType = (CrateType)random.Next(Enum.GetValues(typeof(CrateType)).Cast<int>().Max() - 2);
        _quantity = (int)_crateType < 5 ? random.Next(10, 30) : 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            var collectingShip = contact.otherCollider.GetComponent<Ship>();

            if (collectingShip == null && contact.otherCollider.transform.parent != null)
            {
                collectingShip = contact.otherCollider.transform.parent.GetComponent<Ship>();
            }
            if (collectingShip != null)
            {
                var networkdIdentity = collectingShip.GetComponent<NetworkIdentity>();
                collectingShip.RpcCollectCrate(_crateType, _quantity);
                Destroy(contact.thisCollider.gameObject);
            }
        }
    }
}
