using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackDatabaseSO : ScriptableObject
{
    public List<AttackType> AttackTypes;
}

[Serializable]
public class AttackType
{
    [field: SerializeField]
    public int Id { get; private set; }
    [field: SerializeField]
    public string[] Paths { get; private set; }
    [field: SerializeField]
    public Vector2Int[] AttackedFields { get; private set; }
    [field: SerializeField]
    public MoveBehaviors MoveBehavior { get; private set; }

    private static Dictionary<char, Vector2Int> directions = new()
    {
        ['0'] = Vector2Int.up,
        ['1'] = new(1, 1),
        ['2'] = Vector2Int.right,
        ['3'] = new(1, -1),
        ['4'] = Vector2Int.down,
        ['5'] = new(-1, -1),
        ['6'] = Vector2Int.left,
        ['7'] = new(-1, 1)
    };

    public List<List<Vector2Int>> GetAllPaths()
    {
        var res = new List<List<Vector2Int>>();
        foreach (var path in Paths)
        {
            var innerList = new List<Vector2Int>();
            var resultVector = Vector2Int.zero;
            foreach (var c in path)
            {
                resultVector += directions[c];
                innerList.Add(resultVector);
            }
        }

        return res;
    }
}

public enum MoveBehaviors
{
    Move,
    IgnoreObstacles,
    Teleport
}
