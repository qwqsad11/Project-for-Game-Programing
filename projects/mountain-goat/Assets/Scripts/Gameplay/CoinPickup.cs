using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private GameObject consumedVisual;
    [SerializeField] private bool destroyRootOnConsume = true;
    [SerializeField] private bool rotate = true;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 220f, 35f);
    [SerializeField] private bool floatAnimation = true;
    [SerializeField] private float floatAmplitude = 0.18f;
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private bool spawnCollectBurst = true;
    [SerializeField] private Color feedbackColor = new Color(1f, 0.78f, 0.08f, 1f);

    private bool collected;
    private Collider pickupCollider;
    private Vector3 startLocalPosition;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        pickupCollider = GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
        else
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.45f;
            pickupCollider = sphereCollider;
        }

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        ResetAnimationOrigin();
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        if (rotate)
        {
            transform.Rotate(rotationAxis * Time.deltaTime, Space.Self);
        }

        if (floatAnimation)
        {
            float bob = Mathf.Sin((Time.time + transform.GetInstanceID() * 0.01f) * floatSpeed) * floatAmplitude;
            transform.localPosition = startLocalPosition + Vector3.up * bob;
        }
    }

    public void ResetAnimationOrigin()
    {
        startLocalPosition = transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other != null ? other.gameObject : null);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other != null ? other.gameObject : null);
    }

    private void TryCollect(GameObject otherObject)
    {
        if (collected || otherObject == null)
        {
            return;
        }

        GoatController goatController = otherObject.GetComponentInParent<GoatController>() ??
                                        otherObject.GetComponentInChildren<GoatController>() ??
                                        otherObject.GetComponent<GoatController>();
        if (goatController == null)
        {
            return;
        }

        collected = true;
        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(coinValue);
            GameManager.Instance.AddScore(1);
        }

        SpawnCollectBurst();

        if (consumedVisual != null)
        {
            consumedVisual.SetActive(false);
        }

        if (destroyRootOnConsume)
        {
            StartCoroutine(CollectDisappearRoutine());
        }
    }

    private void SpawnCollectBurst()
    {
        if (!spawnCollectBurst)
        {
            return;
        }

        GameObject burstObject = new GameObject("Coin Collect Burst");
        burstObject.transform.position = transform.position;

        ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.32f;
        main.startSpeed = 2.2f;
        main.startSize = 0.13f;
        main.startColor = feedbackColor;
        main.maxParticles = 24;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 18)
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;

        Renderer coinRenderer = GetComponentInChildren<Renderer>();
        ParticleSystemRenderer particleRenderer = burstObject.GetComponent<ParticleSystemRenderer>();
        if (coinRenderer != null && particleRenderer != null)
        {
            particleRenderer.material = coinRenderer.sharedMaterial;
        }

        Destroy(burstObject, 0.8f);
    }

    private System.Collections.IEnumerator CollectDisappearRoutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 punchScale = originalScale * 1.25f;
        float punchDuration = 0.06f;
        float shrinkDuration = 0.1f;

        for (float timer = 0f; timer < punchDuration; timer += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, punchScale, timer / punchDuration);
            yield return null;
        }

        for (float timer = 0f; timer < shrinkDuration; timer += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(punchScale, Vector3.zero, timer / shrinkDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}
