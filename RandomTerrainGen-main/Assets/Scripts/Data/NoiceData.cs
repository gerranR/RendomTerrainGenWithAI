﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class NoiceData : UpdatableData
{
    public Noise.NormalizeMode normalizeMode;


    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public float noiseScale;

    public int seed;
    public Vector2 offset;


#if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        base.OnValidate();
    }
#endif
}
