using UnityEngine;

public static class DebugExtension
{
    public static void DrawWireSphere(Vector3 center, Color color, float radius, int segments = 16)
    {
        float step = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * step;
            float angle2 = (i + 1) * step;

            Vector3 offset1 = new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
            Vector3 offset2 = new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;

            Debug.DrawLine(center + offset1, center + offset2, color);
        }
    }

    public static void DrawDebugBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color, float duration = 0)
    {
        Vector3[] points = new Vector3[8];
        Matrix4x4 matrix = Matrix4x4.TRS(center, orientation, Vector3.one);

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? -halfExtents.x : halfExtents.x,
                (i & 2) == 0 ? -halfExtents.y : halfExtents.y,
                (i & 4) == 0 ? -halfExtents.z : halfExtents.z
            );
            points[i] = matrix.MultiplyPoint3x4(corner);
        }

        int[,] edges = {
        {0,1},{1,3},{3,2},{2,0},
        {4,5},{5,7},{7,6},{6,4},
        {0,4},{1,5},{2,6},{3,7}
    };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            Debug.DrawLine(points[edges[i, 0]], points[edges[i, 1]], color, duration);
        }
    }
}
