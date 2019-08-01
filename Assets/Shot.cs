using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{
    [SerializeField] float ShotLifeTime = 3f;
    float _age;
    void Start ()
    {
    }
	
	void Update ()
    {
        _age += Time.deltaTime;
        if (_age > ShotLifeTime)
            Destroy(gameObject);
	}

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (!contact.otherCollider.gameObject.name.Contains("wall"))
            {
                Destroy(contact.thisCollider.gameObject);
            }

            var parent = collision.contacts[0].otherCollider.transform.parent;
            var armouredObject = parent == null ? collision.contacts[0].otherCollider.GetComponent<ArmouredObject>() : parent.GetComponent<ArmouredObject>();

            if (armouredObject != null)
            {
                var thisRigidbody = contact.thisCollider.attachedRigidbody;
                var otherRigidbody = contact.otherCollider.attachedRigidbody;

                var damage = (thisRigidbody.velocity - otherRigidbody.velocity).magnitude / 4f;
                armouredObject.TakeDamage(damage, thisRigidbody.velocity / Time.fixedDeltaTime, parent != null, collision.contacts[0].otherCollider.gameObject.name);
            }
        }
    }
}
