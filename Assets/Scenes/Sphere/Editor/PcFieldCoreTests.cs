using NUnit.Framework;
using UnityEngine;

public class PcFieldCoreTests
{
    [Test]
    public void Quantize_UsesStableHalfCellBoundary()
    {
        Assert.AreEqual(new Vector2Int(2, 3), PcFieldMath.Quantize(new Vector2(1.51f, 2.51f)));
        Assert.AreEqual(new Vector2Int(1, 2), PcFieldMath.Quantize(new Vector2(1.49f, 2.49f)));
    }

    [Test]
    public void Sweep_PreventsLargeDeltaFromBecomingSingleStep()
    {
        Assert.GreaterOrEqual(PcFieldMath.SweepSteps(new Vector2(1.2f, 0f), 0.2f), 6);
    }

    [TestCase(-1, 0, false)]
    [TestCase(0, -1, false)]
    [TestCase(0, 0, true)]
    [TestCase(9, 9, true)]
    [TestCase(10, 9, false)]
    public void Bounds_AreExclusiveAtWidthAndHeight(int x, int y, bool expected)
    {
        Assert.AreEqual(expected, PcFieldMath.IsInside(new Vector2Int(x, y), 10, 10));
    }

    [Test]
    public void DiagonalTransition_IsDistinguishedFromAxisMove()
    {
        Assert.IsTrue(PcFieldMath.IsDiagonalTransition(Vector2Int.zero, Vector2Int.one));
        Assert.IsFalse(PcFieldMath.IsDiagonalTransition(Vector2Int.zero, Vector2Int.right));
    }

    [Test]
    public void RequestGate_RejectsOldSceneAndOldRequest()
    {
        PcFieldRequestGate gate = new PcFieldRequestGate();
        gate.NewGeneration();
        int generation = gate.Generation;
        int first = gate.Begin();
        int second = gate.Begin();
        Assert.IsFalse(gate.IsCurrent(generation, first));
        Assert.IsTrue(gate.IsCurrent(generation, second));
        gate.NewGeneration();
        Assert.IsFalse(gate.IsCurrent(generation, second));
    }

    [Test]
    public void RequestGate_CompletionPreventsDuplicateCallback()
    {
        PcFieldRequestGate gate = new PcFieldRequestGate();
        gate.NewGeneration();
        int request = gate.Begin();
        Assert.IsTrue(gate.IsCurrent(gate.Generation, request));
        gate.Complete(request);
        Assert.IsFalse(gate.IsCurrent(gate.Generation, request));
    }

    [Test]
    public void CameraClamp_DoesNotExposeWorldOutsideContentBounds()
    {
        float allowed = PcFieldMath.ClampCameraAxis(100f, -50f, 50f,
            -200f, 200f, 0f, -40f, 40f);
        Assert.LessOrEqual(allowed, 40f);
    }
}
