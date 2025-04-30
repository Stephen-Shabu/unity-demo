using UnityEngine;

public static class MovementDefines
{
    public static class Camera
    {
        public const float MAGNITUDE_THRESHOLD = .25f;
    }

    public static class Character
    {
        public const int FLOOR_COLLIDER_COUNT = 1;
        public const float MAGNITUDE_THRESHOLD = .25f;
        public const float GROUNDED_COLLIDER_SIZE_MUTIPLIER = .25f;
        public const float INERTIA_DAMPER = .25f;
        public const float LUNGE_FORCE = 30f;
        public const float LEAN_ANGLE = -22.5f;
    }
}
