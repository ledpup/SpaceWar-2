using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ship : MonoBehaviour, ITargeting
{

    public GameObject Target { get; set; }
    Rigidbody _targetRigidBody;

    float _armour;
    Rigidbody _rigidbody;
    Text _speedText;
    Text _nameText;
    Text _armourText;
    float _lockedRotationUntil;
    float _fuel;
    Text _fuelText;
    Cannon _cannon;
    Quaternion _rotationOffset;
    List<string> _factions;
    void Start ()
    {
        // Because I'm using capsules with a 90 degree rotation on x
        _rotationOffset = Quaternion.Euler(90, 0, 0);

        _rigidbody = GetComponent<Rigidbody>();
        _armour = 10;
        _fuel = 100;

        _factions = UnityEditorInternal.InternalEditorUtility.tags.Where(x => x.StartsWith("Faction")).ToList();

        if (name.StartsWith("Player"))
        {

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

            var canvas = GameObject.Find("Canvas");
            _nameText = CreateTextElement(canvas, "Name", x, -20, anchorMin, anchorMax);
            _speedText = CreateTextElement(canvas, "Speed", x, -40, anchorMin, anchorMax);
            _armourText = CreateTextElement(canvas, "Armour", x, -60, anchorMin, anchorMax);
            _fuelText = CreateTextElement(canvas, "Fuel", x, -80, anchorMin, anchorMax);
            _nameText.text = name;
            _nameText.color = Faction.Colour(tag);
        }
        else
        {
            Target = Targeting.AquireTaget(tag, transform.position, _factions);
            _targetRigidBody = Target.GetComponent<Rigidbody>();
        }

        GetComponent<Renderer>().material.color = Faction.Colour(tag);

        _cannon = gameObject.GetComponentInChildren<Cannon>();

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
            float force = 0, turboForce = 0;
            if (name.StartsWith("Player"))
            {
                var horizontal = Input.GetAxis(name + "Horizontal");
                if (_lockedRotationUntil < Time.time)
                    _rigidbody.AddTorque(transform.forward * 5f * -horizontal);

                var vertical = Input.GetAxis(name + "Vertical");
                force = (vertical > 0 ? vertical : vertical * .8f) * 15f;

                var trigger1 = Input.GetAxis(name + "Trigger2");
                turboForce = trigger1 * 10f;
                
            }
            else if (Target != null)
            {
                float projectiveForce = 0;
                if (_cannon != null)
                    projectiveForce = _cannon.ShotForce;

                var shotMass = 1f;
                float speed = (projectiveForce / shotMass) * Time.fixedDeltaTime + _rigidbody.velocity.magnitude;
                var projectileSpeed = speed;

                var interceptPoint = PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, projectileSpeed, Target.transform.position, _targetRigidBody.velocity);
                var direction = interceptPoint - transform.position;
                var rotation = Quaternion.LookRotation(direction) * _rotationOffset;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.fixedDeltaTime);

                force = 7f;
            }
            else
            {
                Target = Targeting.AquireTaget(tag, transform.position, _factions);
                _targetRigidBody = Target.GetComponent<Rigidbody>();
            }

            _rigidbody.AddRelativeForce(Vector3.up * force);
            _rigidbody.AddRelativeForce(Vector3.up * turboForce);
            var forceApplied = force + turboForce;
            _fuel -= (forceApplied * .001f) + 0.001f;
        }

        if (_speedText != null)
            _speedText.text = "Speed " + Math.Round(_rigidbody.velocity.magnitude * 10, 0);
        //_headingText.text = "Heading " + Math.Round(360 - transform.eulerAngles.y, 0);
        if (_fuelText != null)
        {
            _fuelText.text = "Fuel " + Math.Round(_fuel, 0);
            if (_fuel < 20f)
                _fuelText.color = Color.red;
        }
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
        if (_armourText != null)
            _armourText.text = "Armour " + Mathf.RoundToInt(_armour * 10).ToString();
    }
}
