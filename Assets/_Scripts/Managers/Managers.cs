using UnityEngine;

public class Managers : Singleton<Managers>
{
    #region VAR
    public GameManager GameManager;
    public ClientManager ClientManager;
    public GameUI GameUI;
    #endregion
    #region ENGINE
    private void Start()
    {
        if(!GameManager)GameManager = GetComponentInChildren<GameManager>(true);
        if(!ClientManager)ClientManager = GetComponentInChildren<ClientManager>(true);
        if(!GameUI) GameUI = GetComponentInChildren<GameUI>(true);
    }
    #endregion
}
