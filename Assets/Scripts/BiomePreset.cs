using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum BiomeType
{
    Forest,
    Desert,
    Tundra,
    Ocean,
    Grassland,
    Jungle,
    Mountains
}

[CreateAssetMenu(fileName = "Biome Preset", menuName = "New Biome Preset")]
public class BiomePreset : ScriptableObject
{
    public BiomeType biomeType;
    public Sprite[] tiles;
    public float minHeight;
    public float minMoisture;
    public float minHeat;
    public int biomeIndex;
    public Gradient heightGradient;
    public Gradient moistureGradient;
    public Gradient heatGradient;

    // returns a random sprite
    public Sprite GetTileSprite()
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public bool MatchCondition (float height, float moisture, float heat)
    {
        return height >= minHeight && moisture >= minMoisture && heat >= minHeat;
    }
}