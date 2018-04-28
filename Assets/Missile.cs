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

    float _heading;
    Rigidbody _rigidbody;
    MissleType _missleType;
    GameObject _target;
    float _created;

    void Start () {
        _rigidbody = GetComponent<Rigidbody>();
        _heading = -PlayerShip.DegreeToRadian(transform.eulerAngles.z);
        _created = Time.time;
    }

    void FixedUpdate()
    {
        var collider = GetComponent<CapsuleCollider>();
        if (_created + .5F < Time.time && !collider.enabled)
        {
            collider.enabled = true;
        }
        switch (_missleType)
        {
            case MissleType.Guided:
                var horizontal = Input.GetAxis(tag + "Horizontal2");
                _heading += (horizontal * 5) * Time.deltaTime;

                break;
            case MissleType.Homing:

                if (_created + .75f < Time.time && _target != null)
                {
                    var eulerAngles = transform.eulerAngles;
                    var position = transform.position;

                    var leftHeading = _heading - .1f;
                    var rightHeading = _heading + .1f;

                    transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-leftHeading));
                    transform.Translate(Vector3.up * .25f);
                    var leftDistance = Vector3.Distance(_target.transform.position, transform.position);

                    transform.eulerAngles = eulerAngles;
                    transform.position = position;

                    transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-rightHeading));
                    transform.Translate(Vector3.up * .25f);

                    var rightDistance = Vector3.Distance(_target.transform.position, transform.position);

                    transform.eulerAngles = eulerAngles;
                    transform.position = position;

                    if (leftDistance < rightDistance)
                        _heading = leftHeading;
                    else if (leftDistance > rightDistance)
                        _heading = rightHeading;
                    // else stay on current heading
                }
                break;
        }

        transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-_heading));
        _rigidbody.AddRelativeForce(Vector3.up * 40);
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
