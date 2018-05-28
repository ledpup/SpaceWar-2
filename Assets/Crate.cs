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
        Fuel,
        Shot,
        Missile,
        Cannon,
        MissileLauncher,
    }

    CrateType _crateType;
    int _quantity;

    void Start ()
    {
        var random = new System.Random();
        _crateType = (CrateType)random.Next(Enum.GetValues(typeof(CrateType)).Cast<int>().Max());
        _quantity = (int)_crateType < 4 ? random.Next(0, 10) * 10 : 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            var collectingShip = contact.otherCollider.GetComponent<Ship>();

            if (collectingShip != null)
            {
                CmdCollectCrate(contact.otherCollider.gameObject);
                Destroy(contact.thisCollider.gameObject);
            }
        }
    }

    [Command]
    internal void CmdCollectCrate(GameObject collectingShip)
    {
        //if (isLocalPlayer)
        {
            collectingShip.GetComponent<Ship>().Ammo[AmmoType.Cannon] += 10;
        }
    }
}
