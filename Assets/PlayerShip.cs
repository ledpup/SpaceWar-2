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
    float _fuel;
    Text _fuelText;
    void Start ()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _heading = -DegreeToRadian(transform.eulerAngles.y);
        _armour = 10;
        _fuel = 100;

        Vector2 anchorMin = Vector2.zero, anchorMax = Vector2.zero;
        int x = 0;
        if (name == "Player1")
        {
            anchorMin = new Vector2(0, 1);
            anchorMax = new Vector2(0, 1);
            x = 20;
        }
        else if (name == "Player2")
        {
            anchorMin = new Vector2(1, 1);
            anchorMax = new Vector2(1, 1);
            x = -100;
        }
        GetComponent<Renderer>().material.color = Faction.Colour(tag);

        var canvas = GameObject.Find("Canvas");
        _speed = CreateTextElement(canvas, "Speed", x, -20, anchorMin, anchorMax);
        _headingText = CreateTextElement(canvas, "Heading", x, -40, anchorMin, anchorMax);
        _armourText = CreateTextElement(canvas, "Armour", x, -60, anchorMin, anchorMax);
        _fuelText = CreateTextElement(canvas, "Fuel", x, -80, anchorMin, anchorMax);

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
        if (_fuel > 0)
        {
            var horizontal = Input.GetAxis(name + "Horizontal");
            var vertical = Input.GetAxis(name + "Vertical");

            var force = (vertical > 0 ? vertical : vertical * .6f) * 15f;

            var sign = force >= 0 ? 1 : -1;

            if (_lockedRotationUntil < Time.time)
                _heading -= sign * (horizontal * 5) * Time.deltaTime;

            transform.eulerAngles = new Vector3(90, RadianToDegree(-_heading), 0);
            _rigidbody.AddRelativeForce(Vector3.up * force);

            var trigger1 = Input.GetAxis(name + "Trigger2");
            var turboForce = trigger1 * 10f;
            _rigidbody.AddRelativeForce(Vector3.up * turboForce);

            var forceApplied = force + turboForce;

            _fuel -= (forceApplied * .001f) + 0.001f;
        }

        _speed.text = "Speed " + Math.Round(_rigidbody.velocity.magnitude * 10, 0);
        _headingText.text = "Heading " + Math.Round(360 - transform.eulerAngles.y, 0);
        _fuelText.text = "Fuel " + Math.Round(_fuel, 0);
        if (_fuel < 20f)
            _fuelText.color = Color.red;
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
                //var b = rigidbody.velocity.magnitude;

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
