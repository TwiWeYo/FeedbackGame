using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private MovementAnimator movementAnimator;

    public MovementAnimator Animator => movementAnimator;

    [SerializeField]
    public EnemyBehaviors Behavior;
    [SerializeField]
    public int AttackId;

}

public enum EnemyBehaviors
{
    MoveCloser,
    MoveAway,
    Random
}
