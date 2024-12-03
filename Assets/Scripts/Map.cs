using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public BiomePreset[] biomes; // create a array called biomes to store the different biomes
    public GameObject tilePrefab; // create a gameobject called tilePrefab to store the tile prefab
    public GameObject treePrefab; // create a gameobject called treePrefab to store the tree prefab
    public GameObject stonePrefab; // create a gameobject called stonePrefab to store the stone prefab

    [Header("Dimensions")] // create a header called Dimensions
    public int width = 50; // create a int called width and set it to 50
    public int height = 50; // create a int called height and set it to 50
    public float scale = 1.0f; // create a float called scale and set it to 1.0f
    public Vector2 offset; // create a vector2 called offset

    [Header("Seed")]
    public int seed = 0; // This variable will be used to randomize the game wor23ld creation

    [Header("Spawn")]
    //public float treeSpawnProbability;
    //public float stoneSpawnProbability = 0.5f;

    [Header("Height Map")]
    public Wave[] heightWaves;
    public float[,] heightMap;

    [Header("Moisture Map")]

    public Wave[] moistureWaves;
    public float[,] moistureMap;

    [Header("Heat Map")]
    public Wave[] heatWaves;
    public float[,] heatMap;

    void Start()
    {
        offset.x = Random.Range(0, 99999);
        offset.y = Random.Range(0, 99999);
        GenerateMap();
    }

    Color GetGradientColor(float value, Gradient gradient) {
        return gradient.Evaluate(value);
    }

    public void GenerateMap ()
    {
        heightMap = NoiseGenerator.Generate(width, height, scale, heightWaves, offset);
        moistureMap = NoiseGenerator.Generate(width, height, scale, moistureWaves, offset);
        heatMap = NoiseGenerator.Generate(width, height, scale, heatWaves, offset);

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                BiomePreset currentBiome = GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]);
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                tile.GetComponent<SpriteRenderer>().sprite = currentBiome.GetTileSprite();

                if (currentBiome.biomeType == BiomeType.Forest)
                {
                    // Generate a random number between 0 and 1
                    float randomValue = Random.value;

                    // Adjust this threshold to control the tree spawn probability
                    float treeSpawnProbability = 0.5f;

                    // Check if the random value is below the threshold
                    if (randomValue < treeSpawnProbability)
                    {
                        float zOffset = -0.1f;
                        Vector3 treePosition = new Vector3(x, y, tile.transform.position.z + zOffset);
                        GameObject tree = Instantiate(treePrefab, treePosition, Quaternion.identity);
                        // Set tree properties as needed
                    }
                }
                else if (currentBiome.biomeType == BiomeType.Mountains)
                {
                    GameObject stone = Instantiate(stonePrefab, new Vector3(x, y, 0), Quaternion.identity);
                    // Set stone properties as needed
                }
            }
        }
    }

    BiomePreset GetBiomeAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;
        else
            return GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]);
    }

    // Helper method to instantiate a regular tile
    // void InstantiateRegularTile(int x, int y, BiomePreset biome)
    // {
    //     GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
    //     tile.GetComponent<SpriteRenderer>().sprite = biome.GetTileSprite();
    // }

    public class BiomeTempData
    {
        public BiomePreset biome;

        public BiomeTempData (BiomePreset preset)
        {
            biome = preset;
        }

        public float GetDiffValue (float height, float moisture, float heat) 
        {
            return (height - biome.minHeight) + (moisture - biome.minMoisture) + (heat - biome.minHeat);
        }
    }

    BiomePreset GetBiome (float height, float moisture, float heat)
    {
        BiomePreset biomeToReturn = null;
        List<BiomeTempData> biomeTemp = new List<BiomeTempData>();

        foreach(BiomePreset biome in biomes)
        {
            if (biome.MatchCondition(height, moisture, heat)) 
            {
                biomeTemp.Add(new BiomeTempData(biome));
            }
        }

        float curVal = 0.0f;

        foreach(BiomeTempData biome in biomeTemp)
        {
            if(biomeToReturn == null)
            {
                biomeToReturn = biome.biome;
                curVal = biome.GetDiffValue(height, moisture, heat);
            }
            else
            {
                if(biome.GetDiffValue(height, moisture, heat) < curVal)
                {
                    biomeToReturn = biome.biome;
                    curVal = biome.GetDiffValue(height, moisture, heat);
                }
            }
        }

        if (biomeToReturn == null)
        {
            biomeToReturn = biomes[0];
        }

        return biomeToReturn;
    }
}
