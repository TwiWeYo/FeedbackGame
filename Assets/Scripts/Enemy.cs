using UnityEngine;

public class Enemy : MonoBehaviour
{
    
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
