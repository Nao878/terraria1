using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float zOffset = -10f;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            targetPosition.z = zOffset;
            transform.position = targetPosition;
        }
    }
}
