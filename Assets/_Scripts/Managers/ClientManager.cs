using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    public List<MatchPlayer> players;

    [ServerRpc(RequireOwnership =false)]
    public void AddPlayer(MatchPlayer player)
    {
        players.Add(player);
        CheckPlayerCount();
    }
    [ServerRpc(RequireOwnership = false)]
    public void AssignTeam(MatchPlayer player)
    {
        if (players.Count % 2 == 0)
        {
            player.team = ETeam.Black;
            RpcAssignTeam(player, ETeam.Black);
        }
        else { player.team = ETeam.White; RpcAssignTeam(player,ETeam.White); }
    }
    [ObserversRpc(ExcludeOwner = true)]
    private void RpcAssignTeam(MatchPlayer player,ETeam team)
    {
        player.team = team;
    }
    private void CheckPlayerCount()
    {
        if (players.Count == 2)
        {
            Managers.Instance.GameManager.StartGame();
            Managers.Instance.TurnManager.StartTimer();
        }
    }
}
