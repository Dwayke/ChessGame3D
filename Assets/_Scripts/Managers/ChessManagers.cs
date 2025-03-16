using UnityEngine;

public class ChessManagers : Singleton<ChessManagers>
{
    #region VAR
    public ChessGameManager GameManager;
    public ChessboardManager BoardManager;
    public ChessTurnManager TurnManager;
    public ChessClientManager ClientManager;
    public ChessUIManager GameUI;
    #endregion
    #region ENGINE
    private void Start()
    {
        if(!GameManager)GameManager = GetComponentInChildren<ChessGameManager>(true);
        if(!BoardManager)BoardManager = GetComponentInChildren<ChessboardManager>(true);
        if(!TurnManager)TurnManager = GetComponentInChildren<ChessTurnManager>(true);
        if(!ClientManager)ClientManager = GetComponentInChildren<ChessClientManager>(true);
        if(!GameUI) GameUI = GetComponentInChildren<ChessUIManager>(true);
    }
    #endregion
}
