using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiTurret : MonoBehaviour
{

    public GameObject Target;
    float maxDegreesPerSecond = 30.0f;
    private Quaternion qTo;
    public Text ZDirection;
    private Quaternion targetLocalRotation;

    // Use this for initialization
    void Start () {
        targetLocalRotation = Target.transform.localRotation;
    }


    //var v3T = goTarget.transform.position - transform.position;
    //v3T.y = transform.position.y;
    //qTo = Quaternion.LookRotation(v3T, Vector3.up);        
    //transform.rotation = Quaternion.RotateTowards(transform.rotation, qTo, maxDegreesPerSecond* Time.deltaTime);


    // Update is called once per frame

    Quaternion lookAtRotation;

    public float smoothing = 30.0f;

    public float initialForwardAngle = 0; // initial angle of your "gun barrel"
    public float maxRotationSpeed = 60;
    public float threshold = 4;
    void Update()
    {

        var localAimDirection = transform.TransformDirection(Target.transform.position - Target.transform.position);
        //var flatLocalAimDirection = new Vector3(localAimDirection.x, 0, localAimDirection.z);
        var flatLocalAimDirection = new Vector3(0, 0, Mathf.Abs(localAimDirection.z));
        transform.localRotation = Quaternion.LookRotation(flatLocalAimDirection);

        //targetLocalRotation = Quaternion.LookRotation(flatLocalAimDirection, Vector3.up);
        //transform.localRotation = targetLocalRotation;

        var turretLocalAimDirection = transform.TransformDirection(Target.transform.position - transform.position);
        transform.localRotation = Quaternion.LookRotation(turretLocalAimDirection);

        ZDirection.text = transform.rotation.z.ToString();

        // //Vector3 target = GameObject.Find("target").transform.position;
        // Vector3 dir = Target.transform.position - transform.localPosition;
        // dir.z = transform.position.z;

        //// transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, Time.deltaTime * smoothing);

        // transform.LookAt(dir);


        //lookAtRotation = Quaternion.LookRotation(Target.transform.position - transform.position);

        //lookAtRotation.z = 0;
        ////lookAtRotation.y = 0;

        //if (transform.rotation != lookAtRotation)
        //{
        //    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAtRotation, Time.deltaTime * smoothing);
        //}


        //RotateGradually2D();
    }
    public void RotateGradually2D()
    {
        angleToTarget = Mathf.Atan2(Target.transform.position.y - transform.position.y, Target.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
        signToTarget = Mathf.Sign(angleToTarget - _currentAngle);
        if (Mathf.Abs(angleToTarget - _currentAngle) > threshold)
        {
            _currentAngle += signToTarget * maxRotationSpeed * Time.deltaTime;
        }
        else
        {
            _currentAngle = angleToTarget;
        }
        transform.eulerAngles = new Vector3(0, 0, _currentAngle - initialForwardAngle);
    }
    private float angleToTarget; // Destination angle
    private float _currentAngle = 0; // Current angle
    private float signToTarget;
}
