using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour {

    public GameObject Missile;
    public float FiringRate;
    float _nextFire;
    
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        var parent = transform.parent;

        var fire2 = Input.GetButtonDown(parent.name + "Fire2");
        var fire3 = Input.GetButtonDown(parent.name + "Fire3");

        if ((fire2 || fire3) && Time.time > _nextFire)
        {
            _nextFire = Time.time + FiringRate;

            var missile = Instantiate(Missile, transform.position, transform.rotation) as GameObject;

            missile.tag = parent.name;
            missile.SendMessage("SetMissileType", fire2 ? MissleType.Guided : MissleType.Homing);

            if (fire3)
            {
                missile.SendMessage("SetTarget", "Player1");
            }

            var bulletRigidBody = missile.GetComponent<Rigidbody>();
            var shipRigidBody = parent.GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity; // Base speed on the ship's velocity

            Destroy(missile, fire3 ? 10f : 5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.gameObject.name.StartsWith("Shot") || contact.otherCollider.gameObject.name.StartsWith("Missile"))
            {
                Destroy(contact.thisCollider.gameObject);
            }
        }
    }
}
