using FishNet.Object;
using System.Collections;
using UnityEngine;

public class ChessMatchPlayer : NetworkBehaviour
{
    #region VARS
    public ETeam team;
    #endregion
    #region ENGINE
    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(DelayedSetupMatchPlayer());
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        CmdRemovePlayer();
        //ChessManagers.Instance.GameUI.menuAnimator.SetTrigger("StartMenu");
        if (!ChessManagers.Instance.GameManager.isLocalGame&&ChessManagers.Instance.GameManager.isGameStarted)
        {
            AnnounceGameExit();
        }
    }
    #endregion
    #region LOCAL METHODS
    private IEnumerator DelayedSetupMatchPlayer() 
    {
        while (!IsClientInitialized) 
        {
            yield return null; 
        }
        ChessManagers.Instance.GameUI.menuAnimator.SetTrigger("StartMenu");
        CmdAddPlayer();
        CmdAssignTeam();
    }
    [ServerRpc(RequireOwnership = true)]
    private void CmdAddPlayer()
    {
        ChessManagers.Instance.ClientManager.AddPlayer(this);
    }
    [ServerRpc(RequireOwnership = true)]
    private void CmdRemovePlayer()
    {
        ChessManagers.Instance.ClientManager.RemovePlayer(this);
    }
    [ServerRpc(RequireOwnership = false)]
    private void CmdAssignTeam()
    {
        ChessManagers.Instance.ClientManager.AssignTeam(this);
    }
    private void AnnounceGameExit()
    {
        if (team == ETeam.White)
        {
            ChessManagers.Instance.GameManager.CmdOnClientDisconnect(ETeam.Black);
        }
        else if(team == ETeam.Black)
        {
            ChessManagers.Instance.GameManager.CmdOnClientDisconnect(ETeam.White);
        }
        else { Debug.Log("nullTeam"); }
    }
    #endregion
}
