using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Shot : NetworkBehaviour
{
    [SerializeField] float ShellLifeTime = 3f;

    float _age;
    void Start ()
    {
        
    }
	
	[ServerCallback]
	void Update ()
    {
        _age += Time.deltaTime;
        if (_age > ShellLifeTime)
            NetworkServer.Destroy(gameObject);
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
