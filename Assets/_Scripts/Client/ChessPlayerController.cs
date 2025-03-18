using FishNet.Component.Spawning;
using FishNet.Object;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChessPlayerController : NetworkBehaviour
{
    #region VARS
    private ChessMatchPlayer _player;
    private ChessControls _chessControls;
    public Camera currentCamera;
    private Vector2Int _currentHover;
    private ChessPiece _currentlyDragging;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    #endregion
    #region ENGINE
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!currentCamera)
        {
            currentCamera = Camera.main;
        }
        _chessControls = new ChessControls();
        _player = GetComponent<ChessMatchPlayer>();
        _chessControls.Gameplay.Click.performed += OnClick;
        _chessControls.Gameplay.Enable();
        if(IsOwner)StartCoroutine(DelayPositionCamera());
    }
    private IEnumerator DelayPositionCamera()
    {
        if (!ChessManagers.Instance.GameManager.isGameStarted)
        {
            yield return null;
        }
        PositionCamera();
    }
    private void PositionCamera()
    {
        currentCamera.transform.SetParent(transform, false);

        if (_player.team == ETeam.White)
        {
            //currentCamera.transform.SetParent(transform,false);
            //GameObject whiteCamera = FindAnyObjectByType<WhitePlayerCamera>().gameObject;
            //currentCamera.transform.position = whiteCamera.transform.position;
            //currentCamera.transform.rotation = whiteCamera.transform.rotation;
        }
        else
        {
            //GameObject blackCamera = FindAnyObjectByType<BlackPlayerCamera>().gameObject;
            //currentCamera.transform.position = blackCamera.transform.position;
            //currentCamera.transform.rotation = blackCamera.transform.rotation;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        _chessControls.Gameplay.Click.performed -= OnClick;
        _chessControls.Gameplay.Disable();
    }
    void Update()
    {
        if (!IsOwner) return;

        CheckHoverStatus();
    }
    #endregion
    #region LOCAL METHODS
    private void CheckHoverStatus()
    {
        if ((ChessManagers.Instance.TurnManager.currentTurn != _player.team && 
            !ChessManagers.Instance.GameManager.isLocalGame) ||
            !ChessManagers.Instance.GameManager.isGameStarted) return;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            Vector2Int hitPosition = ChessManagers.Instance.BoardManager.LookupTileIndex(info.transform.gameObject);
            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                ChessManagers.Instance.BoardManager.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if (_currentHover != hitPosition)
            {
                ChessManagers.Instance.BoardManager.tiles[_currentHover.x, _currentHover.y].layer = (ChessManagers.Instance.BoardManager.ContainsValidMove(ref ChessManagers.Instance.BoardManager.availableMoves, _currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                _currentHover = hitPosition;
                ChessManagers.Instance.BoardManager.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                ChessManagers.Instance.BoardManager.tiles[_currentHover.x, _currentHover.y].layer
                    = (ChessManagers.Instance.BoardManager.ContainsValidMove(ref ChessManagers.Instance.BoardManager.availableMoves, _currentHover))
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                _currentHover = -Vector2Int.one;
            }
        }
    }
    private void OnClick(InputAction.CallbackContext obj)
    {
        if ((ChessManagers.Instance.TurnManager.currentTurn != _player.team && !ChessManagers.Instance.GameManager.isLocalGame) || !ChessManagers.Instance.GameManager.isGameStarted) return;
        if (!currentCamera) return;
        if (_currentHover != -Vector2Int.one)
        {
            if (_currentlyDragging == null)
            {
                if (ChessManagers.Instance.BoardManager.chessPieces[_currentHover.x, _currentHover.y] != null && 
                    (ChessManagers.Instance.BoardManager.chessPieces[_currentHover.x, _currentHover.y].team == ETeam.White && 
                    ChessManagers.Instance.TurnManager.currentTurn == ETeam.White) || 
                    (ChessManagers.Instance.BoardManager.chessPieces[_currentHover.x, _currentHover.y].team == ETeam.Black && 
                    ChessManagers.Instance.TurnManager.currentTurn == ETeam.Black))
                {
                    _currentlyDragging = ChessManagers.Instance.BoardManager.chessPieces[_currentHover.x, _currentHover.y];
                    ChessManagers.Instance.BoardManager.CheckMoves(_currentlyDragging);
                    PreventCheck();
                    HighlightTiles();
                }
            }
            else
            {
                Vector2Int previousPosition = new(_currentlyDragging.currentX, _currentlyDragging.currentY);
                bool validMove = ChessManagers.Instance.BoardManager.MoveTo(_currentlyDragging, _currentHover.x, _currentHover.y);
                if (!validMove)
                {
                    _currentlyDragging.SetPosition(ChessManagers.Instance.BoardManager.GetTileCenter(previousPosition.x, previousPosition.y));
                }
                else
                {
                    _currentlyDragging.SetPosition(ChessManagers.Instance.BoardManager.GetTileCenter(_currentHover.x, _currentHover.y));
                }
                _currentlyDragging = null;
                RemoveHighlightTiles();
            }

        }
    }
    private void HighlightTiles()
    {
        for (int i = 0; i < ChessManagers.Instance.BoardManager.availableMoves.Count; i++)
        {
            ChessManagers.Instance.BoardManager.tiles[ChessManagers.Instance.BoardManager.availableMoves[i].x, ChessManagers.Instance.BoardManager.availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < ChessManagers.Instance.BoardManager.availableMoves.Count; i++)
        {
            ChessManagers.Instance.BoardManager.tiles[ChessManagers.Instance.BoardManager.availableMoves[i].x, ChessManagers.Instance.BoardManager.availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        ChessManagers.Instance.BoardManager.availableMoves.Clear();
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (ChessManagers.Instance.BoardManager.chessPieces[x, y] != null)
                {
                    if (ChessManagers.Instance.BoardManager.chessPieces[x, y].piece == EPiece.King && ChessManagers.Instance.BoardManager.chessPieces[x, y].team == _currentlyDragging.team)
                    {
                        targetKing = ChessManagers.Instance.BoardManager.chessPieces[x, y];
                    }
                }
            }
        }
        ChessManagers.Instance.BoardManager.SimulateForSinglePiece(_currentlyDragging, ref ChessManagers.Instance.BoardManager.availableMoves, targetKing);
    }
    #endregion
}
