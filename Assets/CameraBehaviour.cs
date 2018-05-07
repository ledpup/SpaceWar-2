using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CameraBehaviour : NetworkManager {

    double mapX, mapY = 100.0f;
    double minX, maxX, minY, maxY;

    [Header("Scene Camera Properties")]
    [SerializeField] Transform SceneCamera;
    [SerializeField] float CameraRotationRadius = 50f;
    [SerializeField] float CameraRotationSpeed = 3f;
    bool _canRotate = true;

    float _rotation;

	void Start () {
        //var vertExtent = GetComponent<Camera>().orthographicSize;
        //var horzExtent = vertExtent * Screen.width / Screen.height;

        //// Calculations assume map is position at the origin
        //minX = horzExtent - mapX / 2.0;
        //maxX = mapX / 2.0 - horzExtent;
        //minY = vertExtent - mapY / 2.0;
        //maxY = mapY / 2.0 - vertExtent;
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);

        _canRotate = false;

        SceneCamera.rotation = Quaternion.Euler(90, 0, 0);
    }

    public override void OnStartHost()
    {
        base.OnStartHost();

        _canRotate = false;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _canRotate = true;
    }

    public override void OnStopHost()
    {
        base.OnStopHost();

        _canRotate = true;
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
            var transforms = new List<Transform>();

            var nis = FindObjectsOfType<NetworkIdentity>().ToList();
            var localPlayers = nis.Where(x => x.isLocalPlayer).Select(x => x.transform).ToList();
            transforms.AddRange(localPlayers);

            CameraFollowSmooth(SceneCamera, transforms);
        }
    }

    void CameraFollowSmooth(Transform cameraTransform, List<Transform> transforms)
    {
        var zoomFactor = 1.25f;
        var followTimeDelta = 0.01f;

        Vector3 cameraDestination;
        float distance = 0;
        if (transforms.Count > 1)
        {
            var transformSum = Vector3.zero;
            var distances = new List<float>();
            Vector3 previousObjectPostion = Vector3.zero;
            foreach (var transform in transforms)
            {
                transformSum += transform.position;
                if (previousObjectPostion != Vector3.zero)
                    distances.Add(Vector3.Distance(transform.position, previousObjectPostion));
                previousObjectPostion = transform.position;
            }

            var midpoint = transformSum / transforms.Count;

            distance = distances.Max();

            cameraDestination = midpoint - cameraTransform.forward * distance * zoomFactor;
        }
        else if (transforms.Count == 1)
        {
            cameraDestination = transforms[0].position;
        }
        else
        {
            return;
        }

        // Lock the maximum zoom
        if (cameraDestination.y < 50f)
            cameraDestination.y = 50f;


        cameraTransform.position = Vector3.Slerp(cameraTransform.position, cameraDestination, followTimeDelta);

        // Snap when close enough to prevent annoying slerp behavior
        if ((cameraDestination - cameraTransform.position).magnitude <= 0.05f)
            cameraTransform.position = cameraDestination;
    }
}
