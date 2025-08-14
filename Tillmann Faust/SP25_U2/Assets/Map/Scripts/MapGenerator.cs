using UnityEngine;
using System;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    #region GameObjects
    public GameObject PlanePrefab;
    public GameObject CubePrefab;
    public GameObject PlayerPrefab;
    public GameObject NpcPrefab;
    #endregion
    #region Map-Gen Values
    public Vector3 WorldCenter;
    public Vector3 WorldOrigin;
    public Vector2 WorldSize;
    public float WorldScaleFactor;
    public TextAsset ASCIIMapFile;
    #endregion
    private string[][] MapStringArray;
    private Transform PlayerTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Map extraction
        MapStringArray = ASCIIMapTools.DetextifyASCIIMap(ASCIIMapFile.text);
        WorldSize = new Vector2(MapStringArray.Length, MapStringArray[0].Length);
        // Setze die Ecke der Plane auf (0,0,0), damit nicht die Koordinaten für jedes andere Objekt umgerechnet werden müsse
        WorldOrigin = new Vector3(0f, 0f, 0f);
        WorldCenter = WorldOrigin + new Vector3(WorldSize.x, 0f, WorldSize.y)*WorldScaleFactor*0.5f;
        foreach (string[] arr in MapStringArray)
            Debug.Log(string.Join("", arr));
        Debug.Log(MapStringArray[0][0]);

        // Game Object generation
        InstantiateGround();
        InstantiateObstacles();
        InstaniatePlayer();
        InstantiateNpcs();
    }

    void InstantiateGround()
    {
        // Plane object is 10x10 units per default so it has to be scaled appropriately
        Vector2 groundScale = WorldSize * 0.1f * WorldScaleFactor;
        GameObject ground = Instantiate(PlanePrefab, WorldCenter, Quaternion.identity);
        ground.transform.localScale = new Vector3(groundScale.x, 0.01f, groundScale.y);
        Debug.Log(ground.transform.lossyScale);
        Debug.Log(ground.transform.rotation.eulerAngles);
        /*
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // damit es wie eine Plane im XZ liegt
        quad.transform.localScale = new Vector3(groundScale.x, 1f, groundScale.y); // hier ist Y die "Höhe" auf der Fläche, nicht die Welt-Y
        quad.transform.position = WorldCenter;
        */

    }
    void InstantiateObstacles()
    {
        for (int i = 0; i < WorldSize.x; i++)
        {
            for (int j = 0; j < WorldSize.y; j++)
            {
                if (MapStringArray[i][j] != "1") continue;  // Ignorier nicht-obstacles
                GameObject obstacle = Instantiate(
                    CubePrefab,
                    WorldOrigin + new Vector3(i + 0.5f, 0.5f, j + 0.5f) * WorldScaleFactor, // Center of Grid Cell
                    Quaternion.identity
                );
                obstacle.transform.localScale = new Vector3(WorldScaleFactor, WorldScaleFactor, WorldScaleFactor);
            }
        }
    }

    void InstaniatePlayer()
    {
        for (int i = 0; i < WorldSize.x; i++)
        {
            for (int j = 0; j < WorldSize.y; j++)
            {
                if (MapStringArray[i][j] != "P") continue;  // Ignorier nicht-obstacles
                GameObject Player = Instantiate(
                    PlayerPrefab,
                    WorldOrigin + new Vector3(i + 0.5f, 0.5f, j + 0.5f) * WorldScaleFactor,
                    Quaternion.identity
                );
                PlayerTransform = Player.transform;
                return;
            }
        }
    }

    void InstantiateNpcs()
    {
        for (int i = 0; i < WorldSize.x; i++)
        {
            for (int j = 0; j < WorldSize.y; j++)
            {
                if (MapStringArray[i][j] != "E") continue;  // Ignorier nicht-obstacles
                GameObject npc = Instantiate(
                    NpcPrefab,
                    WorldOrigin + new Vector3(i + 0.5f, 0.5f, j + 0.5f) * WorldScaleFactor,
                    Quaternion.identity
                );
                npc.GetComponent<NpcStateManager>().BehaviorController = new LLMGridBehaviorController(
                    npc: npc.GetComponent<NpcStateManager>(),
                    new GridWorldInfo(
                        this.WorldSize,
                        this.WorldOrigin,
                        this.WorldCenter,
                        this.WorldScaleFactor,
                        this.MapStringArray
                    ),
                    PlayerTransform
                );
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
