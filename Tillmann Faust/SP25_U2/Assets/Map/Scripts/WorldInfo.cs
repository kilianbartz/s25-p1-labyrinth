using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;

public class GridWorldInfo
{
    public Vector2 WorldSize { get; }
    public Vector3 WorldOrigin { get; }
    public Vector3 WorldCenter { get; }
    public float WorldScaleFactor { get; }
    public string[][] WorldLayout { get; }

    public GridWorldInfo(
        Vector2 WorldSize,
        Vector3 WorldOrigin,
        Vector3 WorldCenter,
        float WorldScaleFactor,
        string[][] WorldLayout
    )
    {
        this.WorldSize = WorldSize;
        this.WorldOrigin = WorldOrigin;
        this.WorldCenter = WorldCenter;
        this.WorldScaleFactor = WorldScaleFactor;
        this.WorldLayout = WorldLayout;
    }
}