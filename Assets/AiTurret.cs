using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiTurret : MonoBehaviour
{

    public GameObject Target;
    public float RotationRate;
    Rigidbody _targetRigidBody;
    float _projectileSpeed;
    Cannon _cannon;
    public GameObject TargetPoint;
    GameObject _targetPoint;

    // Use this for initialization
    void Start () {
        _targetRigidBody = Target.GetComponent<Rigidbody>();
        _cannon = gameObject.GetComponentInChildren<Cannon>();

        // F = m * (s/t), where "m" is the mass of the object, "s" is the desired speed and t = Time.fixedDeltaTime.
        var shotMass = 1f;
        float speed = (_cannon.ShotForce / shotMass) * Time.fixedDeltaTime;
        _projectileSpeed = speed;

        _targetPoint = Instantiate(TargetPoint, transform.position, transform.rotation) as GameObject;
        _targetPoint.GetComponent<Renderer>().material.color = Color.red;
    }

    void Update()
    {
        if (Target != null)
        {
            var targetAimPoint = Assets.PredictiveAiming.FirstOrderIntercept(transform.position, Vector3.zero, _projectileSpeed, Target.transform.position, _targetRigidBody.velocity);
            _targetPoint.transform.position = targetAimPoint;

            var rotation = Assets.Targeting.RotateToTarget(targetAimPoint, transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationRate * Time.deltaTime);

            var distanceToTarget = Vector3.Distance(Target.transform.position, transform.position);

            if (distanceToTarget < 10)
            {
                _cannon.FireCannon();
            }
        }
    }   
}