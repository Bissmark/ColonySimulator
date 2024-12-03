using UnityEngine;

public class TextureAssigner : MonoBehaviour
{
    public Texture2D texture1; // Texture for biome 1
    public Texture2D texture2; // Texture for biome 2
    public Material terrainMaterial; // Reference to the terrain material

    void Start()
    {
        // Assign texture 1 to _MainTex1 property
        terrainMaterial.SetTexture("_MainTex1", texture1);

        // Assign texture 2 to _MainTex2 property
        terrainMaterial.SetTexture("_MainTex2", texture2);
    }
}
