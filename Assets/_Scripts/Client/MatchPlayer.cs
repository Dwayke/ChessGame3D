using FishNet.Object;
using UnityEngine;

public class MatchPlayer : NetworkBehaviour
{
    public ETeam team;
    public override void OnStartClient()
    {
        base.OnStartClient();
        AddPlayer();
        AssignTeam();
    }
    private void AddPlayer()
    {
        Managers.Instance.ClientManager.AddPlayer(this);
    }
    private void AssignTeam()
    {
        Managers.Instance.ClientManager.AssignTeam(this);
    }
    private void RpcAssignTeam()
    {
        Managers.Instance.ClientManager.AssignTeam(this);
    }
}
