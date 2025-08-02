using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public int WaveNumber { get; private set; }

    [SerializeField]
    public int gridSize;
    [SerializeField]
    private Grid grid;

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
    
    private void Start()
    {
        StartLevel();
        OnAllEnemiesKilled += StartLevel;
    }

    private void StartLevel()
    {
        shadowManager.SpawnShadow();
        playerManager.SpawnPlayer();
        enemyManager.SpawnEnemies(++WaveNumber);
    }

    public bool CheckIfAllEnemiesKilled()
    {
        if (enemyPositions.Count == 0 && shadowManager.IsDead())
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
            enemyManager.RemoveEnemy(enemyPositions[position]);
            enemyPositions.Remove(position);

            return;
        }
        if (position == shadowPosition)
        {
            shadowManager.RemoveShadow();
            return;
        }
    }

    public void SetEnemyPosition(Enemy enemy, Vector3Int position)
    {
        var currentPosition = enemyPositions.First(q => q.Value == enemy).Key;
        enemyPositions.Remove(currentPosition);

        if (position == playerPosition)
        {
            GameOver();
        }

        if (enemyPositions.ContainsKey(position))
        {
            enemyPositions.Add(currentPosition, enemy);
            Debug.LogWarning("Не нашлось свободного места для enemy");
            return;
        }

        enemyPositions.Add(position, enemy);
    }

    public void SetShadowPosition(Vector3Int position)
    {
        shadowPosition = position;
        
        if (position == playerPosition)
        {
            GameOver();
        }
    }

    public void GameOver()
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

    public bool CheckBounds(Vector3Int cellPosition)
    {
        return cellPosition.x >= 0 && cellPosition.x <= gridSize - 1 &&
               cellPosition.y >= 0 && cellPosition.y <= gridSize - 1;
    }
}
