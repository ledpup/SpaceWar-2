using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour {

    // Use this for initialization
    float heading;
    public new Rigidbody rigidbody;
    public Text Speed;
    public Text Heading;
    void Start () {
        rigidbody = GetComponent<Rigidbody>();
    }
	
	void FixedUpdate() {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var speed = (vertical > 0 ? vertical : vertical * .8f) * 20f;

        var sign = Math.Sign(speed);
        sign = sign == 0 ? 1 : sign;

        heading += sign * (horizontal * 5) * Time.deltaTime;

        rigidbody.AddRelativeForce(Vector3.up * speed);
        transform.eulerAngles = new Vector3(0, 0, RadianToDegree(-heading));

        Speed.text = "Speed " + Math.Round(rigidbody.velocity.magnitude, 1);
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
