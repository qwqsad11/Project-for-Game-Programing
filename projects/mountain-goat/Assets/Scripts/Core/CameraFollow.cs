using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Rotation")]
    [SerializeField] private Vector3 cameraRotation = new Vector3(0f, -75f, 0f);

    [Header("Behind Follow")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 6.2f, -8.8f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private float lookAheadHeight = 1.2f;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.18f;
    [SerializeField] private bool centerTargetOnScreen = true;
    [SerializeField] private float targetScreenYOffset = 0f;
    [SerializeField] private float minFollowHeight = 0f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.08f;

    private Vector3 velocity;
    private Camera cachedCamera;
    private float shakeTimer;
    private Vector3 shakeOffset;
    private bool hasSnappedToTarget;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();
        if (cachedCamera != null)
        {
            cachedCamera.orthographic = true;
            cachedCamera.orthographicSize = 6.6f;
        }

        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    private void LateUpdate()
    {
        ResolveTarget();

        if (target == null)
        {
            return;
        }

        Vector3 desired = ComputeDesiredPosition();

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            shakeOffset = Random.insideUnitSphere * shakeStrength;
            shakeOffset.z = 0f;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        desired += shakeOffset;

        if (!hasSnappedToTarget)
        {
            transform.position = desired;
            velocity = Vector3.zero;
            hasSnappedToTarget = true;
        }
        else
        {
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.position = smoothed;
        }

        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    public void SetTarget(Transform newTarget)
    {
        if (target != newTarget)
        {
            hasSnappedToTarget = false;
            velocity = Vector3.zero;
        }

        target = newTarget;
    }

    private void ResolveTarget()
    {
        GoatController goat = FindObjectOfType<GoatController>();
        if (goat != null)
        {
            if (target == null || target != goat.transform)
            {
                SetTarget(goat.transform);
            }
            return;
        }

        if (target != null)
        {
            return;
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            SetTarget(player.transform);
        }
    }

    public void TriggerShake()
    {
        shakeTimer = Mathf.Max(shakeTimer, shakeDuration);
    }

    private Vector3 ComputeDesiredPosition()
    {
        Quaternion rotation = Quaternion.Euler(cameraRotation);
        Vector3 centeredOffset = centerTargetOnScreen
            ? new Vector3(0f, followOffset.y, followOffset.z)
            : followOffset;
        Vector3 desired = target.position + rotation * centeredOffset;
        if (centerTargetOnScreen)
        {
            desired.y = target.position.y + targetScreenYOffset;
        }
        return desired;
    }
}
