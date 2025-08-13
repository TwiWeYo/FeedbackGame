using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MovementAnimator : MonoBehaviour
{
    [SerializeField]
    public float speed = 10f;
    [SerializeField]
    public float deathSpeed = 0.2f;

    public IEnumerator MoveCoroutine(GameObject target, Vector3? position)
    {
        if (target == null)
        {
            yield break;
        }

        if (!position.HasValue)
        {
            yield break;
        }

        // Дельта для избежания ошибок с плавающей точкой
        while (Vector3.Distance(target.transform.position, position.Value) > 0.01f)
        {
            target.transform.position = Vector3.MoveTowards(target.transform.position, position.Value, speed * Time.deltaTime);
            yield return null;
        }

        target.transform.position = position.Value;
    }

    public IEnumerator AnimateDeathCoroutine(GameObject target)
    {
        var sprite = target.GetComponent<SpriteRenderer>();
        if (sprite == null)
            yield break;

        while (sprite.color.a > 0)
        {
            var color = sprite.color;
            color.a = Math.Max(color.a - deathSpeed, 0);
            sprite.color = color;
            yield return null;
        }
    }
}
