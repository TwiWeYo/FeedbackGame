using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowManager : MonoBehaviour
{
    [SerializeField]
    private Shadow shadowPrefab;
    [SerializeField]
    private GameGrid grid;

    private Shadow shadow;
    private bool isMirrored;

    public List<List<Vector3Int>> ShadowMoveset { get; private set; } = new();
    private List<List<Vector3Int>> lastShadowMoveset = new();

    private int turnNumber;

    private Vector3Int GetUpperCorner() => new Vector3Int(grid.gridSize - 1, grid.gridSize - 1);

    private List<Vector3Int> GetShortestPathToZero(Vector3Int position)
    {
        var res = new List<Vector3Int>();
        var lastPosition = position;

        while (lastPosition != GetUpperCorner())
        {
            var x = lastPosition.x == grid.gridSize - 1 ? 0 : 1;
            var y = lastPosition.y == grid.gridSize - 1 ? 0 : 1;

            lastPosition = new Vector3Int(lastPosition.x + x, lastPosition.y + y);

            res.Add(lastPosition);
        }

        return res;
    }

    public bool IsDead => shadow == null;

    public void SpawnShadow()
    {
        if (!IsDead)
        {
            return;
        }

        lastShadowMoveset = ShadowMoveset;
        if (lastShadowMoveset.Any() && lastShadowMoveset.Last().Last() != GetUpperCorner())
        {
            lastShadowMoveset.Add(GetShortestPathToZero(lastShadowMoveset.Last().Last()));
        }

        ShadowMoveset = new();

        turnNumber = 0;
        isMirrored = true;

        shadow = Instantiate(shadowPrefab);
        var cellPosition = GetUpperCorner();

        shadow.transform.position = grid.GetWorldPosition(shadowPrefab.gameObject, cellPosition);
        grid.SetShadowPosition(cellPosition, true);
    }

    public void MoveShadow()
    {
        StartCoroutine(MoveShadowCoroutine());
    }

    public IEnumerator MoveShadowCoroutine()
    {
        if (shadow == null)
            yield break;

        if (lastShadowMoveset.Count == 0)
            yield break;

        foreach (var point in lastShadowMoveset[turnNumber])
        {
            var mirroredPoint = isMirrored 
                ? GetUpperCorner() - point
                : point;

            bool movementCompleted = false;
            shadow.Animator.OnMovementCompletedEvent += () => { movementCompleted = true; };

            shadow.Animator.AddTargetPosition(grid.GetWorldPosition(shadowPrefab.gameObject, mirroredPoint));
            shadow.Animator.StartMovement(shadow.gameObject);

            yield return new WaitUntil(() => movementCompleted);

            grid.SetShadowPosition(mirroredPoint);
        }

        turnNumber++;
        turnNumber %= lastShadowMoveset.Count;
        
        if (turnNumber == 0)
        {
            isMirrored = !isMirrored;
        }
    }

    public void RemoveShadow()
    {
        shadow.Animator.AnimateDeath(shadow.gameObject);

        shadow = null;
    }
}
