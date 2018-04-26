using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviour {

    // Use this for initialization
    float _heading;
    float _armour;
    Rigidbody _rigidbody;
    public Text Speed;
    public Text Heading;
    public Text Armour;
    void Start () {
        _rigidbody = GetComponent<Rigidbody>();

        if (name == "Player1")
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.red;
        }

        _armour = 10;
        if (Armour != null)
            Armour.text = "Armour " + _armour.ToString();
    }
	
	void FixedUpdate() {
        var horizontal = Input.GetAxis(name + "Horizontal");
        var vertical = Input.GetAxis(name + "Vertical");

        var speed = (vertical > 0 ? vertical : vertical * .6f) * 20f;

        var sign = speed >= 0 ? 1 : -1;

        _heading += sign * (horizontal * 5) * Time.deltaTime;

        _rigidbody.AddRelativeForce(Vector3.up * speed);
        transform.eulerAngles = new Vector3(0, 0, RadianToDegree(-_heading));

        if (Speed != null)
            Speed.text = "Speed " + Math.Round(_rigidbody.velocity.magnitude * 10, 0);

        if (Heading != null)
            Heading.text = "Heading " + Math.Round(360 - transform.eulerAngles.z, 0);
        
    }

    public static float DegreeToRadian(float angle)
    {
        return (float)(Math.PI * angle / 180.0);
    }

    public static float RadianToDegree(float angle)
    {
        return (float)(angle * (180.0 / Math.PI));
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.otherCollider.gameObject.name.StartsWith("Shot") || contact.otherCollider.gameObject.name.StartsWith("Missile"))
            {
                var rigidbody = contact.otherCollider.GetComponent<Rigidbody>();
                var b = rigidbody.velocity.magnitude;

                _armour -= rigidbody.velocity.magnitude / 8.5f;

                if (_armour < 0)
                {
                    Destroy(contact.thisCollider.gameObject);
                }

                if (Armour != null)
                    Armour.text = "Armour " + _armour.ToString();
            }
        }
    }
}
