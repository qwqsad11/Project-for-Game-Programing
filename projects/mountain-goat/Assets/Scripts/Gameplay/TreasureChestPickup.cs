using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreasureChestPickup : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int minCoins = 5;
    [SerializeField] private int maxCoins = 20;

    [Header("Feedback")]
    [SerializeField] private bool destroyAfterOpen;
    [SerializeField] private float openScalePunch = 1.15f;
    [SerializeField] private Color coinBurstColor = new Color(1f, 0.72f, 0.08f, 1f);

    private bool opened;
    private Collider chestCollider;
    private Animator animator;

    public bool IsOpened => opened;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        ConfigurePhysics();
    }

    public bool TryOpen(GoatMovement goat)
    {
        if (opened)
        {
            return false;
        }

        opened = true;
        if (chestCollider != null)
        {
            chestCollider.enabled = false;
        }

        int reward = Random.Range(Mathf.Min(minCoins, maxCoins), Mathf.Max(minCoins, maxCoins) + 1);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(reward);
            GameManager.Instance.AddScore(reward);
        }

        PlayOpenAnimation();
        SpawnCoinBurst(reward);
        StartCoroutine(OpenFeedbackRoutine());
        return true;
    }

    private void ConfigurePhysics()
    {
        chestCollider = GetComponent<Collider>();
        if (chestCollider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.9f, 0.7f, 0.9f);
            boxCollider.center = new Vector3(0f, 0.35f, 0f);
            chestCollider = boxCollider;
        }

        chestCollider.isTrigger = true;

        Collider[] childColliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < childColliders.Length; i++)
        {
            childColliders[i].isTrigger = true;
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

    private void PlayOpenAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (HasAnimatorParameter("Open", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("Open");
        }
        else if (HasAnimatorParameter("isOpen", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("isOpen", true);
        }
        else
        {
            animator.Play(0, 0, 0f);
        }
    }

    private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter.name == parameterName && parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnCoinBurst(int reward)
    {
        GameObject burstObject = new GameObject("Treasure Coin Burst");
        burstObject.transform.position = transform.position + Vector3.up * 0.45f;

        ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.45f;
        main.startSpeed = 2.6f;
        main.startSize = 0.16f;
        main.startColor = coinBurstColor;
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)Mathf.Clamp(reward, 8, 28))
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.18f;

        Destroy(burstObject, 1f);
    }

    private IEnumerator OpenFeedbackRoutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 punchScale = originalScale * openScalePunch;
        float punchDuration = 0.08f;
        float settleDuration = 0.12f;

        for (float timer = 0f; timer < punchDuration; timer += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, punchScale, timer / punchDuration);
            yield return null;
        }

        for (float timer = 0f; timer < settleDuration; timer += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(punchScale, originalScale, timer / settleDuration);
            yield return null;
        }

        transform.localScale = originalScale;

        if (destroyAfterOpen)
        {
            Destroy(gameObject, 0.4f);
        }
    }
}
