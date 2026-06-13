using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrassPickup : MonoBehaviour
{
    [SerializeField] private float hungerReduceAmount = 30f;
    [SerializeField] private GameObject consumedVisual;
    [SerializeField] private bool destroyRootOnConsume = true;

    private bool consumed;
    private Collider pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
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

    private void OnTriggerEnter(Collider other)
    {
        TryConsume(other != null ? other.gameObject : null);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryConsume(other != null ? other.gameObject : null);
    }

    private void TryConsume(GameObject otherObject)
    {
        if (consumed || otherObject == null)
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

        HungerSystem hungerSystem = goatController.GetComponent<HungerSystem>();
        if (hungerSystem == null)
        {
            hungerSystem = goatController.GetComponentInParent<HungerSystem>();
        }

        if (hungerSystem != null)
        {
            hungerSystem.ReduceHunger(hungerReduceAmount);
        }

        consumed = true;

        if (consumedVisual != null)
        {
            consumedVisual.SetActive(false);
        }

        if (destroyRootOnConsume)
        {
            Destroy(gameObject);
        }
    }
}
