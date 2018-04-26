using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedMissileLauncher : MonoBehaviour {

    public GameObject GuidedMissile;
    public float FiringRate;
    float nextFire;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var parent = transform.parent;

        if (Input.GetButtonDown(parent.name + "Fire2") && Time.time > nextFire)
        {
            nextFire = Time.time + FiringRate;

            var missile = Instantiate(GuidedMissile, transform.position, transform.rotation) as GameObject;

            missile.tag = parent.name;

            var bulletRigidBody = missile.GetComponent<Rigidbody>();
            var shipRigidBody = parent.GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity; // Base speed on the ship's velocity

            Destroy(missile, 3.0f);
        }
    }
}
