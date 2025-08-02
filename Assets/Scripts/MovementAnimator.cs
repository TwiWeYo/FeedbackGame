using System;
using System.Collections;
using UnityEngine;

public class MovementAnimator : MonoBehaviour
{
    [SerializeField]
    public float speed = 10f;

    private Vector3? targetPosition;
    
    private GameObject currentTarget;
    private bool isMoving = false;
    
    public event Action OnMovementCompletedEvent;

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
