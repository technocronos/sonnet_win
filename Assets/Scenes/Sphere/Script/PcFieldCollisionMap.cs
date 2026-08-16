using UnityEngine;

/// <summary>既存Stageのcost mapを、PC Fieldから読むための薄い境界。</summary>
public sealed class PcFieldCollisionMap
{
    private const int ImpassableCost = 9990;
    private readonly SphereBehaviour sphere;
    private readonly StageBehaviour stage;

    public PcFieldCollisionMap(SphereBehaviour sphere, StageBehaviour stage)
    {
        this.sphere = sphere;
        this.stage = stage;
    }

    public bool IsInside(Vector2Int cell)
    {
        return PcFieldMath.IsInside(cell, sphere.sphere.structWid, sphere.sphere.structHei);
    }

    public bool IsWalkable(Vector2Int cell, int playerUnitNo)
    {
        return GetBlockReason(cell, playerUnitNo) == null;
    }

    public string GetBlockReason(Vector2Int cell, int playerUnitNo)
    {
        if (!IsInside(cell)) return "OUT_OF_BOUNDS";
        int cost;
        if (stage == null || stage.cost == null
            || !stage.cost.TryGetValue("cost" + cell.x + "_" + cell.y, out cost)
            || cost >= ImpassableCost)
            return GetTerrainBlockReason(cell, GetCost(cell));

        int occupant = sphere.FindUnit(cell.x, cell.y);
        if (occupant != 0 && occupant != playerUnitNo)
            return IsEnemy(occupant, playerUnitNo)
                ? "ENEMY_BLOCKED unit=" + occupant : "UNIT_BLOCKED unit=" + occupant;
        return null;
    }

    public int GetCost(Vector2Int cell)
    {
        int cost;
        return stage != null && stage.cost != null
            && stage.cost.TryGetValue("cost" + cell.x + "_" + cell.y, out cost)
            ? cost : int.MaxValue;
    }

    private string GetTerrainBlockReason(Vector2Int cell, int finalCost)
    {
        int layerCost;
        if (TryGetLayerCost(sphere.sphere.structoverlayer2,
            sphere.sphere.overlayer2Wid, sphere.sphere.overlayer2Hei, cell, out layerCost)
            && layerCost >= ImpassableCost)
            return "OVERLAYER_BLOCKED layer=2 cost=" + layerCost + " final=" + finalCost;
        if (TryGetLayerCost(sphere.sphere.structoverlayer1,
            sphere.sphere.overlayer1Wid, sphere.sphere.overlayer1Hei, cell, out layerCost)
            && layerCost >= ImpassableCost)
            return "OVERLAYER_BLOCKED layer=1 cost=" + layerCost + " final=" + finalCost;
        return "TERRAIN_BLOCKED cost=" + finalCost;
    }

    private bool TryGetLayerCost(string[] rows, int width, int height,
        Vector2Int cell, out int layerCost)
    {
        layerCost = 0;
        if (rows == null || cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height
            || cell.y >= rows.Length || string.IsNullOrEmpty(rows[cell.y])
            || rows[cell.y].Length < (cell.x + 1) * 4)
            return false;
        int tipIndex;
        if (!int.TryParse(rows[cell.y].Substring(cell.x * 4, 4), out tipIndex)
            || sphere.sphere.tip == null || !sphere.sphere.tip.ContainsKey(tipIndex))
            return false;
        return int.TryParse(sphere.sphere.tip[tipIndex], out layerCost);
    }

    private bool IsEnemy(int occupant, int playerUnitNo)
    {
        if (!sphere.sphere.unit.ContainsKey(occupant)
            || !sphere.sphere.unit.ContainsKey(playerUnitNo)) return false;
        int a;
        int b;
        return TryGetUnion(sphere.sphere.unit[occupant], out a)
            && TryGetUnion(sphere.sphere.unit[playerUnitNo], out b) && a != b;
    }

    private bool TryGetUnion(jsonUnit unit, out int union)
    {
        union = 0;
        if (unit == null || string.IsNullOrEmpty(unit.Info)) return false;
        string[] values = unit.Info.Split(' ');
        return values.Length > 1 && int.TryParse(values[1], out union);
    }
}
