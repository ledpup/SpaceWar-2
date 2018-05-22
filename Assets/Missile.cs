using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum MissleType
{
    Dumbfire,
    Guided,
    Homing,
    Smart,
}
public class Missile : NetworkBehaviour
{
    public float RotationRate = .25f;

    Rigidbody _rigidbody;
    MissleType _missleType;
    GameObject _target;
    [SerializeField] float MissileLifeTime = 10f;
    float _age;

    void Start ()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }


    [ServerCallback]
    void Update()
    {
        _age += Time.deltaTime;
        if (_age > MissileLifeTime)
            NetworkServer.Destroy(gameObject);

        var collider = GetComponent<CapsuleCollider>();
        if (_age > .5f && !collider.enabled)
        {
            collider.enabled = true;
        }

        var projectileSpeed = (5 / ((float)Math.Round(Time.time - _age, 2)) + 50);

        switch (_missleType)
        {
            case MissleType.Homing:
                {
                    if (_age > .75f && _target != null)
                    {
                        var direction = _target.transform.position - transform.position;
                        var rotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationRate);
                    }
                    break;
                }
            case MissleType.Smart:
                {
                    if (_age > .75f && _target != null)
                    {
                        var targetRigidBody = _target.GetComponent<Rigidbody>();

                        var targetAimPoint = PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, projectileSpeed, _target.transform.position, targetRigidBody.velocity);

                        var direction = targetAimPoint - transform.position;
                        var rotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationRate);
                    }
                    break;
                }
        }
    }

    private void FixedUpdate()
    {
        var projectileSpeed = (5 / ((float)Math.Round(Time.time - _age, 2)) + 50);
        _rigidbody.AddRelativeForce(Vector3.forward * projectileSpeed);
    }

    void SetMissileType(MissleType missleType)
    {
        _missleType = missleType;
    }

    void SetTarget(string target)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var gameObject in allObjects)
        {
            if (gameObject.name == target)
            {
                _target = gameObject;
                
            }
        }
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

                var damage = (thisRigidbody.velocity - otherRigidbody.velocity).magnitude / 4f;
                armouredObject.TakeDamage(damage, thisRigidbody.velocity / Time.fixedDeltaTime, parent != null, collision.contacts[0].otherCollider.gameObject.name);
            }
        }
    }
}
