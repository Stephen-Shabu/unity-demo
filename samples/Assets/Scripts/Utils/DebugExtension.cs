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
}
