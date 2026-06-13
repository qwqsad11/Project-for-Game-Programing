using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private GameObject consumedVisual;
    [SerializeField] private bool destroyRootOnConsume = true;
    [SerializeField] private bool rotate = true;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 120f, 0f);

    private bool collected;
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

    private void Update()
    {
        if (collected)
        {
            return;
        }

        if (rotate && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            transform.Rotate(rotationAxis * Time.deltaTime, Space.Self);
        }
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(coinValue);
            GameManager.Instance.AddScore(1);
        }

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
