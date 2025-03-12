using FishNet.Object;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    private MatchPlayer _player;
    private ChessControls _chessControls;
    private Camera _currentCamera;
    private Vector2Int _currentHover;
    private ChessPiece _currentlyDragging;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!_currentCamera)
        {
            _currentCamera = Camera.main;
            //_currentCamera = _player.GetComponent<Camera>();
        }
        _chessControls = new ChessControls();
        _player = GetComponent<MatchPlayer>();
        _chessControls.Gameplay.Click.performed += OnClick;
        _chessControls.Gameplay.Enable();
        _currentCamera.transform.SetParent(transform, false);
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
    private void CheckHoverStatus()
    {
        if ((Managers.Instance.TurnManager.currentTurn != _player.team && !Managers.Instance.GameManager.isLocalGame) || !Managers.Instance.GameManager.isGameStarted) return;
        Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            Vector2Int hitPosition = Managers.Instance.GameManager.LookupTileIndex(info.transform.gameObject);
            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                Managers.Instance.GameManager.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if (_currentHover != hitPosition)
            {
                Managers.Instance.GameManager.tiles[_currentHover.x, _currentHover.y].layer = (Managers.Instance.GameManager.ContainsValidMove(ref Managers.Instance.GameManager._availableMoves, _currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                _currentHover = hitPosition;
                Managers.Instance.GameManager.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                Managers.Instance.GameManager.tiles[_currentHover.x, _currentHover.y].layer
                    = (Managers.Instance.GameManager.ContainsValidMove(ref Managers.Instance.GameManager._availableMoves, _currentHover))
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                _currentHover = -Vector2Int.one;
            }
        }
    }
    private void OnClick(InputAction.CallbackContext obj)
    {
        if ((Managers.Instance.TurnManager.currentTurn != _player.team&&!Managers.Instance.GameManager.isLocalGame) || !Managers.Instance.GameManager.isGameStarted) return;
        if (!_currentCamera) return;
        if (_currentHover != -Vector2Int.one)
        {
            if (_currentlyDragging == null)
            {
                if (Managers.Instance.GameManager._chessPieces[_currentHover.x, _currentHover.y] != null && (Managers.Instance.GameManager._chessPieces[_currentHover.x, _currentHover.y].team == ETeam.White && Managers.Instance.TurnManager.currentTurn == ETeam.White) || (Managers.Instance.GameManager._chessPieces[_currentHover.x, _currentHover.y].team == ETeam.Black && Managers.Instance.TurnManager.currentTurn == ETeam.Black))
                {
                    _currentlyDragging = Managers.Instance.GameManager._chessPieces[_currentHover.x, _currentHover.y];
                    Managers.Instance.GameManager.CheckMoves(_currentlyDragging);
                    PreventCheck();
                    HighlightTiles();
                }
            }
            else
            {
                Vector2Int previousPosition = new(_currentlyDragging.currentX, _currentlyDragging.currentY);
                bool validMove = Managers.Instance.GameManager.MoveTo(_currentlyDragging, _currentHover.x, _currentHover.y);
                if (!validMove)
                {
                    _currentlyDragging.SetPosition(Managers.Instance.GameManager.GetTileCenter(previousPosition.x, previousPosition.y));
                }
                else
                {
                    _currentlyDragging.SetPosition(Managers.Instance.GameManager.GetTileCenter(_currentHover.x, _currentHover.y));
                }
                _currentlyDragging = null;
                RemoveHighlightTiles();
            }

        }
    }
    private void HighlightTiles()
    {
        for (int i = 0; i < Managers.Instance.GameManager._availableMoves.Count; i++)
        {
            Managers.Instance.GameManager.tiles[Managers.Instance.GameManager._availableMoves[i].x, Managers.Instance.GameManager._availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < Managers.Instance.GameManager._availableMoves.Count; i++)
        {
            Managers.Instance.GameManager.tiles[Managers.Instance.GameManager._availableMoves[i].x, Managers.Instance.GameManager._availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        Managers.Instance.GameManager._availableMoves.Clear();
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (Managers.Instance.GameManager._chessPieces[x, y] != null)
                {
                    if (Managers.Instance.GameManager._chessPieces[x, y].piece == EPiece.King && Managers.Instance.GameManager._chessPieces[x, y].team == _currentlyDragging.team)
                    {
                        targetKing = Managers.Instance.GameManager._chessPieces[x, y];
                    }
                }
            }
        }
        Managers.Instance.GameManager.SimulateForSinglePiece(_currentlyDragging, ref Managers.Instance.GameManager._availableMoves, targetKing);
    }
}
