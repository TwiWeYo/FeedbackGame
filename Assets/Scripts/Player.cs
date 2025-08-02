using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private MovementAnimator movementAnimator;

    public MovementAnimator Animator => movementAnimator;
}