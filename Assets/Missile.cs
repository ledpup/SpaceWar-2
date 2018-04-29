using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MissleType
{
    Dumbfire,
    Guided,
    Homing,
    Smart,
}
public class Missile : MonoBehaviour
{
    public GameObject TargetPoint;
    float _heading;
    Rigidbody _rigidbody;
    MissleType _missleType;
    GameObject _target;
    float _created;

    GameObject _targetPoint;

    void Start () {
        _rigidbody = GetComponent<Rigidbody>();
        _heading = -PlayerShip.DegreeToRadian(transform.eulerAngles.z);
        _created = Time.time;

        if (_missleType == MissleType.Smart)
        {
            _targetPoint = Instantiate(TargetPoint, transform.position, transform.rotation) as GameObject;
            _targetPoint.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void FixedUpdate()
    {
        var collider = GetComponent<CapsuleCollider>();
        if (_created + .5F < Time.time && !collider.enabled)
        {
            collider.enabled = true;
        }

        var projectileSpeed = (5 / ((float)Math.Round(Time.time - _created, 2)) + 75);

        switch (_missleType)
        {
            case MissleType.Guided:
                var horizontal = Input.GetAxis(tag + "Horizontal2");
                _heading += (horizontal * 5) * Time.deltaTime;

                break;
            case MissleType.Homing:
                {
                    if (_created + .75f < Time.time && _target != null)
                    {
                        float leftHeading, rightHeading, leftDistance, rightDistance;

                        leftHeading = _heading - .2f;
                        rightHeading = _heading + .2f;

                        DetermineTurnDirection(leftHeading, rightHeading, out leftDistance, out rightDistance, _target.transform.position);

                        if (leftDistance < rightDistance)
                            _heading = leftHeading;
                        else if (leftDistance > rightDistance)
                            _heading = rightHeading;
                        // else stay on current heading
                    }
                    break;
                }
            case MissleType.Smart:
                {
                    if (_created + .75f < Time.time && _target != null)
                    {
                        var targetRigidBody = _target.GetComponent<Rigidbody>();

                        float distance = Vector3.Distance(transform.position, _target.transform.position);

                        Vector3 targetPosition = Assets.PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, projectileSpeed, _target.transform.position, targetRigidBody.velocity);

                        _targetPoint.transform.position = targetPosition;

                        float leftHeading, rightHeading, leftDistance, rightDistance;

                        leftHeading = _heading - .2f;
                        rightHeading = _heading + .2f;

                        DetermineTurnDirection(leftHeading, rightHeading, out leftDistance, out rightDistance, targetPosition);

                        if (leftDistance < rightDistance)
                            _heading = leftHeading;
                        else if (leftDistance > rightDistance)
                            _heading = rightHeading;
                        // else stay on current heading
                    }
                    break;
                }
        }

        transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-_heading));
        _rigidbody.AddRelativeForce(Vector3.up * projectileSpeed);
    }

    private void DetermineTurnDirection(float leftHeading, float rightHeading, out float leftDistance, out float rightDistance, Vector3 targetPosition)
    {
        var eulerAngles = transform.eulerAngles;
        var position = transform.position;

        transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-leftHeading));
        transform.Translate(Vector3.up * .5f);
        leftDistance = Vector3.Distance(targetPosition, transform.position);

        transform.eulerAngles = eulerAngles;
        transform.position = position;

        transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-rightHeading));
        transform.Translate(Vector3.up * .5f);

        rightDistance = Vector3.Distance(targetPosition, transform.position);

        transform.eulerAngles = eulerAngles;
        transform.position = position;
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
                if (_targetPoint != null)
                    Destroy(_targetPoint);
            }
        }
    }
}
