using Assets.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSelector : MonoBehaviour
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
    private GameGrid gameGrid;

    [SerializeField]
    private AttackDatabaseSO db;
    private int selectedAttackType = -1;

    private bool isPlayerTurn;
    public bool IsPlayerTurn
    {
        get => isPlayerTurn;
        set
        {
            Debug.Log(isPlayerTurn);
            isPlayerTurn = value;
        }
    }

    private void Start()
    {
        StopAttacking();
        IsPlayerTurn = true;
        InvokeRepeating("MoveEnemies", 5f, 5f);
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

        gameGrid.OnAllEnemiesKilled += () => IsPlayerTurn = true;
    }

    private void StopAttacking()
    {
        selectedAttackType = -1;

        gameGrid.ClearIndicators();

        gridVizualization.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= Attack;
        inputManager.OnExit -= StopAttacking;
    }

    private void HighlightAttackDestinations()
    {
        gameGrid.ClearIndicators();

        var attackType = db.AttackTypes[selectedAttackType];

        var allPaths = attackType.GetAllPaths(gameGrid.playerPosition);

        foreach (var destination in allPaths)
        {
            var attackPath = Instantiate(attackPathPrefab);
            attackPath.SetPath(destination);

            if (!gameGrid.PlaceIndicator(attackPath))
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

        IsPlayerTurn = false;
        
        if (!gameGrid.MovePlayer(mousePosition))
        {
            IsPlayerTurn = true;
            return;
        }

        StopAttacking();
    }

    private void MoveEnemies()
    {
        IsPlayerTurn = false;

        var enemyPositions = new Dictionary<Vector3Int, Enemy>(gameGrid.enemyPositions);

        gameGrid.MoveShadow();
        foreach (var (position, enemy) in enemyPositions)
        {
            var attackType = db.AttackTypes[enemy.AttackId];

            var allPaths = attackType.GetAllPaths(position);
            var legalPaths = gameGrid.GetAllLegalEnemyPaths(allPaths);

            var preferredPath = enemy.Behavior switch
            {
                EnemyBehaviors.MoveCloser => legalPaths.MinBy(q => (q.Last() - gameGrid.playerPosition).magnitude),
                EnemyBehaviors.MoveAway => legalPaths.MaxBy(q => (q.Last() - gameGrid.playerPosition).magnitude),
                _ => legalPaths[Random.Range(0, legalPaths.Count)]
            };

            gameGrid.MoveEnemy(enemy, preferredPath);
        }
        IsPlayerTurn = true;
    }

    void Update()
    {
        var cellPosition = gameGrid.GetCellPosition(inputManager.GetMousePosition());
        cellIndicator.transform.position = gameGrid.GetWorldPosition(cellIndicator, cellPosition);
    }
}
