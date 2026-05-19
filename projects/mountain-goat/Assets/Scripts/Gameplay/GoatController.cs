using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoatController : MonoBehaviour
{
    public static readonly Vector2Int UpperLeft = new Vector2Int(-1, 1);
    public static readonly Vector2Int UpperRight = new Vector2Int(1, 1);
    public static readonly Vector2Int LowerLeft = new Vector2Int(-1, -1);
    public static readonly Vector2Int LowerRight = new Vector2Int(1, -1);

    [SerializeField] protected Vector2Int currentGrid;
    [SerializeField] protected bool canDie = true;

    public virtual Vector2Int CurrentGrid => currentGrid;
    public virtual bool CanDie => canDie;

    public virtual void Die()
    {
    }

    public virtual void ApplySpringBoost()
    {
    }

    public virtual void QueueSpringHop()
    {
        ApplySpringBoost();
    }
}
