using System;
using UnityEngine;

public class DetectionComponent : MonoBehaviour
{

    public Action<Vector3> OnObstacleDetected; 

    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private int rayCount = 30;
    [SerializeField] private int height = 2;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private GameObject dObject;

    private Vector3 detectionRayOrigin = Vector3.zero;

    public void UpdateComponent()
    {
        float angleIncrement = fieldOfViewAngle / (rayCount - 1);
        GameObject detectedObject = null;
        detectionRayOrigin.x = transform.position.x;
        detectionRayOrigin.z = transform.position.z;
        detectionRayOrigin.y = height;

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = -fieldOfViewAngle / 2 + i * angleIncrement;
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

            Vector3 rayDirection = rotation * transform.forward;

            if (Physics.Raycast(detectionRayOrigin, rayDirection, out RaycastHit hit, detectionRadius, detectionLayer, QueryTriggerInteraction.Collide))
            {
                detectedObject = hit.collider.gameObject;

                break;
            }
        }

        if (detectedObject != null)
        {
            if (dObject != detectedObject || dObject.GetInstanceID() != detectedObject.GetInstanceID())
            {
                dObject = detectedObject;
                OnObstacleDetected?.Invoke(dObject.transform.position);
            }
        }
    }

    public void ResetDectectedObject()
    {
        dObject = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(detectionRayOrigin, detectionRadius);

        float angleIncrement = fieldOfViewAngle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = -fieldOfViewAngle / 2 + i * angleIncrement;
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

            Vector3 rayDirection = rotation * transform.forward;

            Gizmos.DrawRay(detectionRayOrigin, rayDirection * detectionRadius);
        }
    }
}
