using FishNet.Object;
using UnityEngine;

public class MatchPlayer : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        ServerRpcCheckPlayerCount();
    }
    [ServerRpc]
    private void ServerRpcCheckPlayerCount()
    {
        Managers.Instance.GameManager.clientParameters.players.Add(this);
        if (Managers.Instance.GameManager.clientParameters.players.Count==2)
        {
            Managers.Instance.GameManager.StartGame();

            ObserversRpcCheckPlayerCount();
        }
    }    
    [ObserversRpc]
    private void ObserversRpcCheckPlayerCount()
    {
    }
}
