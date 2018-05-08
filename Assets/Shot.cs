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

            var damageableObject = collision.contacts[0].otherCollider.GetComponent<ArmouredObject>();

            if (damageableObject != null)
            {
                var thisRigidbody = contact.thisCollider.GetComponent<Rigidbody>();
                var otherRigidbody = contact.otherCollider.GetComponent<Rigidbody>();

                var damage = (thisRigidbody.velocity - otherRigidbody.velocity).magnitude / 5f;
                damageableObject.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        Armour -= damage;
        if (Armour < 0)
            Destroy(gameObject);
    }
}
