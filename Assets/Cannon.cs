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

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var parent = transform.parent;

        if (Input.GetButton(parent.name + "Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + FiringRate;

            var shot = Instantiate(Shot, transform.position, transform.rotation) as GameObject;

            var bulletRigidBody = shot.GetComponent<Rigidbody>();
            var shipRigidBody = parent.GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity; // Base shot speed on the ship's velocity
            bulletRigidBody.AddForce(transform.up * ShotForce);
            shipRigidBody.AddForce(transform.up * (-ShotForce * .1f)); // Unrealistic recoil (for fun!)

            Destroy(shot, 3.0f);
        }
    }
}
