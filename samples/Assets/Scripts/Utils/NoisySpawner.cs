using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SpawnerArgs
{
    public GameObject ObjectToSpawn;
    public int SpawnCount;
    public float spawnRadius;
    public float minSpawnRadius;
    public float objectRadius;
}

public static class NoisySpawner
{
    public static List<Vector3> GetSpawnPoints(SpawnerArgs args, Vector3 origin)
    {
        List<Vector3> generatedPoints = new List<Vector3>();
        int attempts = 30;

        Vector3 center = origin;
        Vector3 firstPoint = GenerateRandomPoint(center, args.spawnRadius, args.minSpawnRadius);
        generatedPoints.Add(firstPoint);

        while (generatedPoints.Count < args.SpawnCount)
        {
            bool pointAdded = false;

            for (int i = 0; i < attempts; i++)
            {
                Vector3 basePoint = generatedPoints[UnityEngine.Random.Range(0, generatedPoints.Count)];

                Vector3 newPoint = basePoint + UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(args.objectRadius, args.spawnRadius);
                newPoint.y = center.y;

                if (Vector3.Distance(center, newPoint) <= args.spawnRadius
                    && Vector3.Distance(center, newPoint) >= args.minSpawnRadius
                    && IsFarEnough(newPoint, generatedPoints, args.objectRadius))
                {
                    generatedPoints.Add(newPoint);
                    pointAdded = true;
                    break;
                }
            }

            if (!pointAdded)
            {
                Debug.LogWarning("Could not place all points within constraints. Reduce spawn count or radius.");
                break;
            }
        }

        return generatedPoints;
    }

    private static Vector3 GenerateRandomPoint(Vector3 center, float maxRadius, float minRadius)
    {
        while (true)
        {
            Vector3 point = center + UnityEngine.Random.insideUnitSphere * maxRadius;
            point.y = center.y;
            if (Vector3.Distance(center, point) >= minRadius)
            {
                return point;
            }
        }
    }

    private static bool IsFarEnough(Vector3 point, List<Vector3> points, float minDist)
    {
        foreach (Vector3 otherPoint in points)
        {
            if (Vector3.Distance(point, otherPoint) < minDist)
            {
                return false;
            }
        }
        return true;
    }
}
