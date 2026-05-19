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

    private void Awake()
    {
        followOffset = new Vector3(0f, 0f, -8.8f);
        lookOffset = Vector3.up * 1.1f;
        lookAheadHeight = 1.2f;
        targetScreenYOffset = 0f;
        smoothTime = 0.14f;

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

        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.position = smoothed;
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void ResolveTarget()
    {
        GoatController goat = FindObjectOfType<GoatController>();
        if (goat != null)
        {
            if (target == null || target != goat.transform)
            {
                target = goat.transform;
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
            target = player.transform;
        }
    }

    public void TriggerShake()
    {
        shakeTimer = Mathf.Max(shakeTimer, shakeDuration);
    }

    private Vector3 ComputeDesiredPosition()
    {
        Vector3 desired = target.position + followOffset;
        if (centerTargetOnScreen)
        {
            desired.y = target.position.y + targetScreenYOffset;
        }
        return desired;
    }
}
