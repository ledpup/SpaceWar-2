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

    public float Armour { get; set; }
    Rigidbody _rigidbody;


    float _lockedRotationUntil;
    float _fuel;

    Cannon _cannon;
    List<string> _factions;
    List<GameObject> _players;
    string _controllerName;

    public GameObject Shot;
    public GameObject Missile;
    
    public float ShotForce = 500f;
    public float CannonFiringRate = .3f;
    public float MissileFiringRate = .8f;
    float _nextCannonFire, _nextMissileFire;
    PlayerHud _playerHud;
    void Start ()
    {
        if (name.StartsWith("Ship"))
        {
            name = "Player1";
        }

        var ni = transform.GetComponent<NetworkIdentity>();
        if (ni != null && !isLocalPlayer)
        {
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

        _cannon = gameObject.GetComponentInChildren<Cannon>();

        _lockedRotationUntil = Time.time;
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<Renderer>().material.color = Faction.Colour(tag);
    }

    void Update()
    {
        if (_controllerName != null)
        {
            if (Input.GetButton(_controllerName + "Fire1"))
            {
                CmdFireCannon();
            }
            var fire2 = Input.GetButton(_controllerName + "Fire2");
            var fire3 = Input.GetButton(_controllerName + "Fire3");
            if (fire2 || fire3)
            {
                CmdFireMissile(fire2);
            }
        }
    }

    [Command]
    internal void CmdFireMissile(bool smartMissile)
    {
        if (Time.time > _nextMissileFire)
        {
            _nextMissileFire = Time.time + MissileFiringRate;
            var missileLauncher = gameObject.GetComponentInChildren<MissileLauncher>();

            LockRotation(.5f);

            var missile = Instantiate(Missile, missileLauncher.transform.position, missileLauncher.transform.rotation) as GameObject;

            missile.tag = _controllerName;
            missile.SendMessage("SetMissileType", smartMissile ? MissleType.Smart : MissleType.Homing);
            missile.SendMessage("SetTarget", "Player1");

            var bulletRigidBody = missile.GetComponent<Rigidbody>();
            var shipRigidBody = GetComponent<Rigidbody>();

            bulletRigidBody.velocity = shipRigidBody.velocity; // Base speed on the ship's velocity

            NetworkServer.Spawn(missile);
        }
    }

    [Command]
    internal void CmdFireCannon()
    {
        if (Time.time > _nextCannonFire)
        {
            _nextCannonFire = Time.time + CannonFiringRate;

            var shot = Instantiate(Shot, _cannon.transform.TransformPoint(Vector3.forward * 1.1f), _cannon.transform.rotation) as GameObject;

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

            float forceApplied = 0;
            _rigidbody.AddRelativeForce(Vector3.forward * force);
            _rigidbody.AddRelativeForce(Vector3.forward * turboForce);
            forceApplied = force + turboForce;
            _fuel -= (forceApplied * .001f) + 0.001f;
        }

        var armouredObject = GetComponent<ArmouredObject>();
        if (armouredObject != null && armouredObject.QueuedPhysics != Vector3.zero)
        {
            _rigidbody.AddForce(armouredObject.QueuedPhysics);
            armouredObject.QueuedPhysics = Vector3.zero;
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
}
