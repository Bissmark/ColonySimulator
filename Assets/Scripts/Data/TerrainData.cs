using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TerrainData : UpdateableData
{
    public float uniformScale = 2f;

    public bool useFlatShading;
    public bool useFalloff;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
}
