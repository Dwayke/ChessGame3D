using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class ChessUIManager : NetworkBehaviour
{
    #region VARS
    public Animator menuAnimator;
    [SerializeField] TMP_InputField _addressInput;
    #endregion
    #region ENGINE

    #endregion
    #region MEMBER
    public void OnLocalGameButton()
    {
        ChessManagers.Instance.GameManager.isLocalGame = true;
        menuAnimator.SetTrigger("InGameMenu");
        ChessManagers.Instance.GameManager.CmdStartGame();
        Debug.Log("Start Local Game");
    }
    public void OnOnlineGameButton()
    {
        ChessManagers.Instance.ClientManager.CmdReadyPlayersCounter(true);
        ChessManagers.Instance.GameManager.isLocalGame = false;
        menuAnimator.SetTrigger("HostMenu");
        Debug.Log("Go to Online Menu");
    }
    public void OnOnlineHostButton()
    {
        menuAnimator.SetTrigger("HostMenu");
        Debug.Log("Host Online Game");
    }
    public void OnOnlineConnectButton()
    {
        Debug.Log("Connect To an Online Game");
    }
    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
        Debug.Log("Back to Main Menu");
    }    
    public void OnHostBackButton()
    {
        ChessManagers.Instance.ClientManager.CmdReadyPlayersCounter(false);
        menuAnimator.SetTrigger("StartMenu"); 
        Debug.Log("Back to Online Menu");
    }
    #endregion
}
