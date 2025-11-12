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
        public const float ATTACK_LEAN_ANGLE = -22.5f;
        public const float HIT_LEAN_ANGLE = 45f;
        public const float HIT_REACT_DURATION = 0.1f;
        public const float HIT_FLASH_DURATION = 0.1f;
        public const float KNOCKBACK_DISTANCE = -10f;
    }
}
