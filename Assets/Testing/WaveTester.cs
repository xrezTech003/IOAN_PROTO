using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTester : MonoBehaviour
{
    public shadeSwapper swapper;

    public Vector3 spawnLoc;
    public float waveIntensity;
    [Range(0, 3)]
    public int waveParam;
    public bool spawnWave;

    void Update()
    {
        if (spawnWave)
        {
            swapper.SpawnWave(spawnLoc, 20, 0);

            spawnWave = false;
        }
    }
}
