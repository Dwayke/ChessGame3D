using FishNet.Object;
using FishNet;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using FishNet.Managing.Scened;
using System.Collections;

public class ChessGameManager : NetworkBehaviour
{
    #region VARS
    #region UI
    [Header("UI")]
    [SerializeField] GameObject _victoryScreen;
    [SerializeField] TMP_Text _victoryText;    
    [SerializeField] GameObject _clientDisconnectScreen;
    [SerializeField] TMP_Text _clientDisconnectText;
    #endregion
    #region LOGIC
    public bool isGameStarted = false;
    public bool isLocalGame = true;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    #endregion
    #endregion
    #region ENGINE
    public override void OnStartServer()
    {
        base.OnStartServer();
        ChessManagers.Instance.GameUI.menuAnimator.SetTrigger("StartMenu");
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        ChessManagers.Instance.ClientManager.CmdRemoveAllPlayers();
    }
    #endregion
    #region MEMBER METHODS
    public void StartGame()
    {
        ChessManagers.Instance.BoardManager.InstaniateChessBoard();
        isGameStarted = true;
        RpcStartGame();
    }
    [ServerRpc(RequireOwnership = false)]
    public void CmdStartGame()
    {
        isGameStarted = true;
        ChessManagers.Instance.BoardManager.InstaniateChessBoard();
        RpcStartGame();
    }
    [ServerRpc(RequireOwnership = false)]
    public void CmdCheckMate(ETeam winner)
    {
        Debug.Log("checkmate");
        isGameStarted = false;
        RpcCheckMate(winner);
        DisplayVictory(winner);
    }

    public void DisplayVictory(ETeam winner)
    {
        _victoryScreen.SetActive(true);
        _victoryText.text = winner.ToString() + " Team Won!";
    }
    [ServerRpc(RequireOwnership =false)]
    public void CmdOnClientDisconnect(ETeam winner)
    {
        isGameStarted = false;
        DisplayOpponentDisconnect(winner, true);
        RpcOnClientDisconnect(winner);
        DisconnectServerAfterDelay(5);
        ReloadScene();
    }
    public void ReloadScene()
    {
        SceneLoadData sld = new("MainMenu")
        {
            ReplaceScenes = ReplaceOption.All
        };
        NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }
    public void OnResetButton()
    {
        //UI
        _victoryScreen.SetActive(false);
        _victoryText.text = "";
        //FieldReset
        ChessManagers.Instance.BoardManager.availableMoves.Clear();
        ChessManagers.Instance.BoardManager.moveList.Clear();
        //CleanUp
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (ChessManagers.Instance.BoardManager.
                chessPieces[x, y] != null)
                {
                    Destroy(ChessManagers.Instance.BoardManager.chessPieces[x, y].gameObject);
                }
                ChessManagers.Instance.BoardManager.chessPieces[x, y] = null;
            }
        }
        for (int i = 0; i < ChessManagers.Instance.BoardManager.deathParameters.deadWhites.Count; i++)
        {
            Destroy(ChessManagers.Instance.BoardManager.deathParameters.deadWhites[i].gameObject);
        }
        for (int i = 0; i < ChessManagers.Instance.BoardManager.deathParameters.deadBlacks.Count; i++)
        {
            Destroy(ChessManagers.Instance.BoardManager.deathParameters.deadBlacks[i].gameObject);
        }
        ChessManagers.Instance.BoardManager.deathParameters.deadWhites.Clear();
        ChessManagers.Instance.BoardManager.deathParameters.deadBlacks.Clear();

        StartGame();
    }
    public void OnExitButton()
    {
        Application.Quit();
    }
    #endregion
    #region LOCAL METHODS
    [ObserversRpc]
    private void RpcCheckMate(ETeam winner)
    {
        isGameStarted = false;
        Debug.Log("checkmate");
        DisplayVictory(winner);
    }
    [ObserversRpc]
    private void RpcStartGame()
    {
        isGameStarted = true;
        ChessManagers.Instance.GameUI.menuAnimator.SetTrigger("InGameMenu");
        ChessManagers.Instance.BoardManager.InstaniateChessBoard();
    }
    [ObserversRpc]
    private void RpcOnClientDisconnect(ETeam winner)
    {
        isGameStarted = false;
        DisplayOpponentDisconnect(winner, true);
        DisconnectServerAfterDelay(5);
    }
    private async void DisconnectServerAfterDelay(int seconds)
    {
        await UniTask.Delay(seconds * 1000);
        ChessManagers.Instance.ClientManager.CmdRemoveAllPlayers();
        ServerManager.StopConnection(false);
        DisplayOpponentDisconnect(ETeam.None, false);
    }
    private void DisplayOpponentDisconnect(ETeam winner, bool shouldDisplay)
    {
        _clientDisconnectScreen.SetActive(shouldDisplay);
        _clientDisconnectText.text = winner.ToString() + " Team Won!";
    }
    #endregion
}

