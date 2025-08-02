using UnityEngine;

namespace Assets.Scripts
{
    public class Shadow : MonoBehaviour
    {
        [SerializeField]
        private MovementAnimator movementAnimator;

        public MovementAnimator Animator => movementAnimator;
    }
}
