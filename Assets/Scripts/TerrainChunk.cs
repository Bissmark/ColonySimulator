using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class TerrainChunk {
    const float colliderGenerationDstThreshold = 5;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    NavMeshSurface navMeshSurface;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapRecieved;
    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDst;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    TextureData textureData;
    Transform viewer;

    public NavMeshSurface NavMeshSurface {
        get; private set;
    }

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, TextureData textureData, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        this.textureData = textureData;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        navMeshSurface = meshObject.AddComponent<NavMeshSurface>();

        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;

        navMeshSurface.collectObjects = CollectObjects.Volume;
        navMeshSurface.center = new Vector3(0, 5, 0);
        navMeshSurface.size = new Vector3(122, 10, 122);
        navMeshSurface.minRegionArea = 100;

        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;

            if (i == colliderLODIndex) {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    }

    public void Load() {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, sampleCentre), OnHeightMapRecieved);
    }

    void OnHeightMapRecieved(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapRecieved = true;

        UpdateTerrainChunk();
        SpawnTrees(10, 250, viewerPosition);
    }

    Vector2 viewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk()
    {
        if (heightMapRecieved) {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible) {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++) {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                        lodIndex = i + 1;
                    } else {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex) {
                    LODMesh lodMesh = lodMeshes[lodIndex];

                    if (lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                        
                        if (navMeshSurface != null) {
                            navMeshSurface.BuildNavMesh();
                        }
                    } else if (!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            if (wasVisible != visible) {
                SetVisible(visible);
                if (onVisibilityChanged != null) {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider) {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold) {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh) {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDstThreshold * colliderGenerationDstThreshold) {
                if (lodMeshes[colliderLODIndex].hasMesh) {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

    HeightMap treeHeightMap;

    public void SpawnTrees(int numTrees, float maxLoadDistance, Vector2 viewerPosition)
    {
        treeHeightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, sampleCentre);



        for (int i = 0; i < numTrees; i++) {
            float x = Random.Range(0, meshSettings.meshWorldSize);
            float z = Random.Range(0, meshSettings.meshWorldSize);

            float height = GetHeightFromHeightMap(x, z);

            // Convert local position to world position
            Vector3 worldPosition = new Vector3(x, height, z) + new Vector3(bounds.min.x, 0, bounds.min.y);

            // Check if the world position is within the maxLoadDistance from the viewer
            if (Vector2.Distance(new Vector2(worldPosition.x, worldPosition.z), viewerPosition) <= maxLoadDistance)
            {
                if (textureData.layers[1].canSpawnTrees)
                {
                    // Instantiate the tree prefab at the calculated position which is also on the layer in the textureData
                    GameObject tree = GameObject.Instantiate(textureData.treePrefab, worldPosition, Quaternion.identity);
                    tree.transform.parent = meshObject.transform;
                }
            }
        }
    }
    
    float GetHeightFromHeightMap(float x, float z)
    {
        // Convert local position to heightmap position
        int mapX = Mathf.RoundToInt(x / meshSettings.meshWorldSize * (treeHeightMap.values.GetLength(0) - 1));
        int mapZ = Mathf.RoundToInt(z / meshSettings.meshWorldSize * (treeHeightMap.values.GetLength(1) - 1));

        // Ensure the indices are within bounds
        mapX = Mathf.Clamp(mapX, 0, treeHeightMap.values.GetLength(0) - 1);
        mapZ = Mathf.Clamp(mapZ, 0, treeHeightMap.values.GetLength(1) - 1);

        // Get the height value from the heightmap
        return heightMap.values[mapX, mapZ];
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod) {
            this.lod = lod;
        }

        void OnMeshDataRecieved(object meshDataObject) {
            mesh = ((MeshData)meshDataObject).CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataRecieved);
        }
    }
}