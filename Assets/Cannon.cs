using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    //public GameObject CannonObject;
    public GameObject Shot;
    public float ShotForce;
    public float FiringRate;
    float nextFire;
    bool _fireButtonValid;
    // Use this for initialization
    void Start()
    {
        var parent = transform.parent;
        try
        {
            _fireButtonValid = Input.GetButton(parent.name + "Fire1");
        }
        catch
        {
            _fireButtonValid = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_fireButtonValid && Input.GetButton(transform.parent.name + "Fire1"))
        {
            FireCannon();
        }
    }

    internal void FireCannon()
    {
        if (Time.time > nextFire)
        {
            nextFire = Time.time + FiringRate;

            var shot = Instantiate(Shot, transform.position, transform.rotation) as GameObject;

            var bulletRigidBody = shot.GetComponent<Rigidbody>();
            var shipRigidBody = transform.parent.GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity; // Base shot speed on the ship's velocity
            bulletRigidBody.AddForce(transform.up * ShotForce);
            shipRigidBody.AddForce(transform.up * (-ShotForce * .1f)); // Unrealistic recoil (for fun!)

            Destroy(shot, 3.0f);
        }
    }
}
