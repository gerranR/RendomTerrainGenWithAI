using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEditor;

public class MapGenerator : MonoBehaviour
{


    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public TerrainData[] terrainData;
    public NoiceData noiseData;
    public TextureData[] textureData;

    public const int dataNum = 0;

    public Material terrainMaterial;

    [Range(0, 6)]
    public int editorPreviewLOD;

    public bool autoUpdate;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MashData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MashData>>();

    [Space(20f)]

    public Node nodePrefab;
    public List<Node> nodeList;

    public GameObject npc;

    [SerializeField]
    private bool canDrawGizmos;

    [SerializeField]
    private GameObject nodeParent;

    [Tooltip("the higher the node density the fewer nodes")]
    public int nodeDensity = 1;

    public bool GenNodes;

    private void Awake()
    {
        textureData[dataNum].UpdateMeshHeight(terrainMaterial, terrainData[dataNum].minHeight, terrainData[dataNum].maxHeight);

        if (noiseData.randomSeed)
        {
            noiseData.seed = UnityEngine.Random.Range(0, 999999);
        }
    }
    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData[dataNum].applyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get
        {
            if (terrainData[dataNum].useFlatShading)
            {
                return 95;
            }
            else
            {
                return 239;
            }
        }
    }

    public void DrawMapInEditor()
    {
        textureData[dataNum].UpdateMeshHeight(terrainMaterial, terrainData[dataNum].minHeight, terrainData[dataNum].maxHeight);
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData[dataNum].meshHeightMultiplier, terrainData[dataNum].meshHeightCurve, editorPreviewLOD, terrainData[dataNum].useFlatShading));
            //CreateNodes(mapData, Vector3.zero);
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }


    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MashData> callback, int biome, float hightLeft, float hightDown)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback, hightLeft, hightDown, biome);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MashData> callback, float hightLeft, float hightDown, int biome = dataNum)
    {
        MashData meshData;

        meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData[biome].meshHeightMultiplier, terrainData[biome].meshHeightCurve, lod, terrainData[biome].useFlatShading, hightLeft,hightDown);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MashData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MashData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);


        if (terrainData[dataNum].useFalloff)
        {

            if (falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    if (terrainData[dataNum].useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }

                }
            }

        }

        return new MapData(noiseMap);
    }

    void OnValidate()
    {

        if (terrainData != null)
        {
            terrainData[dataNum].onValuesUpdated -= OnValuesUpdated;
            terrainData[dataNum].onValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.onValuesUpdated -= OnValuesUpdated;
            noiseData.onValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData[dataNum].onValuesUpdated -= OnTextureValuesUpdated;
            textureData[dataNum].onValuesUpdated += OnTextureValuesUpdated;
        }

    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

    public bool CreateNodes(MapData mapData, Vector3 cornerCords)
    {
        if (mapData.heightMap == null)
        {
            Debug.LogError("no heightmap found");
            return false;
        }
        canDrawGizmos = false;
        nodeParent.transform.rotation = Quaternion.identity;

        print(terrainData[dataNum].meshHeightCurve.Evaluate(mapData.heightMap[0, 0]) * terrainData[dataNum].meshHeightMultiplier);
        for (int x = 0; x < mapChunkSize; x += nodeDensity)
        {
            for (int z = 0; z < mapChunkSize; z += nodeDensity)
            {
                float height = terrainData[dataNum].meshHeightCurve.Evaluate(mapData.heightMap[z, x]) * terrainData[dataNum].meshHeightMultiplier;
                float xPos = (x - mapChunkSize / 2) + cornerCords.x;
                float zPos = (z - mapChunkSize / 2) + cornerCords.y;
                Vector3 pos = new Vector3(xPos, height + 1, zPos);

                bool canMakeNode = true;
                foreach (Node n in nodeList)
                {
                    if (Vector3.Distance(pos, n.transform.position) <= nodeDensity)
                    {
                        canMakeNode = false;
                    }
                }

                if (canMakeNode)
                {
                    Node node = Instantiate(nodePrefab, pos, Quaternion.identity, nodeParent.transform);
                    nodeList.Add(node);
                }
            }
        }
        
        CreateConections();

        return true;
    }

    void CreateConections()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int j = i + 1; j < nodeList.Count; j++)
            {
                if(Vector2.Distance(new Vector2(nodeList[i].transform.position.x, nodeList[i].transform.position.z), new Vector2(nodeList[j].transform.position.x, nodeList[j].transform.position.z)) <= nodeDensity + (nodeDensity / 2) + 5f)
                {
                    ConnectNodes(nodeList[i], nodeList[j]);
                    ConnectNodes(nodeList[j], nodeList[i]);
                }
            }
        }

        canDrawGizmos = true;
        nodeParent.transform.Rotate(new Vector3(0, 90, 0));
        SpanwAI();
    }

    void ConnectNodes(Node from, Node to)
    {
        if (from == to) { return; }

        from.conactions.Add(to);
    }

    void SpanwAI()
    {
        Node randNode = nodeList[UnityEngine.Random.Range(0, nodeList.Count)];

        NPC newNPC = Instantiate(npc, randNode.transform.position, Quaternion.identity).GetComponent<NPC>();

        //newNPC.transform.position += Vector3.up * 20;

        newNPC.currentNode = randNode;
    }

    private void OnDrawGizmos()
    {
        if(canDrawGizmos)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < nodeList.Count; i++)
            {
                for (int j = 0; j < nodeList[i].conactions.Count; j++)
                {
                    Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i].conactions[j].transform.position);
                }
            }
        }
    }
}


public struct MapData
{
    public readonly float[,] heightMap;


    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}