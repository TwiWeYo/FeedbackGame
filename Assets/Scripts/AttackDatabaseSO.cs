using System;
using System.Collections.Generic;
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
    public Vector2Int[] DestinationPoints { get; private set; }
    [field: SerializeField]
    public Vector2Int[] AttackedFields { get; private set; }
    [field: SerializeField]
    public bool IgnoreObstacles { get; private set; }
}
