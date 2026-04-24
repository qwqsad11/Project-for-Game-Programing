using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CelestialBody : MonoBehaviour
{
    public string bodyName = "Planet";
    [TextArea(2, 4)]
    public string description = "这是一个漂亮的天体。点击它可以看到更多信息。";
    public AudioClip clickSound;
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 0.8f;
    public float scalePulse = 1.2f;
    public float pulseSpeed = 4f;

    private Renderer[] renderers;
    private Color[] originalColors;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            if (mat.HasProperty("_Color"))
            {
                originalColors[i] = mat.color;
            }
            else if (mat.HasProperty("_BaseColor"))
            {
                originalColors[i] = mat.GetColor("_BaseColor");
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }

        originalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void OnSelected()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(SelectionFeedback());
    }

    private IEnumerator SelectionFeedback()
    {
        SetHighlightColor(highlightColor);
        float elapsed = 0f;
        Vector3 targetScale = originalScale * scalePulse;

        if (clickSound != null)
        {
            audioSource.clip = clickSound;
            audioSource.Play();
        }

        while (elapsed < highlightDuration)
        {
            float t = Mathf.Sin(elapsed * pulseSpeed) * 0.5f + 0.5f;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        RestoreOriginalColors();
        feedbackCoroutine = null;
    }

    private void SetHighlightColor(Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            if (mat.HasProperty("_Color"))
            {
                mat.color = color;
            }
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", color * 2f);
                mat.EnableKeyword("_EMISSION");
            }
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            if (mat.HasProperty("_Color"))
            {
                mat.color = originalColors[i];
            }
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
