using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreasureChestPickup : MonoBehaviour
{
    public enum ChestRarity
    {
        Common = 0,   // 木箱 — TreasureChest_0
        Rare = 1,     // 铁箱 — TreasureChest_1
        Epic = 2,     // 金箱 — TreasureChest_2
        Legendary = 3 // 水晶箱 — TreasureChest_3
    }

    [Header("Rarity Config")]
    [SerializeField] private ChestRarity rarity = ChestRarity.Common;

    [Header("Reward Ranges")]
    [SerializeField] private Vector2Int commonCoinRange = new Vector2Int(5, 15);
    [SerializeField] private Vector2Int rareCoinRange = new Vector2Int(20, 40);
    [SerializeField] private Vector2Int epicCoinRange = new Vector2Int(50, 100);
    [SerializeField] private Vector2Int legendaryCoinRange = new Vector2Int(150, 300);

    [Header("Rarity Colors")]
    [SerializeField] private Color commonBurstColor = new Color(0.82f, 0.55f, 0.28f, 1f);   // Bronze
    [SerializeField] private Color rareBurstColor = new Color(0.75f, 0.78f, 0.82f, 1f);     // Silver
    [SerializeField] private Color epicBurstColor = new Color(1f, 0.85f, 0.15f, 1f);        // Gold
    [SerializeField] private Color legendaryBurstColor = new Color(0.70f, 0.35f, 0.95f, 1f); // Purple

    [Header("Rarity Glow")]
    [SerializeField] private bool enableRarityGlow = true;
    [SerializeField] private float legendaryGlowPulseSpeed = 2.5f;
    [SerializeField] private float legendaryGlowIntensity = 0.35f;
    [SerializeField] private Color legendaryGlowColor = new Color(0.7f, 0.3f, 1f, 0.6f);

    [Header("Feedback")]
    [SerializeField] private bool destroyAfterOpen = true;
    [SerializeField] private float openScalePunch = 1.15f;

    private bool opened;
    private Collider chestCollider;
    private Animator animator;
    private int coinReward;
    private Color burstColor;
    private MaterialPropertyBlock glowPropertyBlock;
    private Renderer[] childRenderers;
    private float glowTimer;

    public bool IsOpened => opened;
    public ChestRarity Rarity => rarity;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        ConfigurePhysics();
        CacheRenderers();
        ApplyRarityConfig();
    }

    private void Update()
    {
        if (!opened && enableRarityGlow && rarity == ChestRarity.Legendary)
        {
            PulseLegendaryGlow();
        }
    }

    /// <summary>
    /// Called after instantiation to set the rarity (overrides serialized field).
    /// </summary>
    public void Initialize(ChestRarity chestRarity)
    {
        rarity = chestRarity;
        ApplyRarityConfig();
    }

    public bool TryOpen(GoatController opener = null)
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(coinReward);
            GameManager.Instance.AddScore(coinReward);
        }

        PlayOpenAnimation();
        SpawnCoinBurst();
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

    private void CacheRenderers()
    {
        childRenderers = GetComponentsInChildren<Renderer>(true);
        glowPropertyBlock = new MaterialPropertyBlock();
    }

    private void ApplyRarityConfig()
    {
        coinReward = rarity switch
        {
            ChestRarity.Common => Random.Range(commonCoinRange.x, commonCoinRange.y + 1),
            ChestRarity.Rare => Random.Range(rareCoinRange.x, rareCoinRange.y + 1),
            ChestRarity.Epic => Random.Range(epicCoinRange.x, epicCoinRange.y + 1),
            ChestRarity.Legendary => Random.Range(legendaryCoinRange.x, legendaryCoinRange.y + 1),
            _ => Random.Range(commonCoinRange.x, commonCoinRange.y + 1)
        };

        burstColor = rarity switch
        {
            ChestRarity.Common => commonBurstColor,
            ChestRarity.Rare => rareBurstColor,
            ChestRarity.Epic => epicBurstColor,
            ChestRarity.Legendary => legendaryBurstColor,
            _ => commonBurstColor
        };

        ApplyRarityTint();
    }

    private void ApplyRarityTint()
    {
        Color tint = rarity switch
        {
            ChestRarity.Common => Color.white,
            ChestRarity.Rare => new Color(0.82f, 0.85f, 0.90f, 1f),
            ChestRarity.Epic => new Color(1f, 0.92f, 0.65f, 1f),
            ChestRarity.Legendary => new Color(0.85f, 0.75f, 1f, 1f),
            _ => Color.white
        };

        if (childRenderers == null)
        {
            childRenderers = GetComponentsInChildren<Renderer>(true);
        }

        for (int i = 0; i < childRenderers.Length; i++)
        {
            Renderer renderer = childRenderers[i];
            if (renderer == null) continue;

            renderer.GetPropertyBlock(glowPropertyBlock);
            glowPropertyBlock.SetColor("_Color", tint);
            renderer.SetPropertyBlock(glowPropertyBlock);
        }
    }

    private void PulseLegendaryGlow()
    {
        glowTimer += Time.deltaTime * legendaryGlowPulseSpeed;
        float pulse = (Mathf.Sin(glowTimer) + 1f) * 0.5f;
        Color emissionColor = Color.Lerp(Color.black, legendaryGlowColor, pulse * legendaryGlowIntensity);

        if (childRenderers == null) return;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            Renderer renderer = childRenderers[i];
            if (renderer == null) continue;

            renderer.GetPropertyBlock(glowPropertyBlock);
            glowPropertyBlock.SetColor("_EmissionColor", emissionColor);
            renderer.SetPropertyBlock(glowPropertyBlock);
        }
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

    private void SpawnCoinBurst()
    {
        GameObject burstObject = new GameObject("Treasure Coin Burst");
        burstObject.transform.position = transform.position + Vector3.up * 0.45f;

        ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = rarity switch
        {
            ChestRarity.Legendary => 0.7f,
            ChestRarity.Epic => 0.55f,
            _ => 0.45f
        };
        main.startSpeed = rarity switch
        {
            ChestRarity.Legendary => 3.8f,
            ChestRarity.Epic => 3.2f,
            ChestRarity.Rare => 2.8f,
            _ => 2.6f
        };
        main.startSize = rarity switch
        {
            ChestRarity.Legendary => 0.22f,
            ChestRarity.Epic => 0.19f,
            _ => 0.16f
        };
        main.startColor = burstColor;
        main.maxParticles = rarity switch
        {
            ChestRarity.Legendary => 60,
            ChestRarity.Epic => 50,
            ChestRarity.Rare => 40,
            _ => 30
        };
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)Mathf.Clamp(coinReward / 2, 8, 40))
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.18f;

        // Legendary gets an extra sparkle ring
        if (rarity == ChestRarity.Legendary)
        {
            ParticleSystem.SubEmittersModule subEmitters = particles.subEmitters;
            // Simple second burst after a short delay using a separate particle system
            StartCoroutine(LegendarySecondBurst(burstObject.transform.position));
        }

        Destroy(burstObject, 1.2f);
    }

    private IEnumerator LegendarySecondBurst(Vector3 position)
    {
        yield return new WaitForSeconds(0.15f);

        GameObject ringObject = new GameObject("Legendary Ring Burst");
        ringObject.transform.position = position + Vector3.up * 0.2f;

        ParticleSystem ringParticles = ringObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule ringMain = ringParticles.main;
        ringMain.startLifetime = 0.5f;
        ringMain.startSpeed = 1.8f;
        ringMain.startSize = 0.12f;
        ringMain.startColor = legendaryBurstColor;
        ringMain.maxParticles = 35;
        ringMain.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule ringEmission = ringParticles.emission;
        ringEmission.rateOverTime = 0f;
        ringEmission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 20)
        });

        ParticleSystem.ShapeModule ringShape = ringParticles.shape;
        ringShape.shapeType = ParticleSystemShapeType.Circle;
        ringShape.radius = 0.35f;

        Destroy(ringObject, 1f);
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
