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
    public GameObject TargetPoint;
    public float RotationRate;

    Rigidbody _rigidbody;
    MissleType _missleType;
    GameObject _target;
    float _created;

    void Start ()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _created = Time.time;

        RotationRate = .95f;
    }

    void FixedUpdate()
    {
        var collider = GetComponent<CapsuleCollider>();
        if (_created + .25F < Time.time && !collider.enabled)
        {
            collider.enabled = true;
        }

        var projectileSpeed = (5 / ((float)Math.Round(Time.time - _created, 2)) + 50);

        switch (_missleType)
        {
            case MissleType.Homing:
                {
                    if (_created + .75f < Time.time && _target != null)
                    {
                        var direction = _target.transform.position - transform.position;
                        var rotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationRate);
                    }
                    break;
                }
            case MissleType.Smart:
                {
                    if (_created + .75f < Time.time && _target != null)
                    {
                        var targetRigidBody = _target.GetComponent<Rigidbody>();

                        var targetAimPoint = Assets.PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, projectileSpeed, _target.transform.position, targetRigidBody.velocity);

                        var direction = targetAimPoint - transform.position;
                        var rotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationRate);
                    }
                    break;
                }
        }

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
        }
    }
}
