using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    double mapX, mapY = 100.0f;
    double minX, maxX, minY, maxY;

	// Use this for initialization
	void Start () {
        var vertExtent = GetComponent<Camera>().orthographicSize;
        var horzExtent = vertExtent * Screen.width / Screen.height;

        // Calculations assume map is position at the origin
        minX = horzExtent - mapX / 2.0;
        maxX = mapX / 2.0 - horzExtent;
        minY = vertExtent - mapY / 2.0;
        maxY = mapY / 2.0 - vertExtent;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
