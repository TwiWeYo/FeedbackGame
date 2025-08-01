using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private Vector2 lastPosition;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private LayerMask layerMask;

    public event Action OnClicked, OnExit;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnClicked?.Invoke();
        if (Input.GetKeyDown(KeyCode.Escape))
            OnExit?.Invoke();
    }

    public bool IsMouseOverUI() => EventSystem.current.IsPointerOverGameObject();

    public Vector2 GetMousePosition()
    {
        if (mainCamera == null)
            return lastPosition;

        var mousePosition = Input.mousePosition;

        var ray = mainCamera.ScreenPointToRay(mousePosition);

        var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);
        if (hit.collider != null)
        {
            lastPosition = hit.point;
        }

        return lastPosition;
    }
}
