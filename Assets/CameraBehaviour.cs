using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CameraBehaviour : MonoBehaviour {

    double mapX, mapY = 100.0f;
    double minX, maxX, minY, maxY;

    [Header("Scene Camera Properties")]
    [SerializeField] Transform SceneCamera;
    public Camera FirstSplitCamera;
    [SerializeField] float CameraRotationRadius = 50f;
    [SerializeField] float CameraRotationSpeed = 3f;
    bool _canRotate = true;
    float _rotation;

    public GameObject Vehicle;

	void Start ()
    {
        //var vertExtent = GetComponent<Camera>().orthographicSize;
        //var horzExtent = vertExtent * Screen.width / Screen.height;

        //// Calculations assume map is position at the origin
        //minX = horzExtent - mapX / 2.0;
        //maxX = mapX / 2.0 - horzExtent;
        //minY = vertExtent - mapY / 2.0;
        //maxY = mapY / 2.0 - vertExtent;
        FirstSplitCamera.enabled = false;

        _canRotate = false;

        SceneCamera.rotation = Quaternion.Euler(90, 0, 0);
    }

    void Update ()
    {
        if (_canRotate)
        {
            _rotation += CameraRotationSpeed * Time.deltaTime;
            if (_rotation >= 360f)
                _rotation = -360f;

            SceneCamera.position = Vector3.zero;
            SceneCamera.rotation = Quaternion.Euler(0f, _rotation, 0f);
            SceneCamera.Translate(0f, CameraRotationRadius, -CameraRotationRadius);
            SceneCamera.LookAt(Vector3.zero);
        }
        else
        {
            //var localPlayers = nis.Where(x => x.isLocalPlayer).Select(x => x.transform).ToList();
            //var positions = localPlayers.Select(x => x.position).ToList();

            CameraFollowSmooth(SceneCamera, new List<Vector3> { Vector3.zero });

            var vehicles = GameObject.FindObjectsOfType<Vehicle>();

            for (var i = 1; i < 4; i++)
            {
                if (Input.GetButtonDown($"Player{i}Fire1"))
                {
                    if (!vehicles.Any(x => x.name.StartsWith($"Player{i}")))
                    {
                        var random = new System.Random();
                        var vehicle = Instantiate(Vehicle, new Vector3(random.Next(-50, 35), 0.5f, random.Next(-20, 20)), Quaternion.identity) as GameObject;
                        vehicle.name = $"Player{i}";
                        vehicle.tag = $"Faction{i}";
                    }
                }
            }
        }
    }


    void CameraFollowSmooth(Transform cameraTransform, List<Vector3> positionsToTrack)
    {
        var zoomFactor = 1.25f;
        var followTimeDelta = 0.01f;

        Vector3 cameraDestination;
        float distance = 0;
        if (positionsToTrack.Count > 1)
        {
            var transformSum = Vector3.zero;
            var distances = new List<float>();
            Vector3 previousObjectPostion = Vector3.zero;
            foreach (var position in positionsToTrack)
            {
                transformSum += position;
                if (previousObjectPostion != Vector3.zero)
                    distances.Add(Vector3.Distance(position, previousObjectPostion));
                previousObjectPostion = position;
            }

            var midpoint = transformSum / positionsToTrack.Count;

            distance = distances.Max();

            cameraDestination = midpoint - cameraTransform.forward * distance * zoomFactor;
        }
        else if (positionsToTrack.Count == 1)
        {
            cameraDestination = positionsToTrack[0];
        }
        else
        {
            return;
        }

        // Lock the maximum zoom
        if (cameraDestination.y < 50f)
        {
            FirstSplitCamera.enabled = false;
            Camera.main.rect = new Rect(0, 0, 1, 1);
            cameraDestination.y = 50f;
        }
        else if (cameraDestination.y > 100f)
        {
            FirstSplitCamera.enabled = true;
            Camera.main.rect = new Rect(0, 0, 0.5f, 1);
            Camera.main.transform.position = new Vector3(positionsToTrack[0].x, 50, positionsToTrack[0].z);
            FirstSplitCamera.transform.position = new Vector3(positionsToTrack[1].x, 50, positionsToTrack[1].z);
            //cameraDestination.y = 50f;
        }

        cameraTransform.position = Vector3.Slerp(cameraTransform.position, cameraDestination, followTimeDelta);

        // Snap when close enough to prevent annoying slerp behavior
        if ((cameraDestination - cameraTransform.position).magnitude <= 0.05f)
            cameraTransform.position = cameraDestination;
    }
}
