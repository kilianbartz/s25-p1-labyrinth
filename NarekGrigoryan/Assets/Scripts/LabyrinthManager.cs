using System;
using UnityEngine;
using TMPro;   //  << für 3-D-Text

#region Datenstrukturen ------------------------------------------------------
[Serializable]
public class LabyrinthConfig { public string[] grid; public LightSettings lights; }

[Serializable]
public class LightSettings  { public int count; public string type; }
#endregion -------------------------------------------------------------------

public class LabyrinthManager : MonoBehaviour
{
    [Header("JSON  (Assets/Resources/)")]
    public TextAsset jsonFile;                        // labyrinth.json hier zuweisen

    [Header("Wand-Materialien")]
    public Material absorbingMaterial;   // A
    public Material reflectiveMaterial;  // R
    public Material transparentMaterial; // T

    [Header("Raster-Parameter")]
    public float cellSize      = 2f;    // Gangbreite
    public float wallHeight    = 2f;    // Wandhöhe
    public float wallThickness = 0.25f; // Wandbreite

    [Header("END-Text")]
    public Color endColor   = new Color(1f, 0.95f, 0.2f); // Gelb
    public float endSize    = 10f;   // TMP-FontSize
    public float endScaleXZ = 1.6f;  // Größe Trigger / Schrift (Meter)

    //-----------------------------------------------------------------------
    void Start()
    {
        // ---------- JSON laden ----------
        if (jsonFile == null) { Debug.LogError("JSON fehlt!"); return; }
        var cfg = JsonUtility.FromJson<LabyrinthConfig>(jsonFile.text);
        if (cfg?.grid == null || cfg.grid.Length == 0) { Debug.LogError("Grid leer!"); return; }

        int rows = cfg.grid.Length;
        int cols = cfg.grid[0].Length;

        // ---------- Wände & END-Feld aufbauen ----------
        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < cols; x++)
            {
                char symbol = cfg.grid[z][x];

                if (symbol == '_') continue;           // Freier Gang

                if (symbol == 'G')                     // END-Feld
                {
                    SpawnEnd(x, z);
                    continue;
                }

                // -------- Wand anlegen ----------
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{x}_{z}_{symbol}";
                wall.transform.position =
                    new Vector3(x * cellSize, wallHeight * 0.5f, z * cellSize);
                wall.transform.localScale =
                    new Vector3(wallThickness, wallHeight, wallThickness);

                var mr = wall.GetComponent<MeshRenderer>();
                mr.material = symbol switch
                {
                    'A' => absorbingMaterial,
                    'R' => reflectiveMaterial,
                    'T' => transparentMaterial,
                    _ => mr.material
                }; 

            
                var collider = wall.GetComponent<BoxCollider>();
                collider.size = Vector3.one;       // Korrigiert die Collidergröße (in lokalen Koordinaten)
                collider.center = Vector3.zero;    // Center-Offset auf 0 (Standard)
                                
            }
        }

        // ---------- Boden-Lichter einsetzen ----------
        //if (cfg.lights != null && cfg.lights.type == "Point" && cfg.lights.count > 0)
            SpawnFloorLights(cfg, cols, rows);

        Debug.Log("Labyrinth fertig aufgebaut ✅");
    }

    // ---------------------------------------------------------------------
    void SpawnEnd(int gx, int gz)
    {
        // Mittelpunkt der Zelle, knapp über Boden
        Vector3 pos = new Vector3(gx * cellSize, 0.02f, gz * cellSize);

        // GameObject + TextMeshPro
        var go = new GameObject($"End_{gx}_{gz}");
        go.transform.SetParent(transform);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // flach liegend

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text        = "END";
        tmp.fontSize    = endSize;
        tmp.alignment   = TextAlignmentOptions.Center;
        tmp.color       = endColor;
        tmp.enableWordWrapping = false;
        tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(endScaleXZ, endScaleXZ);

        // Trigger-Box
        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size      = new Vector3(endScaleXZ, 0.2f, endScaleXZ);
        box.center    = new Vector3(0, 0.1f, 0);

        // Einmaliger Console-Jubel
        go.AddComponent<EndTrigger>();
    }

    // ---------------------------------------------------------------------
    void SpawnFloorLights(LabyrinthConfig cfg, int cols, int rows)
        {
            System.Random rng = new System.Random();
            int placed = 0, safety = 1000;      // etwas mehr Versuche für strengere Bedingung

            // Emissives Material für Leuchtdisks
            var diskMat = new Material(Shader.Find("HDRP/Lit"));
            Color diskCol = new Color(1f, 0.93f, 0.6f);
            diskMat.SetColor("_BaseColor",     diskCol);
            diskMat.SetColor("_EmissiveColor", diskCol * 10f);
            diskMat.EnableKeyword("_EMISSIVE_COLOR");

            const float diskDia = 0.35f;
            const float diskHgt = 0.05f;

            // Hilfsfunktion: prüft, ob alle 8 Nachbarn frei sind
            bool IsCentral(int x, int z)
            {
                for (int dz = -1; dz <= 1; dz++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        int nz = z + dz;
                        if (nx < 0 || nx >= cols || nz < 0 || nz >= rows) return false;
                        if (cfg.grid[nz][nx] != '_') return false;   // nicht frei
                    }
                return true;
            }

            while (placed < cfg.lights.count && safety-- > 0)
            {
                int gx = rng.Next(cols);
                int gz = rng.Next(rows);
                if (!IsCentral(gx, gz)) continue;            // nur „Mitte“-Felder

                Vector3 pos = new Vector3(gx * cellSize, diskHgt * 0.5f, gz * cellSize);

                // Punktlicht
                var lightGO = new GameObject($"PointLight_{placed}");
                lightGO.transform.SetParent(transform);
                lightGO.transform.position = pos;

                var li = lightGO.AddComponent<Light>();
                li.type      = LightType.Point;
                li.color     = diskCol;
                li.intensity = 12000f;
                li.range     = cellSize * 6f;

                var hd = lightGO.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
                hd.SetIntensity(li.intensity, UnityEngine.Rendering.LightUnit.Lumen);
                hd.volumetricDimmer = 1f;

                // Leuchtdisk
                var disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                disk.transform.SetParent(lightGO.transform, false);
                disk.transform.localScale = new Vector3(diskDia, diskHgt * 0.5f, diskDia);
                disk.GetComponent<MeshRenderer>().material = diskMat;
                Destroy(disk.GetComponent<Collider>());

                placed++;
            }
        }
}