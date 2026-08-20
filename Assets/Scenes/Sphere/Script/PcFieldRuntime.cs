using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Flags]
public enum PcFieldInputLockReason
{
    None = 0,
    Loading = 1,
    Event = 2,
    Dialogue = 4,
    Menu = 8,
    Battle = 16,
    NetworkRecovery = 32,
}

/// <summary>Scene世代とrequest idで古い非同期応答を拒否する。</summary>
public sealed class PcFieldRequestGate
{
    public int Generation { get; private set; }
    public int ActiveRequestId { get; private set; }
    private int nextRequestId;

    public void NewGeneration()
    {
        Generation++;
        ActiveRequestId = 0;
    }

    public int Begin()
    {
        ActiveRequestId = ++nextRequestId;
        return ActiveRequestId;
    }

    public bool IsCurrent(int generation, int requestId)
    {
        return generation == Generation && requestId != 0
            && requestId == ActiveRequestId;
    }

    public void Complete(int requestId)
    {
        if (requestId == ActiveRequestId) ActiveRequestId = 0;
    }
}

/// <summary>
/// Legacy Sphereの入力・COMNDループから分離したPC Field Runtime入口。
/// Stageは既存map/unit描画資産のホストとしてのみ使用する。
/// </summary>
public sealed class PcFieldRuntime : MonoBehaviour
{
    private const float MoveSpeedCellsPerSecond = 3.5f;
    private const float SyncIntervalSeconds = 0.50f;
    private const int MaxCellsPerSync = 16;

    [SerializeField] private bool verboseLogging;

    private SphereBehaviour sphere;
    private StageBehaviour stage;
    private PcFieldCollisionMap collision;
    private PcFieldPlayerController player;
    private PcFieldCameraController cameraController;
    private PcFieldEnemyController enemies;
    private PcFieldEventController events;
    private PcFieldHudController hud;
    private readonly Queue<Vector2Int> unsentPath = new Queue<Vector2Int>();
    private bool initialized;
    private bool networkInFlight;
    private bool prepareRequested;
    private bool encounterPending;
    private int pendingEnemyUnitNo;
    private int playerUnitNo;
    private Vector2Int serverConfirmedGrid;
    private Vector2Int inFlightTarget;
    private float lastSyncAt;
    private readonly PcFieldRequestGate requestGate = new PcFieldRequestGate();
    private bool destroyed;
    private bool waitingForPlayerView;
    private PcFieldInputLockReason inputLocks;

    public PcFieldInputLockReason InputLocks { get { return inputLocks; } }

    public void Initialize(SphereBehaviour sphereBehaviour, StageBehaviour stageBehaviour)
    {
        if (initialized) CleanupRuntime(true);
        requestGate.NewGeneration();
        destroyed = false;
        if (sphereBehaviour == null || stageBehaviour == null || sphereBehaviour.sphere == null)
        {
            Debug.LogError("[PCFIELD][INIT] Missing Sphere/Stage/map data; runtime stopped", this);
            enabled = false;
            return;
        }
        sphere = sphereBehaviour;
        enabled = true;
        stage = stageBehaviour;
        collision = new PcFieldCollisionMap(sphere, stage);
        cameraController = new PcFieldCameraController(sphere, stage.transform as RectTransform);
        events = new PcFieldEventController();
        RectTransform canvas = stage.transform.parent as RectTransform;
        if (canvas != null) hud = new PcFieldHudController(canvas);
        initialized = true;
        inputLocks = PcFieldInputLockReason.Loading;
        lastSyncAt = Time.realtimeSinceStartup;
        Debug.Log("[PCFIELD][INIT] generation=" + requestGate.Generation, this);
    }

    public void BeginFieldControl(int unitNo)
    {
        if (!initialized || !sphere.IsPcFreeMovement) return;
        playerUnitNo = unitNo;
        UnitBehaviour view;
        if (stage.objUnits == null
            || stage.objUnits.units == null
            || !stage.objUnits.units.TryGetValue("unit_" + unitNo, out view)
            || view == null)
        {
            waitingForPlayerView = true;
            SetLock(PcFieldInputLockReason.Loading, true);
            Debug.LogWarning("[PCFIELD][INIT] Waiting for player view unit=" + unitNo, this);
            return;
        }

        waitingForPlayerView = false;
        jsonUnit unit;
        if (sphere.sphere.unit == null || !sphere.sphere.unit.TryGetValue(unitNo, out unit)
            || unit == null || unit.X < 0)
        {
            Debug.LogError("[PCFIELD][INIT] Player unit data missing unit=" + unitNo, this);
            SetLock(PcFieldInputLockReason.Loading, true);
            return;
        }
        serverConfirmedGrid = new Vector2Int(unit.X, unit.Y);
        if (player == null)
        {
            player = new PcFieldPlayerController(sphere, collision, unitNo, view, verboseLogging);
            player.CellEntered += OnPlayerCellEntered;
            enemies = new PcFieldEnemyController(sphere, unitNo);
        }

        events.LeaveBarrier();
        SetLock(PcFieldInputLockReason.Loading, false);
        SetLock(PcFieldInputLockReason.Event | PcFieldInputLockReason.Battle
            | PcFieldInputLockReason.NetworkRecovery, false);
        encounterPending = false;
        if (!prepareRequested)
        {
            prepareRequested = true;
            SendPrepareRequest();
        }
        else
        {
            hud?.SetStatus("WASD: Move");
        }
    }

    private void Update()
    {
        if (!initialized || destroyed) return;
        if (waitingForPlayerView && playerUnitNo > 0)
        {
            BeginFieldControl(playerUnitNo);
            if (waitingForPlayerView) return;
        }
        if (player == null || events.BarrierActive || inputLocks != PcFieldInputLockReason.None)
            return;
        player.Tick(MoveSpeedCellsPerSecond);
        TrySendSync(false);
    }

    private void LateUpdate()
    {
        if (!initialized || player == null) return;
        cameraController.Tick(player.VisualGrid, 0.10f, 0.20f);
    }

    private void OnPlayerCellEntered(Vector2Int cell)
    {
        unsentPath.Enqueue(cell);
        int enemyUnitNo;
        if (enemies != null && enemies.TryGetEncounter(cell, out enemyUnitNo))
        {
            events.EnterBarrier();
            SetLock(PcFieldInputLockReason.Battle, true);
            encounterPending = true;
            pendingEnemyUnitNo = enemyUnitNo;
            hud?.SetStatus("Encounter...");
            TrySendSync(true);
            return;
        }
        TrySendSync(false);
    }

    private void TrySendSync(bool force)
    {
        if (networkInFlight) return;
        if (unsentPath.Count == 0)
        {
            if (encounterPending) SendEncounter();
            return;
        }
        if (!force && unsentPath.Count < MaxCellsPerSync
            && Time.realtimeSinceStartup - lastSyncAt < SyncIntervalSeconds)
            return;

        int count = Mathf.Min(MaxCellsPerSync, unsentPath.Count);
        StringBuilder path = new StringBuilder(count * 8);
        for (int i = 0; i < count; i++)
        {
            Vector2Int cell = unsentPath.Dequeue();
            if (i > 0) path.Append(':');
            path.Append(cell.x).Append(',').Append(cell.y);
            inFlightTarget = cell;
        }
        lastSyncAt = Time.realtimeSinceStartup;
        networkInFlight = true;
        int generation;
        int requestId;
        BeginRequest(out generation, out requestId);
        Dictionary<string, string> values = new Dictionary<string, string>();
        values["pcField"] = "1";
        values["pcFieldPath"] = path.ToString();
        APIConnectManager.Instance.SpherePcMove(sphere.Param.sphereId,
            sphere.sphere.validation_code, sphere.sphere.revision, values,
            json => { if (AcceptResponse(generation, requestId)) OnSyncResponse(json); });
    }

    private void OnSyncResponse(string json)
    {
        EndRequest();
        Vector2Int accepted;
        int revision;
        if (!TryParseFieldState(json, "PCFIELD_SYNC", out accepted, out revision))
        {
            RejectToServer("sync response invalid");
            return;
        }
        if (sphere == null || sphere.sphere == null || sphere.sphere.unit == null
            || !sphere.sphere.unit.ContainsKey(playerUnitNo))
        {
            Debug.LogError("[PCFIELD][SYNC] Player disappeared while sync was in flight", this);
            SetLock(PcFieldInputLockReason.NetworkRecovery, true);
            enabled = false;
            return;
        }
        sphere.sphere.revision = revision;
        serverConfirmedGrid = accepted;
        sphere.sphere.unit[playerUnitNo].X = accepted.x;
        sphere.sphere.unit[playerUnitNo].Y = accepted.y;
        if (accepted != inFlightTarget)
        {
            unsentPath.Clear();
            player.CorrectTo(accepted);
        }
        // Forward normal Sphere leads while consuming PC-field control leads locally.
        if (DispatchNormalLeads(json)) return;
        if (encounterPending) SendEncounter();
        else TrySendSync(true);
    }

    private void SendEncounter()
    {
        if (networkInFlight || pendingEnemyUnitNo <= 0) return;
        if (sphere == null || sphere.sphere == null || sphere.sphere.unit == null
            || !sphere.sphere.unit.ContainsKey(pendingEnemyUnitNo))
        {
            RejectToServer("encounter target disappeared");
            return;
        }
        if (verboseLogging)
            Debug.Log("[PCFIELD][ENCOUNTER] target=" + pendingEnemyUnitNo
                + " serverPlayer=" + serverConfirmedGrid, this);
        networkInFlight = true;
        int generation;
        int requestId;
        BeginRequest(out generation, out requestId);
        Dictionary<string, string> values = new Dictionary<string, string>();
        values["pcField"] = "1";
        values["pcFieldEncounter"] = pendingEnemyUnitNo.ToString();
        APIConnectManager.Instance.SpherePcMove(sphere.Param.sphereId,
            sphere.sphere.validation_code, sphere.sphere.revision, values,
            json => { if (AcceptResponse(generation, requestId)) OnBarrierResponse(json); });
    }

    private void SendPrepareRequest()
    {
        networkInFlight = true;
        events.EnterBarrier();
        SetLock(PcFieldInputLockReason.Loading, true);
        hud?.SetStatus("Preparing field...");
        Dictionary<string, string> values = new Dictionary<string, string>();
        values["pcField"] = "1";
        values["pcFieldPrepare"] = "1";
        int generation;
        int requestId;
        BeginRequest(out generation, out requestId);
        APIConnectManager.Instance.SpherePcMove(sphere.Param.sphereId,
            sphere.sphere.validation_code, sphere.sphere.revision, values,
            json => { if (AcceptResponse(generation, requestId)) OnPrepareResponse(json); });
    }

    private void OnPrepareResponse(string json)
    {
        EndRequest();
        if (!IsOk(json))
        {
            RejectToServer("prepare failed");
            return;
        }
        sphere.Mitter(json);
    }

    private void OnBarrierResponse(string json)
    {
        EndRequest();
        unsentPath.Clear();
        encounterPending = false;
        pendingEnemyUnitNo = 0;
        if (!IsOk(json))
        {
            RejectToServer("encounter failed");
            return;
        }
        sphere.Mitter(json);
    }

    private bool TryParseFieldState(string json, string prefix,
        out Vector2Int cell, out int revision)
    {
        cell = serverConfirmedGrid;
        revision = sphere.sphere.revision;
        try
        {
            JObject root = JObject.Parse(json);
            if ((string)root["result"] != "ok") return false;
            JObject leads = root["lead"] as JObject;
            if (leads == null) return false;
            bool found = false;
            foreach (JProperty property in leads.Properties())
            {
                string lead = (string)property.Value;
                if (lead.StartsWith(prefix + " "))
                {
                    string[] values = lead.Split(' ');
                    cell = new Vector2Int(int.Parse(values[1]), int.Parse(values[2]));
                    found = true;
                }
                else if (lead.StartsWith("REVIS "))
                    int.TryParse(lead.Substring(6), out revision);
            }
            return found;
        }
        catch { return false; }
    }

    private bool DispatchNormalLeads(string json)
    {
        try
        {
            JObject root = JObject.Parse(json);
            JObject leads = root["lead"] as JObject;
            if (leads == null) return false;

            JObject normalLeads = new JObject();
            int normalLeadCount = 0;
            foreach (JProperty property in leads.Properties())
            {
                string lead = (string)property.Value;
                if (string.IsNullOrEmpty(lead)) continue;

                string command = lead.Split(' ')[0];
                if (command == "PCFIELD_SYNC" || command == "PCFIELD_READY"
                    || command == "PCFIELD_ENCOUNTER" || command == "REVIS")
                    continue;

                normalLeadCount++;
                normalLeads["lead" + normalLeadCount] = lead;
            }

            if (normalLeadCount == 0) return false;

            JObject normalResponse = (JObject)root.DeepClone();
            normalResponse["lead"] = normalLeads;
            normalResponse["leadNum"] = normalLeadCount;

            events.EnterBarrier();
            SetLock(PcFieldInputLockReason.Event, true);
            sphere.Mitter(normalResponse.ToString(Newtonsoft.Json.Formatting.None));
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[PCFIELD][SYNC] Failed to dispatch normal leads: " + e.Message, this);
            return false;
        }
    }

    private bool IsOk(string json)
    {
        try { return (string)JObject.Parse(json)["result"] == "ok"; }
        catch { return false; }
    }

    private void RejectToServer(string reason)
    {
        Debug.LogError("[PCFIELD][ERROR] " + reason, this);
        networkInFlight = false;
        unsentPath.Clear();
        encounterPending = false;
        pendingEnemyUnitNo = 0;
        events.LeaveBarrier();
        SetLock(PcFieldInputLockReason.Loading | PcFieldInputLockReason.Event
            | PcFieldInputLockReason.Battle | PcFieldInputLockReason.NetworkRecovery, false);
        if (player != null) player.CorrectTo(serverConfirmedGrid);
        hud?.SetStatus("Sync error - position restored");
    }

    public void ShutdownForLegacy()
    {
        requestGate.NewGeneration();
        CleanupRuntime(true);
    }

    private void CleanupRuntime(bool resetCamera)
    {
        if (player != null) player.CellEntered -= OnPlayerCellEntered;
        player = null;
        unsentPath.Clear();
        networkInFlight = false;
        events?.LeaveBarrier();
        if (resetCamera) cameraController?.Reset();
        hud?.Dispose();
        hud = null;
        initialized = false;
        networkInFlight = false;
        waitingForPlayerView = false;
        prepareRequested = false;
        encounterPending = false;
        pendingEnemyUnitNo = 0;
        inputLocks = PcFieldInputLockReason.None;
    }

    private void OnDestroy()
    {
        destroyed = true;
        requestGate.NewGeneration();
        CleanupRuntime(false);
    }

    private void BeginRequest(out int generation, out int requestId)
    {
        generation = requestGate.Generation;
        requestId = requestGate.Begin();
        if (verboseLogging)
            Debug.Log("[PCFIELD][SYNC] send generation=" + generation + " request=" + requestId, this);
    }

    private bool AcceptResponse(int generation, int requestId)
    {
        if (destroyed || !initialized || !requestGate.IsCurrent(generation, requestId))
        {
            if (verboseLogging)
                Debug.LogWarning("[PCFIELD][SYNC] stale response ignored generation="
                    + generation + " request=" + requestId, this);
            return false;
        }
        return true;
    }

    private void EndRequest()
    {
        networkInFlight = false;
        requestGate.Complete(requestGate.ActiveRequestId);
    }

    public void SetMenuActive(bool active)
    {
        SetLock(PcFieldInputLockReason.Menu, active);
    }

    public void SetDialogueActive(bool active)
    {
        SetLock(PcFieldInputLockReason.Dialogue, active);
    }

    private void SetLock(PcFieldInputLockReason reason, bool active)
    {
        if (active) inputLocks |= reason;
        else inputLocks &= ~reason;
    }
}
