using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {

    float _heading;
    Rigidbody _rigidbody;

    void Start () {
        _rigidbody = GetComponent<Rigidbody>();
        _heading = -PlayerShip.DegreeToRadian(transform.eulerAngles.z);
    }

    void FixedUpdate()
    {
        var horizontal = Input.GetAxis(tag + "Horizontal2");

        _heading += (horizontal * 5) * Time.deltaTime;

        _rigidbody.AddRelativeForce(Vector3.up * 30);
        transform.eulerAngles = new Vector3(0, 0, PlayerShip.RadianToDegree(-_heading));

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
