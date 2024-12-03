using UnityEngine;

public class BiomeTextureAssigner : MonoBehaviour
{
    public Sprite[] biomeSprites; // Array of biome sprites from the spritesheet
    public Material terrainMaterial; // Reference to the terrain material

    void Start()
    {
        // Assign each biome sprite to the corresponding texture property in the terrain material
        for (int i = 0; i < biomeSprites.Length; i++)
        {
            // Set the texture property based on the index
            terrainMaterial.SetTexture("_BiomeTexture" + (i + 1), biomeSprites[i].texture);
        }
    }
}
