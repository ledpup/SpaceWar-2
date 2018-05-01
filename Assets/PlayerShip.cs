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
    Text _speed;
    Text _headingText;
    Text _armourText;
    float _lockedRotationUntil;

    void Start ()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _heading = -DegreeToRadian(transform.eulerAngles.z);

        Vector2 anchorMin = Vector2.zero, anchorMax = Vector2.zero;
        int x = 0;
        if (name == "Player1")
        {
            GetComponent<Renderer>().material.color = Color.green;
            anchorMin = new Vector2(0, 1);
            anchorMax = new Vector2(0, 1);
            x = 20;
        }
        else if (name == "Player2")
        {
            GetComponent<Renderer>().material.color = Color.red;
            anchorMin = new Vector2(1, 1);
            anchorMax = new Vector2(1, 1);
            x = -100;
        }

        var canvas = GameObject.Find("Canvas");
        _speed = CreateTextElement(canvas, "Speed", x, -20, anchorMin, anchorMax);
        _headingText = CreateTextElement(canvas, "Heading", x, -40, anchorMin, anchorMax);
        _armourText = CreateTextElement(canvas, "Armour", x, -60, anchorMin, anchorMax);

        _armour = 10;
        UpdateArmourText();

        _lockedRotationUntil = Time.time;
    }

    private Text CreateTextElement(GameObject canvas, string elementName, int x, int y, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name + elementName);
        go.transform.parent = canvas.transform;

        var text = go.AddComponent<Text>();

        var rectTransform = text.GetComponent<RectTransform>();

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.offsetMin = new Vector2(x, y - 30f);
        rectTransform.offsetMax = new Vector2(x + 160f, y);

        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;

        return text;
    }

    void FixedUpdate() {
        var horizontal = Input.GetAxis(name + "Horizontal");
        var vertical = Input.GetAxis(name + "Vertical");

        var speed = (vertical > 0 ? vertical : vertical * .6f) * 20f;

        var sign = speed >= 0 ? 1 : -1;

        if (_lockedRotationUntil < Time.time)
            _heading -= sign * (horizontal * 5) * Time.deltaTime;

        transform.eulerAngles = new Vector3(0, 0, RadianToDegree(-_heading));
        _rigidbody.AddRelativeForce(Vector3.up * speed);

        _speed.text = "Speed " + Math.Round(_rigidbody.velocity.magnitude * 10, 0);
        _headingText.text = "Heading " + Math.Round(360 - transform.eulerAngles.z, 0);
    }

    void LockRotation(float duration)
    {
        _lockedRotationUntil = Time.time + duration;
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

                var collider = collision.contacts[0].thisCollider;
                if (collider.name.StartsWith("Missile Launcher") || collider.name.StartsWith("Cannon"))
                {
                    // Change this later to damage child components rather than destroying them outright
                    Destroy(collider.gameObject);
                }
                else
                {
                    _armour -= rigidbody.velocity.magnitude / 5f;

                    if (_armour < 0)
                    {
                        Destroy(contact.thisCollider.gameObject);
                    }

                    UpdateArmourText();
                }
            }
        }
    }

    private void UpdateArmourText()
    {
        _armourText.text = "Armour " + Mathf.RoundToInt(_armour * 10).ToString();
    }
}
