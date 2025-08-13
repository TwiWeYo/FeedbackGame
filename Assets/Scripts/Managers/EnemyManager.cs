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
    [SerializeField]
    private GameObject attackHighlight;

    public void SpawnEnemies(int count)
    {
        var enemies = GetEnemies();
        foreach (var enemyType in enemies)
        {
            var enemyPosition = Vector3Int.zero;
            do
            {
                enemyPosition = new(Random.Range(0, grid.gridSize), Random.Range(0, grid.gridSize), 0);
            }
            while (enemyPosition == grid.playerPosition || enemyPosition == grid.shadowPosition || grid.enemyPositions.ContainsKey(enemyPosition));

            var enemy = Instantiate(enemyType, grid.GetWorldPosition(enemyType.gameObject, enemyPosition), Quaternion.identity);

            grid.enemyPositions.Add(enemyPosition, enemy);
        }
    }

    private List<Enemy> GetEnemies()
    {
        return grid.WaveNumber switch
        {
            2 => new() { enemyPrefabs[0] },
            3 => new() { enemyPrefabs[1] },
            4 => new() { enemyPrefabs[0], enemyPrefabs[0] },
            5 => new() { enemyPrefabs[2] },
            6 => new() { enemyPrefabs[1], enemyPrefabs[1], enemyPrefabs[0] },
            7 => new() { enemyPrefabs[3] },
            8 => new() { enemyPrefabs[3], enemyPrefabs[0] },
            9 => new() { enemyPrefabs[3], enemyPrefabs[0], enemyPrefabs[1] },
            10 => new() { enemyPrefabs[4] },
            11 => new() { enemyPrefabs[3], enemyPrefabs[1], enemyPrefabs[1] },
            12 => new() { enemyPrefabs[2], enemyPrefabs[2] },
            13 => new() { enemyPrefabs[5] },
            14 => new() { enemyPrefabs[3], enemyPrefabs[2] },
            15 => new() { enemyPrefabs[3], enemyPrefabs[3] },
            16 => new() { enemyPrefabs[1], enemyPrefabs[1], enemyPrefabs[2] },
            17 => new() { enemyPrefabs[6] },
            18 => new() { enemyPrefabs[7] },
            19 => Enumerable.Repeat(enemyPrefabs[0], 4).ToList(),
            20 => Enumerable.Repeat(enemyPrefabs[1], 4).ToList(),
            21 => Enumerable.Repeat(enemyPrefabs[2], 3).ToList(),
            22 => Enumerable.Repeat(enemyPrefabs[6], 2).ToList(),
            23 => Enumerable.Repeat(enemyPrefabs[8], 1).ToList(),
            24 => Enumerable.Repeat(enemyPrefabs[3], 3).ToList(),
            25 => new() { enemyPrefabs[3], enemyPrefabs[3], enemyPrefabs[5] },
            26 => new() { enemyPrefabs[3], enemyPrefabs[3], enemyPrefabs[6] },
            27 => Enumerable.Repeat(enemyPrefabs[5], 3).ToList(),
            28 => new() { enemyPrefabs[7], enemyPrefabs[8] },
            29 => Enumerable.Repeat(enemyPrefabs[4], 4).ToList(),
            30 => new() { enemyPrefabs[3], enemyPrefabs[6] },
            31 => Enumerable.Repeat(enemyPrefabs[6], 3).ToList(),
            32 => new() { enemyPrefabs[9] },
            _ => new()
        };
    }

    private IEnumerator MoveEnemiesCoroutine(Enemy enemy, List<Vector3Int> preferredPath)
    {
        var highlight = Instantiate(attackHighlight);
        foreach (var point in preferredPath)
        {
            if (highlight != null)
            {
                highlight.transform.position = grid.GetWorldPosition(highlight, point);
            }

            var coroutine = enemy.Animator.MoveCoroutine(enemy.gameObject, grid.GetWorldPosition(enemy.gameObject, point));
            yield return StartCoroutine(coroutine);

            grid.SetEnemyPosition(enemy, point, point != preferredPath.Last());
        }
        Destroy(highlight);
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
            var legalPaths = GetAllLegalEnemyPaths(allPaths, prefferedPaths.Values.Select(q => q.Last()).Append(grid.shadowPosition).ToList());

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

    public void RemoveEnemyCoroutine(Enemy enemy)
    {
        var coroutine = enemy.Animator.AnimateDeathCoroutine(enemy.gameObject);
        StartCoroutine(coroutine);
    }
}
