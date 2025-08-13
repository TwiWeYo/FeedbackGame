using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameGrid grid;
    [SerializeField]
    private Player playerPrefab;
    

    private Player player;

    private Dictionary<Vector3Int, AttackPath> attackPaths = new();

    public void SpawnPlayer()
    {
        if (player == null)
        {
            player = Instantiate(playerPrefab);
        }

        grid.SetPlayerPosition(Vector3Int.zero);

        player.transform.position = grid.GetWorldPosition(player.gameObject, grid.playerPosition);
    }

    public event Action<List<Vector3Int>> OnPlayerMoved;

    public void MovePlayer(Vector3 position)
    {
        StartCoroutine(MovePlayerCoroutine(position));
    }

    private IEnumerator MovePlayerCoroutine(Vector3 position)
    {
        var cellPosition = grid.GetCellPosition(position);

        if (!attackPaths.ContainsKey(cellPosition))
        {
            yield break;
        }

        var moves = new List<Vector3Int>();

        foreach (var point in attackPaths[cellPosition].Path)
        {
            Vector3 worldPosition = grid.GetWorldPosition(player.Animator.gameObject, point);

            var coroutine = player.Animator.MoveCoroutine(player.Animator.gameObject, worldPosition);

            yield return StartCoroutine(coroutine);

            grid.SetPlayerPosition(point);
            moves.Add(grid.playerPosition);

            if (point == cellPosition)
            {
                break;
            }
        }

        OnPlayerMoved?.Invoke(moves);
    }

    public IEnumerator RemovePlayerCoroutine()
    {
        var coroutine = player.Animator.AnimateDeathCoroutine(player.gameObject);
        yield return StartCoroutine(coroutine);

        grid.GameOver();
    }

    public bool PlaceIndicator(AttackPath attackPath)
    {
        var maxAvailablePosition = attackPath.Path.Cast<Vector3Int?>().LastOrDefault(q => grid.CheckBounds(q.Value));

        if (!maxAvailablePosition.HasValue)
            return false;

        if (attackPaths.ContainsKey(maxAvailablePosition.Value))
            return false;

        attackPaths.Add(maxAvailablePosition.Value, attackPath);
        attackPath.transform.position = grid.GetWorldPosition(attackPath.attackTargetIndicator, maxAvailablePosition.Value);
        DrawAttackPath(attackPath);

        return true;
    }

    public void ClearIndicators()
    {
        foreach (var item in attackPaths)
        {
            item.Value.DestroySelfAndChildren(item.Value);
        }

        attackPaths.Clear();
    }

    public void DrawAttackPath(AttackPath attackPath)
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
                var worldPosition = grid.GetWorldPosition(attackPath.attackGhostIndicator, point);
                var obj = Instantiate(attackPath.attackGhostIndicator, worldPosition, Quaternion.identity);

                attackPath.AddPointToPath(obj);
                continue;
            }

            else
            {
                var worldPosition = grid.GetWorldPosition(attackPath.attackPathIndicator, point);
                var obj = Instantiate(attackPath.attackPathIndicator, worldPosition, Quaternion.identity);

                attackPath.AddPointToPath(obj);
                continue;
            }
        }
    }
}
