using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    double mapX, mapY = 100.0f;
    double minX, maxX, minY, maxY;
    public GameObject t1, t2, t3;

	void Start () {
        var vertExtent = GetComponent<Camera>().orthographicSize;
        var horzExtent = vertExtent * Screen.width / Screen.height;

        // Calculations assume map is position at the origin
        minX = horzExtent - mapX / 2.0;
        maxX = mapX / 2.0 - horzExtent;
        minY = vertExtent - mapY / 2.0;
        maxY = mapY / 2.0 - vertExtent;
    }

    public void CameraFollowSmooth(Camera camera, List<Transform> transforms)
    {
        var zoomFactor = 1f;
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

            cameraDestination = midpoint - camera.transform.forward * distance * zoomFactor;
        }
        else
            cameraDestination = transforms[0].position;

        // Lock the maximum zoom
        if (cameraDestination.z < 25f)
            cameraDestination.z = 25f;


        if (camera.orthographic)
        {
            camera.orthographicSize = distance;
        }

        camera.transform.position = Vector3.Slerp(camera.transform.position, cameraDestination, followTimeDelta);

        // Snap when close enough to prevent annoying slerp behavior
        if ((cameraDestination - camera.transform.position).magnitude <= 0.05f)
            camera.transform.position = cameraDestination;
    }

    void Update () {

        var transforms = new List<Transform>();

        if (t1 != null)
            transforms.Add(t1.transform);
        if (t2 != null)
            transforms.Add(t2.transform);
        if (t3 != null)
            transforms.Add(t3.transform);

        CameraFollowSmooth(GetComponent<Camera>(), transforms);
    }
}
