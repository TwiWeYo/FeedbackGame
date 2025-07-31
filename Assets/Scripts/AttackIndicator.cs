using System.Collections.Generic;
using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    private bool isInitialized;

    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private GameObject pathHighlight;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Grid grid;

    public List<Vector2Int> moveSteps;
    public Vector2Int position;

    private void Awake()
    {
        if (!isInitialized)
        {
            Initialize(moveSteps, position);
            isInitialized = true;
        }
    }

    private void Initialize(List<Vector2Int> moveSteps, Vector2Int position)
    {
        Debug.Log($"Initialized with position {position}");
    }


    private void OnMouseEnter()
    {
        if (!isInitialized)
            return;

        foreach (var moveStep in moveSteps)
        {
            Instantiate(pathHighlight, grid.CellToWorld(new Vector3Int(moveStep.x, moveStep.y, 0)), Quaternion.identity);
        }
    }

    private void OnMouseExit()
    {
        if (!isInitialized)
            return;
        
        var highlights = GameObject.FindGameObjectsWithTag("PathHighlight");

        foreach (var item in highlights)
        {
            Destroy(item);
        }
    }
}
