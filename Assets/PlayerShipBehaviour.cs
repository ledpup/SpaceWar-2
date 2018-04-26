using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShipBehaviour : MonoBehaviour {

    // Use this for initialization
    float heading;
    Rigidbody _rigidbody;
    public Text Speed;
    public Text Heading;
    void Start () {
        _rigidbody = GetComponent<Rigidbody>();
    }
	
	void FixedUpdate() {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var speed = (vertical > 0 ? vertical : vertical * .6f) * 20f;

        var sign = speed >= 0 ? 1 : -1;

        heading += sign * (horizontal * 5) * Time.deltaTime;

        _rigidbody.AddRelativeForce(Vector3.up * speed);
        transform.eulerAngles = new Vector3(0, 0, RadianToDegree(-heading));

        Speed.text = "Speed " + Math.Round(_rigidbody.velocity.magnitude, 1);
        Heading.text = "Heading " + Math.Round(360 - transform.eulerAngles.z, 0);
    }

    private double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    private float RadianToDegree(float angle)
    {
        return (float)(angle * (180.0 / Math.PI));
    }
}
