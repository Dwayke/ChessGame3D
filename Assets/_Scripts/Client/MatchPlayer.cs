using FishNet.Object;
using UnityEngine;

public class MatchPlayer : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        AddPlayer();
    }
    private void AddPlayer()
    {
        Managers.Instance.ClientManager.AddPlayer(this);
    }
}
