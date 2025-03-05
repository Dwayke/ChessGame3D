using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public ETeam currentTurn = ETeam.White;
    public ETurnTime turnTime = ETurnTime.ThirtySec;
    private readonly SyncTimer _timeRemaining = new();
    bool _isWhiteTurn = true;
    float timer;
    private void OnEnable()
    {
        _isWhiteTurn = true;
        timer = TurnTimeToSeconds(turnTime);
        _timeRemaining.OnChange += TimeRemaining_OnChange;
    }
    private void OnDisable()
    {
        _timeRemaining.OnChange -= TimeRemaining_OnChange;
    }
    private void TimeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer)
    {
        if (op == SyncTimerOperation.Finished) { CmdSwitchTurn(); }
    }
    private void FixedUpdate()
    {
        _timeRemaining.Update();
        if (_timeRemaining.Remaining <= 0&& Managers.Instance.ClientManager.players.Count == 2)
        {
            StartTimer();
        }
    }
    public void CmdSwitchTurn()
    {
        Debug.Log("switch turn");
        _isWhiteTurn = !_isWhiteTurn;
        if (_isWhiteTurn)
        {
            currentTurn = ETeam.White;
            RpcSwitchTurn(ETeam.White); 
        }
        else
        {
            currentTurn = ETeam.Black; 
            RpcSwitchTurn(ETeam.Black); 
        }
    }
    [ObserversRpc]
    private void RpcSwitchTurn(ETeam team)
    {
        currentTurn = team;
    }
    private float TurnTimeToSeconds(ETurnTime tt)
    {
        return tt switch
        {
            ETurnTime.ThirtySec => 30f,
            ETurnTime.SixtySec => 60f,
            ETurnTime.NinetySec => 90f,
            ETurnTime.FiveMinutes => 300f,
            ETurnTime.None => 99999f,
            _ => 0f
        };
    }
    public void StartTimer()
    {
        _timeRemaining.StartTimer(timer);
    }
    public void ResetTimer()
    {
        _timeRemaining.StopTimer();
        _timeRemaining.StartTimer(timer);
    }
}
public enum ETurnTime
{
    None = 0,
    ThirtySec = 1,
    SixtySec=2,
    NinetySec=3,
    FiveMinutes = 4
}
