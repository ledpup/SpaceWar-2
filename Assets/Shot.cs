using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Shot : NetworkBehaviour
{
    [SerializeField] float ShotLifeTime = 3f;
    public float Armour { get; set; }

    float _age;
    void Start ()
    {
        Armour = 1;
    }
	
	[ServerCallback]
	void Update ()
    {
        _age += Time.deltaTime;
        if (_age > ShotLifeTime)
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

            if (!isServer)
            {
                return;
            }

            var parent = collision.contacts[0].otherCollider.transform.parent;

            var armouredObject = parent == null ? collision.contacts[0].otherCollider.GetComponent<ArmouredObject>() : parent.GetComponent<ArmouredObject>();

            if (armouredObject != null)
            {
                var thisRigidbody = contact.thisCollider.attachedRigidbody;
                var otherRigidbody = contact.otherCollider.attachedRigidbody;

                var damage = (thisRigidbody.velocity - otherRigidbody.velocity).magnitude / 2f;
                armouredObject.TakeDamage(damage, thisRigidbody.velocity / Time.fixedDeltaTime, parent != null, collision.contacts[0].otherCollider.gameObject.name);
            }
        }
    }
}
