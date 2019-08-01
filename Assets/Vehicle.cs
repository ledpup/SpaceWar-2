using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AmmoType
{
    Cannon,
    Missile,
}

public class Vehicle : MonoBehaviour, ITargeting
{

    public GameObject Target { get; set; }
    Rigidbody _targetRigidBody;
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
    Dictionary<AmmoType, int> _ammo;
    List<Hardpoint> _hardpoints;
    void Start ()
    {        
        _controllerName = name;

        _rigidbody = GetComponent<Rigidbody>();
        _fuel = 100;

        _factions = new List<string> { "Faction1", "Faction2", "Faction3" };
        _players = new List<GameObject>();
        _factions.ForEach(x => _players.AddRange(GameObject.FindGameObjectsWithTag(x)));

        _playerHud = gameObject.GetComponent<PlayerHud>();

        _cannon = gameObject.GetComponentInChildren<Cannon>();

        _lockedRotationUntil = Time.time;

        _ammo = new Dictionary<AmmoType, int> { { AmmoType.Cannon, 50 }, { AmmoType.Missile, 10 } };

        GetComponent<Renderer>().material.color = Faction.Colour(tag);
    }

    void Update()
    {
        if (_controllerName != null)
        {
            if (_cannon != null && Input.GetButton(_controllerName + "Fire1") && Time.time > _nextCannonFire && _ammo[AmmoType.Cannon] > 0)
            {
                _nextCannonFire = Time.time + CannonFiringRate;
                FireCannon();
                ChangeShots(-1);
            }
            var fire2 = Input.GetButton(_controllerName + "Fire2");
            var fire3 = Input.GetButton(_controllerName + "Fire3");
            if ((fire2 || fire3) && Time.time > _nextMissileFire && _ammo[AmmoType.Missile] > 0)
            {
                _nextMissileFire = Time.time + MissileFiringRate;
                FireMissile(fire2);
                _ammo[AmmoType.Missile]--;
                _playerHud.MissilesText.text = "Missiles " + _ammo[AmmoType.Missile].ToString();
            }
        }
        if (_playerHud != null && string.IsNullOrEmpty(_playerHud.ShotsText.text))
        {
            _playerHud.ShotsText.text = "Shots " + _ammo[AmmoType.Cannon].ToString();
            _playerHud.MissilesText.text = "Missiles " + _ammo[AmmoType.Missile].ToString();
        }
    }

    private void ChangeShots(int change)
    {
        _ammo[AmmoType.Cannon] += change;
        _playerHud.ShotsText.text = "Shots " + _ammo[AmmoType.Cannon].ToString();
    }

    internal void FireMissile(bool smartMissile)
    {
        var missileLauncher = gameObject.GetComponentInChildren<MissileLauncher>();

        LockRotation(1f);

        var missile = Instantiate(Missile, missileLauncher.transform.position, missileLauncher.transform.rotation) as GameObject;

        missile.tag = _controllerName;
        missile.SendMessage("SetMissileType", smartMissile ? MissleType.Smart : MissleType.Homing);
        missile.SendMessage("SetTarget", "Player1");

        var bulletRigidBody = missile.GetComponent<Rigidbody>();
        var vehicleRigidBody = GetComponent<Rigidbody>();

        bulletRigidBody.velocity = vehicleRigidBody.velocity; // Base speed on the ship's velocity
    }

    internal void FireCannon()
    {
        var shot = Instantiate(Shot, _cannon.transform.TransformPoint(Vector3.forward * 1.1f), _cannon.transform.rotation) as GameObject;

        var shotRigidBody = shot.GetComponent<Rigidbody>();
        var vehicleRigidBody = transform.GetComponent<Rigidbody>();

        shotRigidBody.velocity = vehicleRigidBody.velocity;
        shotRigidBody.AddForce(transform.forward * ShotForce);
        vehicleRigidBody.AddForce(transform.forward * (-ShotForce * .1f)); // Unrealistic recoil (for fun!)
    }

    void FixedUpdate()
    {    
        float force = 0, turboForce = 0;
        if (_controllerName != null && _controllerName.StartsWith("Player"))
        {
            var horizontal = Input.GetAxis(_controllerName + "Horizontal");
            if (_lockedRotationUntil < Time.time)
                _rigidbody.AddTorque(transform.up * 5f * horizontal);

            var vertical = Input.GetAxis(_controllerName + "Vertical");
            force = (vertical > 0 ? vertical : vertical * .8f) * 15f;

            var trigger1 = Input.GetAxis(_controllerName + "Trigger2");
            turboForce = trigger1 * 10f;
        }

        if (_fuel > 0)
        {
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

    internal void CollectCrate(Crate.CrateType crateType, int quantity)
    {
        switch (crateType)
        {
            case Crate.CrateType.Armour:
                var armouredObject = GetComponent<ArmouredObject>();
                armouredObject.Armour += quantity;
                _playerHud.ArmourText.text = "Armour " + Mathf.RoundToInt(armouredObject.Armour).ToString();
                break;
            case Crate.CrateType.Fuel:
                _fuel += quantity;
                _playerHud.FuelText.text = "Fuel " + Math.Round(_fuel, 0);
                break;
            case Crate.CrateType.Missile:
                _ammo[AmmoType.Missile] += quantity;
                _playerHud.MissilesText.text = "Missiles " + _ammo[AmmoType.Missile].ToString();
                break;
            case Crate.CrateType.Shot:
                ChangeShots(quantity);
                break;
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
