using UnityEngine;

public class GrassPickup : MonoBehaviour
{
    [SerializeField] private float hungerReduceAmount = 30f;
    [SerializeField] private GameObject consumedVisual;
    [SerializeField] private bool destroyRootOnConsume = true;
    [SerializeField] private float consumeDistance = 0.75f;

    private bool consumed;
    private Collider pickupCollider;
    private GoatController goat;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (consumed || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (goat == null)
        {
            goat = FindObjectOfType<GoatController>();
        }

        if (goat == null)
        {
            return;
        }

        bool overlapsCollider = pickupCollider != null &&
                                goat.TryGetComponent(out Collider goatCollider) &&
                                pickupCollider.bounds.Intersects(goatCollider.bounds);
        bool sameTileDistance = Vector3.Distance(goat.transform.position, transform.position) <= consumeDistance;

        if (overlapsCollider || sameTileDistance)
        {
            TryConsume(goat.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryConsume(other.gameObject);
    }

    private void TryConsume(GameObject otherObject)
    {
        if (consumed || otherObject == null)
        {
            return;
        }

        HungerSystem hungerSystem = ResolveHungerSystem(otherObject);
        if (hungerSystem == null)
        {
            return;
        }

        consumed = true;
        hungerSystem.ReduceHunger(hungerReduceAmount);

        if (consumedVisual != null)
        {
            consumedVisual.SetActive(false);
        }

        if (destroyRootOnConsume)
        {
            Destroy(gameObject);
        }
    }

    private static HungerSystem ResolveHungerSystem(GameObject otherObject)
    {
        HungerSystem hungerSystem = otherObject.GetComponent<HungerSystem>();
        if (hungerSystem != null)
        {
            return hungerSystem;
        }

        GoatController goatController = otherObject.GetComponent<GoatController>();
        if (goatController != null)
        {
            hungerSystem = goatController.GetComponent<HungerSystem>();
            if (hungerSystem != null)
            {
                return hungerSystem;
            }
        }

        hungerSystem = otherObject.GetComponentInParent<HungerSystem>();
        if (hungerSystem != null)
        {
            return hungerSystem;
        }

        return otherObject.GetComponentInChildren<HungerSystem>();
    }
}
