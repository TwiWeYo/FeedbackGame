using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameGrid : MonoBehaviour
{
    private int waveNumber = 1;
    private int turnNumber = 0;

    [SerializeField]
    private int gridSize;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject shadowPrefab;

    private GameObject player;
    private GameObject shadow;

    [SerializeField]
    private Enemy[] enemyPrefabs;
    private Vector3Int shadowPosition;

    private List<List<Vector3Int>> shadowMoveset = new();
    private List<List<Vector3Int>> lastShadowMoveset = new();
    public Vector3Int playerPosition { get; private set; }

    public Dictionary<Vector3Int, AttackPath> attackPaths = new();
    public Dictionary<Vector3Int, Enemy> enemyPositions = new();

    private void Start()
    {
        player = Instantiate(playerPrefab);
        playerPosition = Vector3Int.zero;

        player.transform.position = GetWorldPosition(playerPrefab, playerPosition);
        SpawnEnemies(waveNumber);
    }

    public event Action OnAllEnemiesKilled;

    public bool PlaceIndicator(AttackPath attackPath)
    {
        var maxAvailablePosition = attackPath.Path.Cast<Vector3Int?>().LastOrDefault(q => CheckBounds(q.Value));

        if (!maxAvailablePosition.HasValue)
            return false;

        if (attackPaths.ContainsKey(maxAvailablePosition.Value))
            return false;

        attackPaths.Add(maxAvailablePosition.Value, attackPath);
        attackPath.transform.position = GetWorldPosition(attackPath.attackTargetIndicator, maxAvailablePosition.Value);
        DrawPath(attackPath);

        return true;
    }

    public void DrawPath(AttackPath attackPath)
    {
        bool hasPassedMaxPath = false;

        foreach (var point in attackPath.Path)
        {
            if (attackPaths.ContainsKey(point))
            {
                hasPassedMaxPath = true;
                continue;
            }

            if (hasPassedMaxPath)
            {
                var worldPosition = GetWorldPosition(attackPath.attackGhostIndicator, point);
                var obj = Instantiate(attackPath.attackGhostIndicator, worldPosition, Quaternion.identity);

                attackPath.AddPointToPath(obj);
                continue;
            }

            else
            {
                var worldPosition = GetWorldPosition(attackPath.attackPathIndicator, point);
                var obj = Instantiate(attackPath.attackPathIndicator, worldPosition, Quaternion.identity);

                attackPath.AddPointToPath(obj);
                continue;
            }
        }
    }

    public void ClearIndicators()
    {
        foreach (var item in attackPaths)
        {
            item.Value.DestroySelfAndChildren(item.Value);
        }

        attackPaths.Clear();
    }

    public bool MovePlayer(Vector3 position)
    {
        var cellPosition = GetCellPosition(position);

        if (!attackPaths.ContainsKey(cellPosition))
            return false;

        var moves = new List<Vector3Int>();

        foreach (var point in attackPaths[cellPosition].Path)
        {
            player.transform.position = GetWorldPosition(playerPrefab, point);
            playerPosition = cellPosition;
            moves.Add(playerPosition);

            if (enemyPositions.ContainsKey(point))
            {
                RemoveEnemy(enemyPositions[point]);
            }
            else if (point == shadowPosition)
            {
                RemoveShadow();
            }

            if (point == cellPosition)
            {
                break;
            }
        }

        shadowMoveset.Add(moves);
        return true;
    }

    public List<List<Vector3Int>> GetAllLegalEnemyPaths(List<List<Vector3Int>> allPaths)
    {
        var res = new List<List<Vector3Int>>();

        foreach (var path in allPaths)
        {
            var destination = path.Cast<Vector3Int?>().LastOrDefault(q => CheckBounds(q.Value) && !enemyPositions.ContainsKey(q.Value));
            if (!destination.HasValue)
                continue;

            res.Add(path.GetRange(0, path.IndexOf(destination.Value) + 1));
        }

        return res;
    }

    public void SpawnEnemies(int count)
    {
        playerPosition = Vector3Int.zero;
        player.transform.position = GetWorldPosition(playerPrefab, playerPosition);

        SpawnShadow();

        for (int i = 0; i < count - 1; i++)
        {
            var enemyPosition = Vector3Int.zero;
            do
            {
                enemyPosition = new(UnityEngine.Random.Range(0, gridSize), UnityEngine.Random.Range(0, gridSize), 0);
            }
            while (enemyPosition == playerPosition || enemyPosition == shadowPosition || enemyPositions.ContainsKey(enemyPosition));

            var enemyType = UnityEngine.Random.Range(0, 10) > 6 ? 1 : 0;

            var enemy = Instantiate(enemyPrefabs[enemyType], GetWorldPosition(enemyPrefabs[enemyType].gameObject, enemyPosition), Quaternion.identity);

            enemyPositions.Add(enemyPosition, enemy);
        }
    }

    public void MoveEnemy(Enemy enemy, List<Vector3Int> path)
    {
        RemoveEnemy(enemy, false);

        foreach (var point in path)
        {
            enemy.transform.position = GetWorldPosition(enemy.gameObject, point);
            if (point == playerPosition)
            {
                GameOver();
            }
        }

        enemyPositions.Add(path.Last(), enemy);
    }

    public void RemoveEnemy(Enemy enemy, bool isDestroy = true)
    {
        var key = enemyPositions.First(q => q.Value == enemy).Key;
        enemyPositions.Remove(key);
        if (isDestroy)
        {
            Destroy(enemy.gameObject);
            if (enemyPositions.Count == 0 && shadow == null)
            {
                OnAllEnemiesKilled?.Invoke();
                SpawnEnemies(++waveNumber);
            }
        }
    }

    private void SpawnShadow()
    {
        lastShadowMoveset = shadowMoveset;
        shadowMoveset = new();


        shadow = Instantiate(shadowPrefab);
        var cellPosition = new Vector3Int(gridSize - 1, gridSize - 1, 0);

        if (!lastShadowMoveset.Any())
        {
            cellPosition = new Vector3Int(UnityEngine.Random.Range(1, gridSize - 1), UnityEngine.Random.Range(1, gridSize - 1), 0);
        }

        shadow.transform.position = GetWorldPosition(shadowPrefab, cellPosition);
        shadowPosition = cellPosition;
    }

    public void MoveShadow()
    {
        if (shadow == null)
            return;

        if (lastShadowMoveset.Count == 0)
            return;

        foreach (var point in lastShadowMoveset[turnNumber % shadowMoveset.Count])
        {
            var mirroredPoint = new Vector3Int(gridSize - 1, gridSize - 1, 0) - point;
            shadow.transform.position = GetWorldPosition(shadowPrefab, mirroredPoint);
            shadowPosition = mirroredPoint;

            if (enemyPositions.ContainsKey(mirroredPoint))
            {
                //RemoveEnemy(enemyPositions[mirroredPoint]);
            }

            else if (shadowPosition == playerPosition)
            {
                GameOver();
            }
        }

        turnNumber++;
    }

    public void RemoveShadow()
    {
        Destroy(shadow);
        shadow = null;

        if (enemyPositions.Count == 0)
        {
            OnAllEnemiesKilled?.Invoke();
            SpawnEnemies(++waveNumber);
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
    }

    public Vector3 GetWorldPosition(GameObject gameObject, Vector3Int cellPosition)
    {
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not found on {gameObject.name}!");
            return Vector3.zero; // or some other default
        }

        var spriteBounds = spriteRenderer.bounds;
        var pivotOffset = gameObject.transform.position - spriteBounds.min;

        return grid.CellToWorld(cellPosition) + pivotOffset;
    }

    public Vector3Int GetCellPosition(Vector3 position) => grid.WorldToCell(position);

    private bool CheckBounds(Vector3Int cellPosition)
    {
        return cellPosition.x >= 0 && cellPosition.x <= gridSize - 1 &&
               cellPosition.y >= 0 && cellPosition.y <= gridSize - 1;
    }
}
