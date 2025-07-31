using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private GameObject cellIndicator;
    [SerializeField]
    private GameObject attackCellIndicator;
    [SerializeField]
    private GameObject gridVizualization;


    [SerializeField]
    private Grid grid;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private AttackDatabaseSO db;
    private int selectedAttackType = -1;

    private void Start()
    {
        StopAttacking();
    }

    public void StartAttacking(int id)
    {
        selectedAttackType = db.AttackTypes.FindIndex(q => q.Id == id);
        if (selectedAttackType < 0)
        {
            Debug.LogError($"{id} not found");
            return;
        }

        gridVizualization.SetActive(true);
        cellIndicator.SetActive(true);

        HighlightAttackDestinations();

        inputManager.OnClicked += Attack;
        inputManager.OnExit += StopAttacking;
    }

    private void StopAttacking()
    {
        selectedAttackType = -1;
        ClearAttackIndicators();

        gridVizualization.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= Attack;
        inputManager.OnExit -= StopAttacking;
    }

    private void HighlightAttackDestinations()
    {
        ClearAttackIndicators();
        var playerPosition3D = grid.WorldToCell(player.transform.position);

        var destinations = db.AttackTypes[selectedAttackType].DestinationPoints;

        var allPossibleAttackDestinations = destinations
            .Select(q => new[] { playerPosition3D + new Vector3Int(q.x, q.y, playerPosition3D.z), playerPosition3D - new Vector3Int(q.x, q.y, playerPosition3D.z) })
            .SelectMany(q => q)
            .ToArray();

        foreach (var destination in allPossibleAttackDestinations)
        {
            var indicator = Instantiate(attackCellIndicator);

            var spriteRenderer = indicator.GetComponent<SpriteRenderer>();
            var spriteBounds = spriteRenderer.bounds;
            var pivotOffset = spriteRenderer.transform.position - spriteBounds.min;

            indicator.transform.position = grid.CellToWorld(destination) + pivotOffset;
        }
    }

    private void Attack()
    {
        if (inputManager.IsMouseOverUI())
            return;

        player.transform.position = GetCellPosition(player);
        StopAttacking();
    }

    private void ClearAttackIndicators()
    {
        var indicators = GameObject.FindGameObjectsWithTag("AttackIndicator");
        foreach (var indicator in indicators)
        {
            GameObject.Destroy(indicator);
        }
    }

    void Update()
    {
        cellIndicator.transform.position = GetCellPosition(cellIndicator);
    }

    private Vector3 GetCellPosition(GameObject gameObject)
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = grid.WorldToCell(mousePosition);

        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        var spriteBounds = spriteRenderer.bounds;
        var pivotOffset = spriteRenderer.transform.position - spriteBounds.min;

        return grid.CellToWorld(gridPosition) + pivotOffset;
    }
}
