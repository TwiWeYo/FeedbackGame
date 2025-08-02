using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private GameObject cellIndicator;
    [SerializeField]
    private GameObject gridVizualization;
    [SerializeReference]
    private AttackPath attackPathPrefab;

    [SerializeField]
    public GameGrid gameGrid;

    [SerializeField]
    private AttackDatabaseSO db;
    private int selectedAttackType = -1;

    public bool IsPlayerTurn { get; private set; }
    private void Start()
    {
        StopAttacking();
        IsPlayerTurn = true;

        gameGrid.PlayerManager.OnPlayerMoved += FinishAttacking;

        gameGrid.OnAllEnemiesKilled += () => IsPlayerTurn = true;
    }

    public void StartAttacking(int id)
    {
        if (!IsPlayerTurn)
            return;

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

        gameGrid.PlayerManager.ClearIndicators();

        gridVizualization.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= Attack;
        inputManager.OnExit -= StopAttacking;
    }

    private void HighlightAttackDestinations()
    {
        gameGrid.PlayerManager.ClearIndicators();

        var attackType = db.AttackTypes[selectedAttackType];

        var allPaths = attackType.GetAllPaths(gameGrid.playerPosition);

        foreach (var destination in allPaths)
        {
            var attackPath = Instantiate(attackPathPrefab);
            attackPath.SetPath(destination);

            if (!gameGrid.PlayerManager.PlaceIndicator(attackPath))
            {
                attackPath.DestroySelfAndChildren(attackPath);
            }
        }
    }

    private void Attack()
    {
        if (!IsPlayerTurn)
            return;

        if (inputManager.IsMouseOverUI())
            return;

        var mousePosition = inputManager.GetMousePosition();
        gameGrid.PlayerManager.MovePlayer(mousePosition);
    }

    private void FinishAttacking(List<Vector3Int> path)
    {
        gameGrid.ShadowManager.ShadowMoveset.Add(path);

        IsPlayerTurn = false;
        StopAttacking();
        EnemiesTurn();
    }

    private void EnemiesTurn()
    {
        if (gameGrid.CheckIfAllEnemiesKilled())
            return;

        IsPlayerTurn = false;
        gameGrid.ShadowManager.MoveShadow();
        gameGrid.EnemyManager.MoveEnemies();
        IsPlayerTurn = true;
    }

    void Update()
    {
        var cellPosition = gameGrid.GetCellPosition(inputManager.GetMousePosition());
        cellIndicator.transform.position = gameGrid.GetWorldPosition(cellIndicator, cellPosition);
    }
}
