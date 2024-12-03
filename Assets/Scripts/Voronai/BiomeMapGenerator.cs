using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BiomeMapGenerator : MonoBehaviour
{
    public int mapWidth = 100;
    public int mapHeight = 100;
    public int numBiomes = 5; // Number of different biome types
    public float biomeScale = 20f;
    public List<Color> biomeColors; // Colors representing different biomes
    public Tilemap tilemap; // Reference to the Tilemap component
    public BiomeTile[] biomeTiles; // Array of BiomeTile assets representing different biomes

    private void Start()
    {
        Texture2D biomeMap = GenerateBiomeMap();
        GenerateTilemap(biomeMap);

        // Visualize the biome map texture
        //VisualizeBiomeMap(biomeMap);
    }

    // private void VisualizeBiomeMap(Texture2D biomeMap)
    // {
    //     // Create a sprite from the biome map texture
    //     Sprite sprite = Sprite.Create(biomeMap, new Rect(0, 0, biomeMap.width, biomeMap.height), Vector2.zero, 1f);

    //     // Create a GameObject to visualize the biome map texture
    //     GameObject biomeMapVisualizer = new GameObject("BiomeMapVisualizer");
    //     SpriteRenderer spriteRenderer = biomeMapVisualizer.AddComponent<SpriteRenderer>();
    //     spriteRenderer.sprite = sprite;

    //     // Set the position and scale of the visualizer
    //     biomeMapVisualizer.transform.position = new Vector3(mapWidth / 2f, mapHeight / 2f, 0f); // Center of the map
    //     biomeMapVisualizer.transform.localScale = new Vector3(10f, 10f, 1f); // Adjust scale as needed
    // }

    private Texture2D GenerateBiomeMap()
    {
        Texture2D biomeMap = new Texture2D(mapWidth, mapHeight);

        Vector2[] biomeSeeds = new Vector2[numBiomes];

        // Generate random seed points for biomes
        for (int i = 0; i < numBiomes; i++)
        {
            biomeSeeds[i] = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));
        }

        // Generate biome map
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float minDistance = Mathf.Infinity;
                int biomeIndex = 0;

                // Find the closest biome seed point
                for (int i = 0; i < numBiomes; i++)
                {
                    float distance = Vector2.Distance(biomeSeeds[i], new Vector2(x, y));

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        biomeIndex = i;
                    }
                }

                // Assign color based on the closest biome seed
                biomeMap.SetPixel(x, y, biomeColors[biomeIndex]);

                // Debug: Log biome index and position
                Debug.Log("Biome Index at (" + x + ", " + y + "): " + biomeIndex);
            }
        }

        biomeMap.Apply();
        return biomeMap;
    }

    private void GenerateTilemap(Texture2D biomeMap)
    {
        // Generate tilemap from biome map
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Color pixelColor = biomeMap.GetPixel(x, y);

                // Find the closest biome color
                int closestBiomeIndex = GetClosestBiomeIndex(pixelColor);

                // Place tile at (x, y) position based on biome color
                tilemap.SetTile(new Vector3Int(x, y, 0), biomeTiles[closestBiomeIndex]);

                // Debug: Log tile position and color
                Debug.Log("Tile placed at (" + x + ", " + y + ") with color: " + pixelColor);
            }
        }
    }

    private int GetClosestBiomeIndex(Color pixelColor)
    {
        float minDistance = Mathf.Infinity;
        int closestBiomeIndex = 0;

        // Ensure biomeColors list is not empty
        if (biomeColors.Count > 0)
        {
            for (int i = 0; i < numBiomes; i++)
            {
                float distance = Vector3.Distance(new Vector3(pixelColor.r, pixelColor.g, pixelColor.b), new Vector3(biomeColors[i].r, biomeColors[i].g, biomeColors[i].b));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBiomeIndex = i;
                }
            }
        }
        else
        {
            Debug.LogWarning("BiomeColors list is empty!");
        }

        return closestBiomeIndex;
    }
}
