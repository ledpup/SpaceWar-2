using Assets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Turret : NetworkBehaviour, ITargeting
{
    public GameObject Target { get; set; }
    Rigidbody _targetRigidBody;

    public float RotationRate = .2f;
    List<string> _factions;

    public GameObject Shot;
    public float ShotForce = 500f;
    public float FiringRate = .3f;
    float _nextFire;

    void Start ()
    {
        _factions = new List<string> { "Faction1", "Faction2", "Faction3" };

        Target = Targeting.AquireTaget(tag, transform.position, _factions);
        if (Target != null)
            _targetRigidBody = Target.GetComponent<Rigidbody>();

        GetComponent<Renderer>().material.color = Faction.Colour(tag);
    }

    void Update()
    {
        if (tag == "Untagged")
        {
            var players = new List<GameObject>();
            _factions.ForEach(x => players.AddRange(GameObject.FindGameObjectsWithTag(x)));
            var collector = players.FirstOrDefault(x => Vector3.Distance(x.transform.position, transform.position) < 5);
            if (collector != null)
            {
                tag = collector.tag;
                GetComponent<Renderer>().material.color = Faction.Colour(tag);
                AquireTarget();
            }
        }
        else if (Target != null)
        {
            // F = m * (s/t), where "m" is the mass of the object, "s" is the desired speed and t = Time.fixedDeltaTime.
            var shotMass = 1f;
            float projectileSpeed = (ShotForce / shotMass) * Time.fixedDeltaTime;

            var interceptPoint = PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, projectileSpeed, Target.transform.position, _targetRigidBody.velocity);
            var direction = interceptPoint - transform.position;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, RotationRate);

            var distanceToTarget = Vector3.Distance(Target.transform.position, transform.position);

            if (distanceToTarget < 15)
            {
                CmdFireCannon();
            }
        }
        else
        {
            AquireTarget();
        }
    }

    private void AquireTarget()
    {
        Target = Targeting.AquireTaget(tag, transform.position, _factions);
        if (Target != null)
            _targetRigidBody = Target.GetComponent<Rigidbody>();
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
}