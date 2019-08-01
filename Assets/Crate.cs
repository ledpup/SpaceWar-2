using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Crate : MonoBehaviour
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
            var collectingVehicle = contact.otherCollider.GetComponent<Vehicle>();

            if (collectingVehicle == null && contact.otherCollider.transform.parent != null)
            {
                collectingVehicle = contact.otherCollider.transform.parent.GetComponent<Vehicle>();
            }
            if (collectingVehicle != null)
            {
                collectingVehicle.CollectCrate(_crateType, _quantity);
                Destroy(contact.thisCollider.gameObject);
            }
        }
    }
}
