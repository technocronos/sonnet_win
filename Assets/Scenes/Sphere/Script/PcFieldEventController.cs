using UnityEngine;

/// <summary>
/// PC Fieldイベント境界。PoCではServerのprepare/encounter Barrierを担当し、
/// treasure/goal/chainのローカル定義解釈は次段階で追加する。
/// </summary>
public sealed class PcFieldEventController
{
    public bool BarrierActive { get; private set; }

    public void EnterBarrier() { BarrierActive = true; }
    public void LeaveBarrier() { BarrierActive = false; }
}
