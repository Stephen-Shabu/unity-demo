using UnityEngine;

namespace GameEvents
{
    public class EventsCollection
    {
        public class MainGameEvents
        {
            public delegate void CameraEvent(Transform newTarget);
            public static event CameraEvent OnCameraTargetChanged;

            public static void RaiseCameraTargetChanged(Transform newTarget)
            {
                if (OnCameraTargetChanged != null)
                {
                    OnCameraTargetChanged(newTarget);
                }
            }
        }
    }
}

