using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    public List<MatchPlayer> players;
    public int readyPlayersCount = 0;
    [ServerRpc(RequireOwnership =false)]
    public void AddPlayer(MatchPlayer player)
    {
        players.Add(player);
        //CheckPlayerCount();
    }
    [ServerRpc(RequireOwnership = false)]
    public void AssignTeam(MatchPlayer player)
    {
        if (players.Count % 2 != 0)
        {
            player.team = ETeam.White;
            RpcAssignTeam(player, ETeam.White);
        }
        else { player.team = ETeam.Black; RpcAssignTeam(player,ETeam.Black); }
    }
    [ObserversRpc(ExcludeOwner = true)]
    private void RpcAssignTeam(MatchPlayer player,ETeam team)
    {
        player.team = team;
    }
    [ServerRpc(RequireOwnership = false)]
    public void ReadyPlayersCounter(bool shouldIncrement)
    {
        if (shouldIncrement)
        {
            readyPlayersCount += 1;
        }
        else
        {
            readyPlayersCount -=1;
        }
        Debug.Log(readyPlayersCount);
        CheckPlayerCount();
    }
    private void CheckPlayerCount()
    {
        if (players.Count == readyPlayersCount)
        {
            Managers.Instance.GameManager.StartGame();
            Managers.Instance.TurnManager.StartTimer();
        }
    }
}
