using UnityEngine;
using System.Collections;
using Cinemachine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdateableData
{
   public NoiseSettings noiseSettings;

    public bool useFalloff;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minheight {
        get {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxheight {
        get {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }
    
    #if UNITY_EDITOR

    protected override void OnValidate() {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }

    #endif
}
