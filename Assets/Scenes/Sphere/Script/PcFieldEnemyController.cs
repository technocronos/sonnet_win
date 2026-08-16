using System.Collections.Generic;
using UnityEngine;

/// <summary>PC Fieldの敵実体一覧と接触候補を管理する。AIはPoCでは停止。</summary>
public sealed class PcFieldEnemyController
{
    private readonly SphereBehaviour sphere;
    private readonly int playerUnitNo;
    private int playerUnion;

    public PcFieldEnemyController(SphereBehaviour sphere, int playerUnitNo)
    {
        this.sphere = sphere;
        this.playerUnitNo = playerUnitNo;
        TryGetUnion(sphere.sphere.unit[playerUnitNo], out playerUnion);
    }

    public bool TryGetEncounter(Vector2Int playerCell, out int enemyUnitNo)
    {
        enemyUnitNo = 0;
        foreach (KeyValuePair<int, jsonUnit> entry in sphere.sphere.unit)
        {
            if (entry.Key == playerUnitNo || entry.Value == null || entry.Value.X < 0) continue;
            int union;
            if (!TryGetUnion(entry.Value, out union) || union == playerUnion) continue;
            if (Mathf.Abs(playerCell.x - entry.Value.X)
                + Mathf.Abs(playerCell.y - entry.Value.Y) <= 1)
            {
                enemyUnitNo = entry.Key;
                return true;
            }
        }
        return false;
    }

    private bool TryGetUnion(jsonUnit unit, out int union)
    {
        union = 0;
        if (unit == null || string.IsNullOrEmpty(unit.Info)) return false;
        string[] values = unit.Info.Split(' ');
        return values.Length > 1 && int.TryParse(values[1], out union);
    }
}
