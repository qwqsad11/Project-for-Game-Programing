using System.Collections;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float worldYOffset = 0.45f;

    public bool IsJumping { get; private set; }

    public IEnumerator JumpRoutine(
        Transform target,
        Vector3 start,
        Vector3 end,
        float jumpDuration,
        float jumpHeight,
        float squashAmount,
        float stretchAmount,
        System.Action onLand)
    {
        IsJumping = true;

        start.y += worldYOffset;
        end.y += worldYOffset;

        Vector3 baseScale = target.localScale;
        Quaternion baseRotation = target.rotation;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            Vector3 position = Vector3.Lerp(start, end, t);
            position.y = Mathf.Lerp(start.y, end.y, t) + arc;
            target.position = position;

            float squashPhase = t < 0.5f ? t / 0.5f : (1f - t) / 0.5f;
            float squash = Mathf.Lerp(1f, 1f - squashAmount, squashPhase);
            float stretch = Mathf.Lerp(1f, 1f + stretchAmount, squashPhase);
            target.localScale = new Vector3(baseScale.x * squash, baseScale.y * stretch, baseScale.z * squash);

            yield return null;
        }

        target.position = end;
        target.localScale = baseScale;
        target.rotation = baseRotation;
        if (cameraFollow == null)
        {
            cameraFollow = FindObjectOfType<CameraFollow>();
        }

        if (cameraFollow != null)
        {
            cameraFollow.TriggerShake();
        }

        IsJumping = false;
        onLand?.Invoke();
    }
}
