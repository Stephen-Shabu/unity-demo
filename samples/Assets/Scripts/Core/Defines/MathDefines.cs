using UnityEngine;

public static class MathDefines
{
    public const float FULL_CIRCLE_DEG = (2 * Mathf.PI) * Mathf.Rad2Deg;
    public const int MILLISECOND_MULTIPLIER = 1000;

    public static float GetAngleFromDirectionXZ(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z);
    }

    public static float InterpolateAngle(float startAngle, float targetAngle, float t)
    {
        var diff = targetAngle - startAngle;

        diff += 2 * Mathf.PI;

        diff = (diff + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;

        var interpolatedaAngle = startAngle + t * diff;

        return interpolatedaAngle;
    }

    public static class Easing
    {
        public static float EaseInOut(float t)
        {
            return 1 - Mathf.Pow(1 - t, 2f);
        }
    }
}
