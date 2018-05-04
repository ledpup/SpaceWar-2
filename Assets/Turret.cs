using Assets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour, ITargeting
{
    public GameObject Target { get; set; }
    Rigidbody _targetRigidBody;

    public float RotationRate;
    float _projectileSpeed;
    Cannon _cannon;
    List<GameObject> _players;
    List<string> _factions;

    // Use this for initialization
    void Start ()
    {
        _factions = new List<string> { "Faction1", "Faction2", "Faction3" };

        _players = new List<GameObject>();
        _factions.ForEach(x => _players.AddRange(GameObject.FindGameObjectsWithTag(x)));

        Target = Targeting.AquireTaget(tag, transform.position, _factions);
        if (Target != null)
            _targetRigidBody = Target.GetComponent<Rigidbody>();

        _cannon = gameObject.GetComponentInChildren<Cannon>();

        GetComponent<Renderer>().material.color = Faction.Colour(tag);

        // F = m * (s/t), where "m" is the mass of the object, "s" is the desired speed and t = Time.fixedDeltaTime.
        var shotMass = 1f;
        float speed = (_cannon.ShotForce / shotMass) * Time.fixedDeltaTime;
        _projectileSpeed = speed;

    }



    void Update()
    {
        if (tag == "Untagged")
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
            var interceptPoint = PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, _projectileSpeed, Target.transform.position, _targetRigidBody.velocity);
            var direction = interceptPoint - transform.position;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, RotationRate * Time.deltaTime);
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
}