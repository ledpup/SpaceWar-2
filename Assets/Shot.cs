using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour {

	void Start ()
    {
        
    }
	
	// Update is called once per frame
	void Update () {
        //var mag = transform.GetComponent<Rigidbody>().velocity.magnitude;
	}

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (!contact.otherCollider.gameObject.name.Contains("wall"))
            {
                Destroy(contact.thisCollider.gameObject);
            }
        }
    }
}
