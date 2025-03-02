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
    private void CheckPlayerCount()
    {
        if (players.Count == 2)
        {
            Managers.Instance.GameManager.StartGame();
        }
    }
}
