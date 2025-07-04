﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.UIElements;

public static class MeshGenerator
{
    public static MashData GenerateTerrainMesh(float[,] heightmap, float heightMultipleir, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading, float OldheightMultipleirX = 0.0f, float OldheightMultipleirZ = 0.0f)
    {
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int borderedSize = heightmap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplifide = borderedSize - 2;
        float topLeftX = (meshSizeUnsimplifide - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplifide - 1) / 2f;

        if (OldheightMultipleirX == 0)
            OldheightMultipleirX = heightMultipleir;
        if(OldheightMultipleirZ == 0)
            OldheightMultipleirZ = heightMultipleir;

        Debug.Log($"heightMultipleir: {heightMultipleir}, newheightMultipleirX: {OldheightMultipleirX}, newheightMultipleirZ: {OldheightMultipleirZ}");

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MashData meshData = new MashData(verticesPerLine, useFlatShading);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if(isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);

                float worldX = topLeftX + percent.x * meshSizeUnsimplifide;
                float worldZ = topLeftZ + percent.y * meshSizeUnsimplifide;

                float blendStartX = topLeftX;
                float blendEndX = topLeftX + meshSizeUnsimplifide / 2;

                float blendStartZ = topLeftZ;
                float blendEndZ = topLeftZ + meshSizeUnsimplifide / 2;

                float blendFactorX = Mathf.Clamp01(Mathf.InverseLerp(blendStartX, blendEndX, worldX));

                float biomeBlendX = Mathf.SmoothStep(0, 1, blendFactorX);
                float lerpBlendX = Mathf.Lerp(heightMultipleir, OldheightMultipleirX, biomeBlendX);


                float blendFactorZ = Mathf.Clamp01(Mathf.InverseLerp(blendStartZ, blendEndZ, worldZ));
                float biomeBlendZ = Mathf.SmoothStep(0, 1, blendFactorZ);
                float lerpBlendZ = Mathf.Lerp(heightMultipleir, OldheightMultipleirZ, biomeBlendZ);

                float finalBlend = (lerpBlendX + lerpBlendZ) / 2;

                float baseHeight = heightmap[x, y];
                float height = heightCurve.Evaluate(baseHeight) * finalBlend;

                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplifide, height, topLeftZ - percent.y * meshSizeUnsimplifide);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTriangle(a,d,c);
                    meshData.AddTriangle(d,a,b);
                }

                vertexIndex++;
            }
        }

        meshData.Finelize();

        return meshData;

    }
}

public class MashData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int trianglesIndex;
    int borderTrianglesIndex;

    bool useFlatShading;

    public MashData(int verticesPerLine, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTrianglesIndex] = a;
            borderTriangles[borderTrianglesIndex + 1] = b;
            borderTriangles[borderTrianglesIndex + 2] = c;
            borderTrianglesIndex += 3;
        }
        else
        {
            triangles[trianglesIndex] = a;
            triangles[trianglesIndex + 1] = b;
            triangles[trianglesIndex + 2] = c;
            trianglesIndex += 3;
        }
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalsFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] = triangleNormal;
            vertexNormals[vertexIndexB] = triangleNormal;
            vertexNormals[vertexIndexC] = triangleNormal;
        }
        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalsFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0)
                vertexNormals[vertexIndexA] = triangleNormal;
            if (vertexIndexB >= 0)
                vertexNormals[vertexIndexB] = triangleNormal;
            if (vertexIndexC >= 0)
                vertexNormals[vertexIndexC] = triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalsFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void Finelize()
    {
        if (useFlatShading)
            FlatShading();
        else
            bakeNormals();
    }

    void bakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if (useFlatShading)
            mesh.RecalculateNormals();
        else
            mesh.normals = bakedNormals;
        return mesh;    
    }
}