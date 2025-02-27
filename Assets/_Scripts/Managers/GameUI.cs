using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    #region VARS
    //public ServerManager server;
    //public ClientManager client;

    [SerializeField] private Animator _menuAnimator;
    [SerializeField] TMP_InputField _addressInput;
    #endregion
    #region ENGINE

    #endregion
    #region MEMBER
    public void OnLocalGameButton()
    {
        //server.StartConnection();
        //client.StartConnection(_addressInput.text, 7777);
        _menuAnimator.SetTrigger("InGameMenu");
        Debug.Log("Start Local Game");
    }
    public void OnOnlineGameButton()
    {
        _menuAnimator.SetTrigger("OnlineMenu");
        Debug.Log("Go to Online Menu");
    }
    public void OnOnlineHostButton()
    {
        //server.StartConnection(7777);
        //client.StartConnection("127.0.0.1", 7777);
        _menuAnimator.SetTrigger("HostMenu");
        Debug.Log("Host Online Game");
    }
    public void OnOnlineConnectButton()
    {
        //client.StartConnection(_addressInput.text, 7777);
        Debug.Log("Connect To an Online Game");
    }
    public void OnOnlineBackButton()
    {
        _menuAnimator.SetTrigger("StartMenu");
        Debug.Log("Back to Main Menu");
    }    
    public void OnHostBackButton()
    {
        //server.StopConnection(true);
        //client.StopConnection();
        _menuAnimator.SetTrigger("OnlineMenu"); 
        Debug.Log("Back to Online Menu");
    }
    #endregion
}
