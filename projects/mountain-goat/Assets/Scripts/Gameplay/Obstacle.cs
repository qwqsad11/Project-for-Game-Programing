using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private bool rotate;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 90f, 0f);

    private void Update()
    {
        if (rotate && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            transform.Rotate(rotationAxis * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GoatController goat = other.GetComponent<GoatController>();
        if (goat != null && goat.CanDie)
        {
            goat.Die();
        }
    }
}
