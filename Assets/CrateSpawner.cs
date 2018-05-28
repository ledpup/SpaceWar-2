using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CrateSpawner : NetworkBehaviour
{
    public GameObject Crate;
    float _crateDropTime;
    void Start ()
    {
        
    }
	
	void Update ()
    {
        _crateDropTime += Time.deltaTime;
        if (_crateDropTime > 10)
        {
            _crateDropTime = 0;
            SpawnCrate();
        }
    }

    [Server]
    internal void SpawnCrate()
    {
        var random = new System.Random();
        var crate = Instantiate(Crate, new Vector3(random.Next(-50, 35), 25, random.Next(-20, 20)), Quaternion.identity) as GameObject;

        NetworkServer.Spawn(crate);
    }
}
