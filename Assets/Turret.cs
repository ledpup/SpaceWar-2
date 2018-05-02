using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
    GameObject _target;
    public float RotationRate;
    Rigidbody _targetRigidBody;
    float _projectileSpeed;
    Cannon _cannon;
    public GameObject TargetPoint;
    GameObject _targetPoint;
    List<GameObject> _players;
    List<string> _factions;

    // Use this for initialization
    void Start ()
    {
        _factions = new List<string> { "Faction1", "Faction2" };

        _players = new List<GameObject>();
        _factions.ForEach(x => _players.AddRange(GameObject.FindGameObjectsWithTag(x)));

        FindTaget(_factions);

        _cannon = gameObject.GetComponentInChildren<Cannon>();

        GetComponent<Renderer>().material.color = Faction.Colour(tag);

        // F = m * (s/t), where "m" is the mass of the object, "s" is the desired speed and t = Time.fixedDeltaTime.
        var shotMass = 1f;
        float speed = (_cannon.ShotForce / shotMass) * Time.fixedDeltaTime;
        _projectileSpeed = speed;

        _targetPoint = Instantiate(TargetPoint, transform.position, transform.rotation) as GameObject;
        _targetPoint.GetComponent<Renderer>().material.color = Color.red;
    }

    private void FindTaget(List<string> factions)
    {
        if (tag == "Untagged")
            return;

        var enemies = new List<GameObject>();
        factions.Where(x => x != tag).ToList().ForEach(x => enemies.AddRange(GameObject.FindGameObjectsWithTag(x)));
        _target = enemies.First();
        _targetRigidBody = _target.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (tag == "Untagged")
        {
            var collector = _players.FirstOrDefault(x => Vector3.Distance(x.transform.position, transform.position) < 5);
            if (collector != null)
            {
                tag = collector.tag;
                GetComponent<Renderer>().material.color = Faction.Colour(tag);
                FindTaget(_factions);
            }
        }
        
        if (_target != null)
        {
            var targetAimPoint = Assets.PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, _projectileSpeed, _target.transform.position, _targetRigidBody.velocity);
            _targetPoint.transform.position = targetAimPoint;

            Vector3 direction = targetAimPoint - transform.position;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, RotationRate * Time.deltaTime);

            var distanceToTarget = Vector3.Distance(_target.transform.position, transform.position);

            if (distanceToTarget < 15)
            {
                _cannon.FireCannon();
            }
        }
    }   
}