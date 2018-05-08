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

    public float Armour { get; set; }
    Rigidbody _rigidbody;


    float _lockedRotationUntil;
    float _fuel;

    Cannon _cannon;
    List<string> _factions;
    List<GameObject> _players;
    string _controllerName;

    public GameObject Shot;
    public float ShotForce = 500f;
    public float FiringRate = .3f;
    float _nextFire;
    bool _manualFireButtonValid;
    PlayerHud _playerHud;
    void Start ()
    {
        var ni = transform.GetComponent<NetworkIdentity>();
        if (ni != null && !isLocalPlayer)
        {
            Destroy(this);
            return;
        }

        _controllerName = name.Replace("(Clone)", "");

        _rigidbody = GetComponent<Rigidbody>();
        Armour = 10;
        _fuel = 100;

        _factions = new List<string> { "Faction1", "Faction2", "Faction3" };
        _players = new List<GameObject>();
        _factions.ForEach(x => _players.AddRange(GameObject.FindGameObjectsWithTag(x)));

        _playerHud = gameObject.GetComponent<PlayerHud>();

        if (!name.StartsWith("Player"))
        {
            AcquireTarget();
        }

        GetComponent<Renderer>().material.color = Faction.Colour(tag);

        _cannon = gameObject.GetComponentInChildren<Cannon>();

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
                    AcquireTarget();
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
                AcquireTarget();
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

        if (_playerHud != null)
        {
            _playerHud.SpeedText.text = "Speed " + Math.Round(_rigidbody.velocity.magnitude * 10, 0);
            _playerHud.FuelText.text = "Fuel " + Math.Round(_fuel, 0);
            if (_fuel < 20f)
                _playerHud.FuelText.color = Color.red;
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
            
        }
    }

    private void AcquireTarget()
    {
        Target = Targeting.AquireTaget(tag, transform.position, _factions);
        if (Target != null)
            _targetRigidBody = Target.GetComponent<Rigidbody>();
    }
}
