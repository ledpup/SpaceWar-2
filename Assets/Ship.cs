using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Ship : NetworkBehaviour, ITargeting
{

    public GameObject Target { get; set; }
    Rigidbody _targetRigidBody;

    public bool AddForce;
    public bool AddTorque = true;

    float _armour;
    Rigidbody _rigidbody;
    Text _speedText;
    Text _nameText;
    Text _armourText;
    float _lockedRotationUntil;
    float _fuel;
    Text _fuelText;
    Cannon _cannon;
    List<string> _factions;
    List<GameObject> _players;
    string _controllerName;

    public GameObject Shot;
    public float ShotForce = 500f;
    public float FiringRate = .3f;
    float _nextFire;
    bool _manualFireButtonValid;
    void Start ()
    {
        var ni = transform.GetComponent<NetworkIdentity>();
        if (ni != null && !isLocalPlayer)
        {
            Destroy(this);
            return;
        }

        _rigidbody = GetComponent<Rigidbody>();
        _armour = 10;
        _fuel = 100;

        _factions = new List<string> { "Faction1", "Faction2", "Faction3" };
        _players = new List<GameObject>();
        _factions.ForEach(x => _players.AddRange(GameObject.FindGameObjectsWithTag(x)));

        _controllerName = name.Replace("(Clone)", "");

        if (_controllerName.StartsWith("Player"))
        {

            Vector2 anchorMin = Vector2.zero, anchorMax = Vector2.zero;
            int x = 0;
            if (_controllerName == "Player1")
            {
                anchorMin = new Vector2(0, 1);
                anchorMax = new Vector2(0, 1);
                x = 20;
            }
            else if (_controllerName == "Player2")
            {
                anchorMin = new Vector2(1, 1);
                anchorMax = new Vector2(1, 1);
                x = -100;
            }

            var canvas = GameObject.Find("Canvas");
            _nameText = CreateTextElement(canvas, _controllerName, "Name", x, -20, anchorMin, anchorMax);
            _speedText = CreateTextElement(canvas, _controllerName, "Speed", x, -40, anchorMin, anchorMax);
            _armourText = CreateTextElement(canvas, _controllerName, "Armour", x, -60, anchorMin, anchorMax);
            _fuelText = CreateTextElement(canvas, _controllerName, "Fuel", x, -80, anchorMin, anchorMax);
            _nameText.text = _controllerName;
            _nameText.color = Faction.Colour(tag);
        }
        else
        {
            AquireTarget();
        }

        GetComponent<Renderer>().material.color = Faction.Colour(tag);

        _cannon = gameObject.GetComponentInChildren<Cannon>();

        UpdateArmourText();

        _lockedRotationUntil = Time.time;


        try
        {
            Input.GetButton(_controllerName + "Fire1");

            _manualFireButtonValid = true;
        }
        catch (Exception ex)
        {
            _manualFireButtonValid = false;
        }
    }

    private Text CreateTextElement(GameObject canvas, string controllerName, string elementName, int x, int y, Vector2 anchorMin, Vector2 anchorMax)
    {
        
        var go = new GameObject(controllerName + elementName);
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

    void Update()
    {
        if (_manualFireButtonValid && Input.GetButton(_controllerName + "Fire1"))
        {
            CmdFireCannon();
        }
        //else
        //{
        //    if (Target != null)
        //    {
        //        var distanceToTarget = Vector3.Distance(Target.transform.position, transform.position);
        //        if (distanceToTarget < 15)
        //        {
        //            CmdFireCannon();
        //        }
        //    }
        //}
    }

    [Command]
    internal void CmdFireCannon()
    {
        if (Time.time > _nextFire)
        {
            _nextFire = Time.time + FiringRate;

            var shot = Instantiate(Shot, transform.position + (transform.forward * 1.6f), transform.rotation) as GameObject;

            var shotRigidBody = shot.GetComponent<Rigidbody>();

            var shipRigidBody = transform.GetComponent<Rigidbody>();

            shotRigidBody.velocity = shipRigidBody.velocity;
            shotRigidBody.AddForce(transform.forward * ShotForce);
            shipRigidBody.AddForce(transform.forward * (-ShotForce * .1f)); // Unrealistic recoil (for fun!)

            Destroy(shot, 3.0f);

            NetworkServer.Spawn(shot);
        }
    }
    void FixedUpdate() {
        if (_fuel > 0)
        {
            float force = 0, turboForce = 0;
            if (_controllerName.StartsWith("Player"))
            {
                var controllerName = name.Replace("(Clone)", "");
                var horizontal = Input.GetAxis(controllerName + "Horizontal");
                if (_lockedRotationUntil < Time.time)
                    _rigidbody.AddTorque(transform.up * 5f * horizontal);

                var vertical = Input.GetAxis(controllerName + "Vertical");
                force = (vertical > 0 ? vertical : vertical * .8f) * 15f;

                var trigger1 = Input.GetAxis(controllerName + "Trigger2");
                turboForce = trigger1 * 10f;
                
            }
            else if (tag == "Untagged")
            {
                _players.RemoveAll(item => item == null);
                var collector = _players.FirstOrDefault(x => Vector3.Distance(x.transform.position, transform.position) < 5);
                if (collector != null)
                {
                    tag = collector.tag;
                    GetComponent<Renderer>().material.color = Faction.Colour(tag);
                    AquireTarget();
                }
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
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.fixedDeltaTime);

                force = 7f;
            }
            else
            {
                AquireTarget();
            }

            float forceApplied = 0;
            if (AddForce)
            {
                _rigidbody.AddRelativeForce(Vector3.forward * force);
                _rigidbody.AddRelativeForce(Vector3.forward * turboForce);
                forceApplied = force + turboForce;
            }
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

    private void AquireTarget()
    {
        Target = Targeting.AquireTaget(tag, transform.position, _factions);
        if (Target != null)
            _targetRigidBody = Target.GetComponent<Rigidbody>();
    }
}
