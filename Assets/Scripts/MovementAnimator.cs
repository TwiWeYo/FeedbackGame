using System;
using System.Collections;
using UnityEngine;

public class MovementAnimator : MonoBehaviour
{
    [SerializeField]
    public float speed = 10f;
    [SerializeField]
    public float deathSpeed = 0.2f;

    private Vector3? targetPosition;
    
    private GameObject currentTarget;
    private bool isMoving = false;
    private bool isDying = false;

    public event Action OnMovementCompletedEvent;
    public event Action OnDeadEvent;

    public void AddTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }

    public void StartMovement(GameObject target)
    {
        if (isMoving)
        {
            Debug.LogWarning("Движение уже выполняется!");
            return;
        }

        if (!targetPosition.HasValue)
        {
            Debug.LogWarning("Нет целевых позиций для движения!");
            return;
        }

        currentTarget = target;
        isMoving = true;
        StartCoroutine(AnimateMovementCoroutine());
    }

    public void AnimateDeath(GameObject target)
    {
        if (isDying)
            return;

        isDying = true;
        currentTarget = target;
        StartCoroutine(AnimateDeathCoroutine());
    }

    private IEnumerator AnimateDeathCoroutine()
    {
        var sprite = currentTarget.GetComponent<SpriteRenderer>();
        if (sprite == null)
            yield break;

        while (sprite.color.a > 0)
        {
            var color = sprite.color;
            color.a = Math.Max(color.a - deathSpeed, 0);
            sprite.color = color;
            yield return null;
        }

        isDying = false;
        OnDeadEvent?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator AnimateMovementCoroutine()
    {
        // Дельта для избежания ошибок с плавающей точкой
        while (Vector3.Distance(currentTarget.transform.position, targetPosition.Value) > 0.01f)
        {
            currentTarget.transform.position = Vector3.MoveTowards(currentTarget.transform.position, targetPosition.Value, speed * Time.deltaTime);
            yield return null;
        }

        currentTarget.transform.position = targetPosition.Value;

        yield return null;

        isMoving = false;
        targetPosition = null;
        OnMovementCompletedEvent?.Invoke();
    }
}
