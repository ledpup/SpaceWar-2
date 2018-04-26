using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEmitterBehaviour : MonoBehaviour
{
    public GameObject BulletEmitter;
    public GameObject Bullet;
    public float BulletForce;
    public float FiringRate;
    float nextFire;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(name + "Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + FiringRate;

            var bullet = Instantiate(Bullet, BulletEmitter.transform.position, BulletEmitter.transform.rotation) as GameObject;

            var bulletRigidBody = bullet.GetComponent<Rigidbody>();
            var shipRigidBody = GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity;
            bulletRigidBody.AddForce(transform.up * BulletForce);

            Destroy(bullet, 3.0f);
        }
    }
}
