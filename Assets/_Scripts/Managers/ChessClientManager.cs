using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class ChessClientManager : NetworkBehaviour
{
    #region VARS
    public List<ChessMatchPlayer> players;
    public int readyPlayersCount = 0;
    #endregion
    #region MEMBER METHODS
    public void AddPlayer(ChessMatchPlayer player)
    {
        players.Add(player);
    }   
    public void RemovePlayer(ChessMatchPlayer player)
    {
        readyPlayersCount -= 1;
        players.Remove(player);
    }    
    [ServerRpc(RequireOwnership =false)]
    public void CmdRemoveAllPlayers()
    {
        foreach (ChessMatchPlayer player in players) 
        {
            readyPlayersCount -= 1;
            player.ClientManager.StopConnection();
            players.Remove(player);
        }
    }
    public void AssignTeam(ChessMatchPlayer player)
    {
        if (players.Count % 2 != 0)
        {
            player.team = ETeam.White;
            RpcAssignTeam(player, ETeam.White);
        }
        else { player.team = ETeam.Black; RpcAssignTeam(player,ETeam.Black); }
    }
    [ServerRpc(RequireOwnership = false)]
    public void CmdReadyPlayersCounter(bool shouldIncrement)
    {
        if (shouldIncrement)
        {
            readyPlayersCount += 1;
        }
        else
        {
            readyPlayersCount -= 1;
        }
        Debug.Log(readyPlayersCount);
        CheckPlayerCount();
    }
    #endregion
    #region LOCAL METHODS
    [ObserversRpc(ExcludeOwner = true)]
    private void RpcAssignTeam(ChessMatchPlayer player,ETeam team)
    {
        player.team = team;
    }

    private void CheckPlayerCount()
    {
        if (players.Count == readyPlayersCount)
        {
            ChessManagers.Instance.GameManager.StartGame();
            ChessManagers.Instance.TurnManager.StartTimer();
        }
    }
    #endregion
}
