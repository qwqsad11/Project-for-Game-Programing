using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    public Transform defaultTransform;
    public Vector3 offset = new Vector3(0f, 1.8f, -5f);
    public float moveSpeed = 4f;
    public float rotateSpeed = 8f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isResetting = true;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        if (defaultTransform != null)
        {
            SetTarget(defaultTransform.position + defaultTransform.TransformVector(offset), defaultTransform.position);
        }
        else
        {
            targetPosition = startPosition;
            targetRotation = startRotation;
        }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    public void FocusOn(Transform body)
    {
        if (body == null)
        {
            ResetView();
            return;
        }

        isResetting = false;
        Vector3 direction = (transform.position - body.position).normalized;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector3.back;
        }

        Vector3 desiredPosition = body.position + direction * 4f + Vector3.up * 1.2f;
        SetTarget(desiredPosition, body.position);
    }

    public void ResetView()
    {
        isResetting = true;
        targetPosition = startPosition;
        targetRotation = startRotation;
    }

    private void SetTarget(Vector3 position, Vector3 lookAtPoint)
    {
        targetPosition = position;
        targetRotation = Quaternion.LookRotation(lookAtPoint - position);
    }
}
