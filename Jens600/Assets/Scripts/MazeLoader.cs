#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using UnityEngine;

public class MazeLoader : MonoBehaviour
{
    public string mazeFileName = "maze.txt";

    public GameObject wallPrefab;
    public GameObject reflectiveWallPrefab;
    public GameObject transparentWallPrefab;
    public GameObject playerPrefab;
    public GameObject goalPrefab;

    public float blockSize = 3f;
    public float wallWidth = 3f;
    public float wallHeight = 8f;
    public float playerSpawnHeight = 1.1f;

    public Transform mazeParent;

    public void GenerateMaze()
    {
        if (mazeParent == null)
        {
            Debug.LogError("Maze Parent is not assigned!");
            return;
        }

        // Vorheriges löschen
#if UNITY_EDITOR
        while (mazeParent.childCount > 0)
        {
            DestroyImmediate(mazeParent.GetChild(0).gameObject);
        }
#else
        foreach (Transform child in mazeParent)
        {
            Destroy(child.gameObject);
        }
#endif

        string path = Path.Combine(Application.dataPath, mazeFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"Maze file not found: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        for (int z = 0; z < lines.Length; z++)
        {
            string line = lines[z];
            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];
                Vector3 pos = new Vector3(x * blockSize, wallHeight / 2f, -z * blockSize);

                GameObject prefab = null;

                switch (c)
                {
                    case '#': prefab = wallPrefab; break;
                    case 'R': prefab = reflectiveWallPrefab; break;
                    case 'T': prefab = transparentWallPrefab; break;
                    case 'S':
                        Instantiate(playerPrefab, pos + new Vector3(0, playerSpawnHeight, 0), Quaternion.identity, mazeParent);
                        break;
                    case 'Z':
                        Instantiate(goalPrefab, pos + new Vector3(0, -1f, 0), Quaternion.identity, mazeParent);
                        break;
                }

                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, pos, Quaternion.identity, mazeParent);
                    go.transform.localScale = new Vector3(wallWidth, wallHeight, wallWidth);
                }
            }
        }

        Debug.Log("Maze generated.");
    }

    public void ClearMaze()
    {
    #if UNITY_EDITOR
        while (mazeParent != null && mazeParent.childCount > 0)
        {
            DestroyImmediate(mazeParent.GetChild(0).gameObject);
        }
        Debug.Log("Maze cleared.");
    #endif
    }

}
