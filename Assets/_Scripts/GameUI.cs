using UnityEngine;

public class GameUI : MonoBehaviour
{
    #region VARS
    public static GameUI Instance;

    [SerializeField] private Animator _menuAnimator;
    #endregion
    #region ENGINE
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    #region MEMBER
    public void OnLocalGameButton()
    {
        Debug.Log("Start Local Game");
    }
    public void OnOnlineGameButton()
    {
        Debug.Log("Go to Online Menu");
    }
    public void OnOnlineHostButton()
    {
        Debug.Log("Host Online Game");
    }
    public void OnOnlineConnectButton()
    {
        Debug.Log("Connect To an Online Game");
    }
    public void OnOnlineBackButton()
    {
        Debug.Log("Back to Main Menu");
    }    
    public void OnHostBackButton()
    {
        Debug.Log("Back to Online Menu");
    }
    #endregion
}
