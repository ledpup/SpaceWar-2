﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
           if (contact.otherCollider.gameObject.name.StartsWith("Bullet"))
            {
                Destroy(contact.otherCollider.gameObject);
            }
        }
    }
}
