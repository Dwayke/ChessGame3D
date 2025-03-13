using FishNet.Object;
using UnityEngine;

public class MatchPlayer : NetworkBehaviour
{
    public ETeam team;
    public override void OnStartClient()
    {
        base.OnStartClient();
        Managers.Instance.GameUI.menuAnimator.SetTrigger("StartMenu");

        AddPlayer();
        AssignTeam();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        RemovePlayer();
        if (!Managers.Instance.GameManager.isLocalGame&&Managers.Instance.GameManager.isGameStarted)
        {
            AnnounceGameExit();
        }
    }
    private void AddPlayer()
    {
        Managers.Instance.ClientManager.AddPlayer(this);
    }
    private void RemovePlayer()
    {
        Managers.Instance.ClientManager.RemovePlayer(this);
    }
    private void AssignTeam()
    {
        Managers.Instance.ClientManager.AssignTeam(this);
    }
    private void AnnounceGameExit()
    {
        if (team == ETeam.White)
        {
            Managers.Instance.GameManager.OnClientDisconnect(ETeam.Black);
        }
        else 
        {
            Managers.Instance.GameManager.OnClientDisconnect(ETeam.White);
        }
    }
}
