using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>PC Field固有のfloat移動。通信・Camera・旧COMNDを所有しない。</summary>
public sealed class PcFieldPlayerController
{
    private const float BoundaryInset = 0.001f;
    private const float MaxSweepStep = 0.20f;
    private readonly SphereBehaviour sphere;
    private readonly PcFieldCollisionMap collision;
    private readonly int unitNo;
    private UnitBehaviour view;
    private Vector2 visualGrid;
    private Vector2Int logicalGrid;
    private string lastReject;
    private float lastRejectAt;
    private readonly bool verboseLogging;

    public event Action<Vector2Int> CellEntered;
    public Vector2 VisualGrid { get { return visualGrid; } }
    public Vector2Int LogicalGrid { get { return logicalGrid; } }

    public PcFieldPlayerController(SphereBehaviour sphere, PcFieldCollisionMap collision,
        int unitNo, UnitBehaviour view, bool verboseLogging = false)
    {
        this.sphere = sphere;
        this.collision = collision;
        this.unitNo = unitNo;
        this.view = view;
        this.verboseLogging = verboseLogging;
        jsonUnit unit = sphere.sphere.unit[unitNo];
        visualGrid = new Vector2(unit.X, unit.Y);
        logicalGrid = new Vector2Int(unit.X, unit.Y);
        ApplyView();
    }

    public void Tick(float speedCellsPerSecond)
    {
        Vector2 input = ReadInput();
        if (input.sqrMagnitude <= 0f) return;
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector2 delta = new Vector2(input.x, -input.y)
            * Mathf.Max(0f, speedCellsPerSecond) * Time.unscaledDeltaTime;
        int steps = PcFieldMath.SweepSteps(delta, MaxSweepStep);
        Vector2 step = delta / steps;
        for (int i = 0; i < steps; i++) MoveStep(step);
        ApplyView();
    }

    public void CorrectTo(Vector2Int authoritative)
    {
        visualGrid = authoritative;
        logicalGrid = authoritative;
        ApplyView();
    }

    private void MoveStep(Vector2 delta)
    {
        Vector2Int from = logicalGrid;
        Vector2 candidate = visualGrid + delta;
        candidate.x = Mathf.Clamp(candidate.x, 0f, sphere.sphere.structWid - 1f);
        candidate.y = Mathf.Clamp(candidate.y, 0f, sphere.sphere.structHei - 1f);
        Vector2Int target = PcFieldMath.Quantize(candidate);
        if (PcFieldMath.IsDiagonalTransition(from, target))
        {
            string xReason = collision.GetBlockReason(new Vector2Int(target.x, from.y), unitNo);
            string yReason = collision.GetBlockReason(new Vector2Int(from.x, target.y), unitNo);
            if (xReason != null || yReason != null)
            {
                LogReject("DIAGONAL_CORNER x=" + (xReason ?? "pass")
                    + " y=" + (yReason ?? "pass"), target);
                MoveAxis(delta.x, true);
                MoveAxis(delta.y, false);
                return;
            }
        }
        MoveAxis(delta.x, true);
        MoveAxis(delta.y, false);
    }

    private void MoveAxis(float amount, bool horizontal)
    {
        if (Mathf.Approximately(amount, 0f)) return;
        Vector2 candidate = visualGrid;
        if (horizontal) candidate.x += amount;
        else candidate.y += amount;
        candidate.x = Mathf.Clamp(candidate.x, 0f, sphere.sphere.structWid - 1f);
        candidate.y = Mathf.Clamp(candidate.y, 0f, sphere.sphere.structHei - 1f);

        Vector2Int candidateCell = PcFieldMath.Quantize(candidate);
        string reason = candidateCell == logicalGrid
            ? null : collision.GetBlockReason(candidateCell, unitNo);
        if (reason != null)
        {
            float half = 0.5f - BoundaryInset;
            if (horizontal)
                candidate.x = Mathf.Clamp(candidate.x, logicalGrid.x - half, logicalGrid.x + half);
            else
                candidate.y = Mathf.Clamp(candidate.y, logicalGrid.y - half, logicalGrid.y + half);
            LogReject(reason, candidateCell);
        }

        visualGrid = candidate;
        Vector2Int next = PcFieldMath.Quantize(visualGrid);
        if (next != logicalGrid)
        {
            logicalGrid = next;
            CellEntered?.Invoke(next);
        }
    }

    private Vector2 ReadInput()
    {
        float x = 0f;
        float y = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;
        if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f))
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return new Vector2(x, y);
    }

    private void LogReject(string reason, Vector2Int cell)
    {
        if (!verboseLogging) return;
        float now = Time.realtimeSinceStartup;
        string signature = cell + ":" + reason;
        if (signature == lastReject && now - lastRejectAt < 0.5f) return;
        lastReject = signature;
        lastRejectAt = now;
        Debug.LogWarning("[PCFIELD][COLLISION] cell=" + cell + " reason=" + reason);
    }

    private void ApplyView()
    {
        if (view == null) return;
        float margin = (sphere.TIP_SIZE - sphere.UNIT_SIZE) / 2f;
        view.transform.localPosition = new Vector3(
            visualGrid.x * sphere.TIP_SIZE + margin,
            -(visualGrid.y * sphere.TIP_SIZE + margin), 0f);
    }
}
