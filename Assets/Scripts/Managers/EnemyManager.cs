using Assets.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private AttackDatabaseSO db;
    [SerializeField]
    private Enemy[] enemyPrefabs;
    [SerializeField]
    private GameGrid grid;
    [SerializeField]
    private float speed;

    public void SpawnEnemies(int count)
    {

        for (int i = 0; i < count - 1; i++)
        {
            var enemyPosition = Vector3Int.zero;
            do
            {
                enemyPosition = new(Random.Range(0, grid.gridSize), Random.Range(0, grid.gridSize), 0);
            }
            while (enemyPosition == grid.playerPosition || enemyPosition == grid.shadowPosition || grid.enemyPositions.ContainsKey(enemyPosition));

            var enemyType = Random.Range(0, enemyPrefabs.Length);

            var enemy = Instantiate(enemyPrefabs[enemyType], grid.GetWorldPosition(enemyPrefabs[enemyType].gameObject, enemyPosition), Quaternion.identity);

            grid.enemyPositions.Add(enemyPosition, enemy);
        }
    }

    private IEnumerator MoveEnemiesCoroutine(Enemy enemy, List<Vector3Int> preferredPath)
    {
        foreach (var point in preferredPath)
        {
            bool movementCompleted = false;
            enemy.Animator.OnMovementCompletedEvent += () => { movementCompleted = true; };

            enemy.Animator.AddTargetPosition(grid.GetWorldPosition(enemy.gameObject, point));
            enemy.Animator.StartMovement(enemy.gameObject);

            yield return new WaitUntil(() => movementCompleted);

            grid.SetEnemyPosition(enemy, point);

            if (point == grid.playerPosition)
            {
                grid.GameOver();
            }

            yield return null;
        }
    }

    public List<List<Vector3Int>> GetAllLegalEnemyPaths(List<List<Vector3Int>> allPaths, List<Vector3Int> occupiedPaths)
    {
        var res = new List<List<Vector3Int>>();

        foreach (var path in allPaths)
        {
            var destination = path
                .Cast<Vector3Int?>()
                .LastOrDefault(q => grid.CheckBounds(q.Value) &&
                                    !grid.enemyPositions.ContainsKey(q.Value) &&
                                    !occupiedPaths.Contains(q.Value)
                );

            if (!destination.HasValue)
                continue;

            res.Add(path.GetRange(0, path.IndexOf(destination.Value) + 1));
        }

        return res;
    }

    public void MoveEnemies()
    {
        var enemyPositions = new Dictionary<Vector3Int, Enemy>(grid.enemyPositions);
        var prefferedPaths = new Dictionary<Enemy, List<Vector3Int>>();

        foreach (var (position, enemy) in enemyPositions)
        {
            var attackType = db.AttackTypes[enemy.AttackId];

            var allPaths = attackType.GetAllPaths(position);
            var legalPaths = GetAllLegalEnemyPaths(allPaths, prefferedPaths.Values.Select(q => q.Last()).ToList());

            var preferredPath = enemy.Behavior switch
            {
                EnemyBehaviors.MoveCloser => legalPaths.MinBy(q => (q.Last() - grid.playerPosition).magnitude),
                EnemyBehaviors.MoveAway => legalPaths.MaxBy(q => (q.Last() - grid.playerPosition).magnitude),
                _ => legalPaths[Random.Range(0, legalPaths.Count)]
            };

            prefferedPaths.Add(enemy, preferredPath);
        }

        foreach (var path in prefferedPaths)
        {
            StartCoroutine(MoveEnemiesCoroutine(path.Key, path.Value));
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }
}
