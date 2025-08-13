using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameGrid : MonoBehaviour
{
    public int WaveNumber { get; private set; }

    [SerializeField]
    public int gridSize;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private TileBase[] tiles;

    [SerializeField]
    private PlayerManager playerManager;
    public PlayerManager PlayerManager => playerManager;
    [SerializeField]
    private ShadowManager shadowManager;
    public ShadowManager ShadowManager => shadowManager;
    [SerializeField]
    private EnemyManager enemyManager;
    public EnemyManager EnemyManager => enemyManager;

    public Vector3Int shadowPosition { get; private set; }
    public Vector3Int playerPosition { get; private set; }

    public Dictionary<Vector3Int, Enemy> enemyPositions = new();

    public event Action OnAllEnemiesKilled;
    public event Action OnGameOver;
    
    private void Start()
    {
    }

    public void StartLevel()
    {
        shadowManager.SpawnShadow();
        playerManager.SpawnPlayer();
        enemyManager.SpawnEnemies(++WaveNumber);

        GenerateTiles();
    }

    private void GenerateTiles()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var randomTile = tiles[UnityEngine.Random.Range(0, tiles.Length)];
                var cellPosition = new Vector3Int(x, y, 0);
                tilemap.SetTile(cellPosition, randomTile);
            }
        }
    }

    public bool CheckIfAllEnemiesKilled()
    {
        if (enemyPositions.Count == 0 && shadowManager.IsDead)
        {
            OnAllEnemiesKilled?.Invoke();
            return true;
        }

        return false;
    }

    public void SetPlayerPosition(Vector3Int position)
    {
        playerPosition = position;

        if (enemyPositions.ContainsKey(position))
        {
            enemyManager.RemoveEnemyCoroutine(enemyPositions[position]);
            enemyPositions.Remove(position);

            return;
        }
        if (position == shadowPosition)
        {
            shadowManager.RemoveShadow();
            return;
        }
    }

    public void SetEnemyPosition(Enemy enemy, Vector3Int position, bool isBypass = true)
    {
        var currentPosition = enemyPositions.First(q => q.Value == enemy).Key;
        enemyPositions.Remove(currentPosition);

        if (enemyPositions.ContainsKey(position))
        {
            enemyPositions.Add(currentPosition, enemy);
            Debug.LogWarning("Не нашлось свободного места для enemy");
            return;
        }

        if (position == playerPosition)
        {
            playerManager.RemovePlayerCoroutine();
        }

        if (position == shadowPosition && !isBypass)
        {
            enemyManager.RemoveEnemyCoroutine(enemyPositions[position]);
        }

        enemyPositions.Add(position, enemy);
    }

    public void SetShadowPosition(Vector3Int position, bool bypassGameOver = false)
    {
        shadowPosition = position;
        
        if (!bypassGameOver && position == playerPosition)
        {
            playerManager.RemovePlayerCoroutine();
        }

        if (enemyPositions.ContainsKey(position))
        {
            enemyManager.RemoveEnemyCoroutine(enemyPositions[position]);
            enemyPositions.Remove(position);
        }
    }

    public void GameOver()
    {
        OnGameOver?.Invoke();
    }

    public Vector3 GetWorldPosition(GameObject gameObject, Vector3Int cellPosition)
    {
        Vector3 cellWorldPosition = grid.CellToWorld(cellPosition);

        Vector3 cellSize = grid.cellSize;

        Vector3 centerOffset = new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);

        return cellWorldPosition + centerOffset;
    }

    public Vector3Int GetCellPosition(Vector3 position) => grid.WorldToCell(position);

    public bool CheckBounds(Vector3Int cellPosition)
    {
        return cellPosition.x >= 0 && cellPosition.x <= gridSize - 1 &&
               cellPosition.y >= 0 && cellPosition.y <= gridSize - 1;
    }
}
