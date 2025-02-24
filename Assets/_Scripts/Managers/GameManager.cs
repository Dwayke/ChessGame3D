using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.HID.HID;

public class GameManager : MonoBehaviour
{
    #region VARS
    public static GameManager Instance;

    #region ART
    [Header("Art Parameters")]
    [SerializeField] SpawnParameters _spawnParameters;
    [SerializeField] DeathParameters _deathParameters;
    [SerializeField] float _dragYOffset = 1.5f;
    [SerializeField] GameObject _victoryScreen;
    [SerializeField] TMP_Text _victoryText;
    #endregion
    #region PREFABS
    [Header("Prefabs & Materials")]
    [SerializeField] GameObject _board;
    [SerializeField] Skins _skins;
    #endregion
    #region LOGIC
    private ChessControls _chessControls;
    private ChessPiece[,] _chessPieces;
    private ChessPiece _currentlyDragging;
    private List<Vector2Int> _availableMoves = new();
    private ESpecialMove _eSpecialMove;
    private List<Vector2Int[]> _moveList = new();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] _tiles;
    private Camera _currentCamera;
    private Vector2Int _currentHover;
    Vector3 _bounds;
    bool _isWhiteTurn;
    #endregion
    #endregion
    #region ENGINE
    private void Awake()
    {
        Instance = this;
        _chessControls = new ChessControls();
        _isWhiteTurn = true;
        GenerateAllTiles(_spawnParameters.tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void OnEnable()
    {
        _chessControls.Gameplay.Click.performed += OnClick;
        _chessControls.Gameplay.Release.performed += OnRelease;
        _chessControls.Gameplay.Enable();
    }
    private void OnDisable()
    {
        _chessControls.Gameplay.Click.performed -= OnClick;
        _chessControls.Gameplay.Release.performed -= OnRelease;
        _chessControls.Gameplay.Disable();
    }
    void Update()
    {
        if (!_currentCamera)
        {
            _currentCamera = Camera.main;
            return;
        }
        CheckHoverStatus();
    }
    #endregion
    #region MEMBER METHODS
    public void CheckMate(ETeam winner)
    {
        DisplayVictory(winner);
    }
    public void DisplayVictory(ETeam winner)
    {
        _victoryScreen.SetActive(true);
        _victoryText.text = winner.ToString() + " Team Won!";
    }
    public void OnResetButton()
    {
        //UI
        _victoryScreen.SetActive(false);
        _victoryText.text = "";
        //FieldReset
        _currentlyDragging = null;
        _availableMoves.Clear();
        _moveList.Clear();
        //CleanUp
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (_chessPieces[x, y] != null)
                {
                    Destroy(_chessPieces[x, y].gameObject);
                }
                _chessPieces[x, y] = null;
            }
        }
        for (int i = 0; i < _deathParameters.deadWhites.Count; i++)
        {
            Destroy(_deathParameters.deadWhites[i].gameObject);
        }
        for (int i = 0; i < _deathParameters.deadBlacks.Count; i++)
        {
            Destroy(_deathParameters.deadBlacks[i].gameObject);
        }
        _deathParameters.deadWhites.Clear();
        _deathParameters.deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        _isWhiteTurn = true;
    }
    public void OnExitButton()
    {
        Application.Quit();
    }
    #endregion
    #region LOCAL METHODS
    #region GENERATE THE BOARD
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        _bounds = new Vector3((tileCountX / 2) * tileSize, _spawnParameters.yOffset, (tileCountY / 2) * tileSize) + _spawnParameters.boardCenter;

        _tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                _tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = _board.transform;

        Mesh mesh = new();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        if ((x + y) % 2 == 0)
        {
            tileObject.AddComponent<MeshRenderer>().material = _spawnParameters.blackTileMaterial;
        }
        else
        {
            tileObject.AddComponent<MeshRenderer>().material = _spawnParameters.whiteTileMaterial;
        }

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, _spawnParameters.yOffset, y * tileSize) - _bounds;
        vertices[1] = new Vector3(x * tileSize, _spawnParameters.yOffset, (y + 1) * tileSize) - _bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, _spawnParameters.yOffset, y * tileSize) - _bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, _spawnParameters.yOffset, (y + 1) * tileSize) - _bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateBounds();


        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }
    #endregion
    #region SPAWN PIECES
    private void SpawnAllPieces()
    {
        _chessPieces = new ChessPiece[TILE_COUNT_Y, TILE_COUNT_Y];
        #region WHITE
        _chessPieces[0, 0] = SpawnSinglePiece(EPiece.Rook, ETeam.White,     _skins.whitePlayerSkin);
        _chessPieces[1, 0] = SpawnSinglePiece(EPiece.Knight, ETeam.White,   _skins.whitePlayerSkin);
        _chessPieces[2, 0] = SpawnSinglePiece(EPiece.Bishop, ETeam.White,   _skins.whitePlayerSkin);
        _chessPieces[3, 0] = SpawnSinglePiece(EPiece.Queen, ETeam.White,    _skins.whitePlayerSkin);
        _chessPieces[4, 0] = SpawnSinglePiece(EPiece.King, ETeam.White,     _skins.whitePlayerSkin);
        _chessPieces[5, 0] = SpawnSinglePiece(EPiece.Bishop, ETeam.White,   _skins.whitePlayerSkin);
        _chessPieces[6, 0] = SpawnSinglePiece(EPiece.Knight, ETeam.White,   _skins.whitePlayerSkin);
        _chessPieces[7, 0] = SpawnSinglePiece(EPiece.Rook, ETeam.White,     _skins.whitePlayerSkin);
        for (int i = 0; i < TILE_COUNT_X; i++)                         
        {                                                              
            _chessPieces[i, 1] = SpawnSinglePiece(EPiece.Pawn, ETeam.White, _skins.whitePlayerSkin);
        }                                                                 
        #endregion                                                        
                                                                          
        #region BLACK                                                     
        _chessPieces[0, 7] = SpawnSinglePiece(EPiece.Rook, ETeam.Black,     _skins.blackPlayerSkin);
        _chessPieces[1, 7] = SpawnSinglePiece(EPiece.Knight, ETeam.Black,   _skins.blackPlayerSkin);
        _chessPieces[2, 7] = SpawnSinglePiece(EPiece.Bishop, ETeam.Black,   _skins.blackPlayerSkin);
        _chessPieces[3, 7] = SpawnSinglePiece(EPiece.Queen, ETeam.Black,    _skins.blackPlayerSkin);
        _chessPieces[4, 7] = SpawnSinglePiece(EPiece.King, ETeam.Black,     _skins.blackPlayerSkin);
        _chessPieces[5, 7] = SpawnSinglePiece(EPiece.Bishop, ETeam.Black,   _skins.blackPlayerSkin);
        _chessPieces[6, 7] = SpawnSinglePiece(EPiece.Knight, ETeam.Black,   _skins.blackPlayerSkin);
        _chessPieces[7, 7] = SpawnSinglePiece(EPiece.Rook, ETeam.Black,     _skins.blackPlayerSkin);
        for (int i = 0; i < TILE_COUNT_X; i++)                          
        {                                                               
            _chessPieces[i, 6] = SpawnSinglePiece(EPiece.Pawn, ETeam.Black, _skins.blackPlayerSkin);
        }
        #endregion
    }
    private ChessPiece SpawnSinglePiece(EPiece pieceType, ETeam team, ESkin skin)
    {
        string path = $"ChessPieces3D/{team}/{skin}/{pieceType}";
        GameObject pieceObject = Resources.Load<GameObject>(path);
        if (pieceObject != null)
        {
            ChessPiece piece = Instantiate(pieceObject, _board.transform).GetComponent<ChessPiece>();
            return piece;
        }
        else
        {
            Debug.LogError($"Sprite not found at path: {path}");
            return null;
        }
    }
    #endregion
    #region POSITIONING
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (_chessPieces[x, y] != null)
                    PositionSinglePiece(x, y);
    }
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        _chessPieces[x, y].currentX = x;
        _chessPieces[x, y].currentY = y;
        _chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * _spawnParameters.tileSize, _spawnParameters.yOffset, y * _spawnParameters.tileSize) - _bounds + new Vector3(_spawnParameters.tileSize / 2, 0, _spawnParameters.tileSize / 2);
    }
    #endregion
    #region OPS
    private void CheckHoverStatus()
    {
        Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);
            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if (_currentHover != hitPosition)
            {
                _tiles[_currentHover.x, _currentHover.y].layer = (ContainsValidMove(ref _availableMoves, _currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }
        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                _tiles[_currentHover.x, _currentHover.y].layer = (ContainsValidMove(ref _availableMoves, _currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                _currentHover = -Vector2Int.one;
            }
            if (_currentlyDragging && Input.GetMouseButtonUp(0))
            {
                _currentlyDragging.SetPosition(GetTileCenter(_currentlyDragging.currentX, _currentlyDragging.currentY));
                _currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        if (_currentlyDragging)
        {
            Plane horizontalPlane = new(Vector3.up, Vector3.up * _spawnParameters.yOffset);
            if (horizontalPlane.Raycast(ray, out float distance))
            {
                _currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * _dragYOffset);
            }
        }
    }
    private void OnClick(InputAction.CallbackContext obj)
    {
        if (_currentHover!= -Vector2Int.one)
        {
            if (_chessPieces[_currentHover.x, _currentHover.y] != null)
            {
                if ((_chessPieces[_currentHover.x, _currentHover.y].team == ETeam.White && _isWhiteTurn) || (_chessPieces[_currentHover.x, _currentHover.y].team == ETeam.Black && !_isWhiteTurn))
                {
                    _currentlyDragging = _chessPieces[_currentHover.x, _currentHover.y];
                    _availableMoves = _currentlyDragging.GetAvailableMoves(ref _chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                    _eSpecialMove = _currentlyDragging.GetSpecialMoves(ref _chessPieces, ref _moveList, ref _availableMoves);
                    PreventCheck();
                    HighlightTiles();
                }
            }
        }
    }
    private void OnRelease(InputAction.CallbackContext obj)
    {
        if (_currentlyDragging != null)
        {
            Vector2Int previousPosition = new(_currentlyDragging.currentX, _currentlyDragging.currentY);
            bool validMove = MoveTo(_currentlyDragging, _currentHover.x, _currentHover.y);
            if (!validMove)
                _currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
            _currentlyDragging = null;
            RemoveHighlightTiles();
        }
    }
    private void HighlightTiles()
    {
        for (int i = 0; i < _availableMoves.Count; i++)
        {
            _tiles[_availableMoves[i].x, _availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < _availableMoves.Count; i++)
        {
            _tiles[_availableMoves[i].x, _availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        _availableMoves.Clear();
    }
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref _availableMoves, new Vector2Int(x, y))) return false;
        Vector2Int previousPosition = new(cp.currentX, cp.currentY);

        if (_chessPieces[x, y] != null)
        {
            ChessPiece ocp = _chessPieces[x, y];
            if (cp.team == ocp.team)
            {
                return false;
            }
            else if (ocp.team == ETeam.White)
            {
                if (ocp.piece == EPiece.King) { CheckMate(ETeam.Black); }
                _deathParameters.deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * _deathParameters.deathSize);
                ocp.SetPosition(new Vector3(_deathParameters.deathStartOffsetModifier * _spawnParameters.tileSize, _spawnParameters.yOffset * _deathParameters.deathYOffsetModifier, -_deathParameters.deathDistanceOffsetModifier * _spawnParameters.tileSize) - _bounds + new Vector3(_spawnParameters.tileSize / 2, 0, _spawnParameters.tileSize / 2) + (Vector3.left * _deathParameters.deathSpacing) * _deathParameters.deadWhites.Count);
            }
            else if (ocp.team == ETeam.Black)
            {
                if (ocp.piece == EPiece.King) { CheckMate(ETeam.White); }
                _deathParameters.deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * _deathParameters.deathSize);
                ocp.SetPosition(new Vector3(-(_spawnParameters.tileSize + 1f), _spawnParameters.yOffset * _deathParameters.deathYOffsetModifier, _spawnParameters.tileSize * (_deathParameters.deathStartOffsetModifier + 1f)) - _bounds + new Vector3(_spawnParameters.tileSize / 2, 0, _spawnParameters.tileSize / 2) + (Vector3.right * _deathParameters.deathSpacing) * _deathParameters.deadBlacks.Count);
            }
        }

        _chessPieces[x, y] = cp;
        _chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        _isWhiteTurn = !_isWhiteTurn;
        _moveList.Add(new Vector2Int[] {previousPosition, new(x, y) });

        ProcessSpecialMove();
        if (CheckForCheckmate()) 
        {
            CheckMate(cp.team);
        }

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (_tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one;
    }
    #endregion
    #region SPECIAL MOVES 
    private void ProcessSpecialMove() 
    {
        if(_eSpecialMove == ESpecialMove.EnPassant)
        {
            var newMove = _moveList[^1];
            ChessPiece playerPawn = _chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = _moveList[^2];
            ChessPiece enemyPawn = _chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(playerPawn.currentX == enemyPawn.currentX&& Mathf.Abs(playerPawn.currentY-enemyPawn.currentY) == 1)
            {
                if (enemyPawn.team==ETeam.White)
                {
                    if (enemyPawn.piece == EPiece.King) { CheckMate(ETeam.Black); }
                    _deathParameters.deadWhites.Add(enemyPawn);
                    enemyPawn.SetScale(Vector3.one * _deathParameters.deathSize);
                    enemyPawn.SetPosition(new Vector3(_deathParameters.deathStartOffsetModifier * _spawnParameters.tileSize, _spawnParameters.yOffset * _deathParameters.deathYOffsetModifier, -_deathParameters.deathDistanceOffsetModifier * _spawnParameters.tileSize) - _bounds + new Vector3(_spawnParameters.tileSize / 2, 0, _spawnParameters.tileSize / 2) + (Vector3.left * _deathParameters.deathSpacing) * _deathParameters.deadWhites.Count);
                }
                if (enemyPawn.team == ETeam.Black)
                {
                    if (enemyPawn.piece == EPiece.King) { CheckMate(ETeam.White); }
                    _deathParameters.deadBlacks.Add(enemyPawn);
                    enemyPawn.SetScale(Vector3.one * _deathParameters.deathSize);
                    enemyPawn.SetPosition(new Vector3(-(_spawnParameters.tileSize + 1f), _spawnParameters.yOffset * _deathParameters.deathYOffsetModifier, _spawnParameters.tileSize * (_deathParameters.deathStartOffsetModifier + 1f)) - _bounds + new Vector3(_spawnParameters.tileSize / 2, 0, _spawnParameters.tileSize / 2) + (Vector3.right * _deathParameters.deathSpacing) * _deathParameters.deadBlacks.Count);
                }
                _chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
            }
        }
        if(_eSpecialMove == ESpecialMove.Promotion)
        {
            Vector2Int[] lastMove = _moveList[^1];
            ChessPiece targetPawn = _chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.piece==EPiece.Pawn)
            {
                if (targetPawn.team == ETeam.White && lastMove[1].y == 7)
                {
                    ChessPiece nuQueen = SpawnSinglePiece(EPiece.Queen, ETeam.White,_skins.whitePlayerSkin);
                    nuQueen.transform.position = _chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(_chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    _chessPieces[lastMove[1].x, lastMove[1].y] = nuQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y,true);
                }
                if (targetPawn.team == ETeam.Black && lastMove[1].y == 0)
                {
                    ChessPiece nuQueen = SpawnSinglePiece(EPiece.Queen, ETeam.Black, _skins.blackPlayerSkin);
                    nuQueen.transform.position = _chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(_chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    _chessPieces[lastMove[1].x, lastMove[1].y] = nuQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
                }
            }
        }
        if(_eSpecialMove == ESpecialMove.Castling)
        {
            Vector2Int[] lastMove = _moveList[^1];
            //Left rook
            if (lastMove[1].x == 2)
            {
                //White
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = _chessPieces[0, 0];
                    _chessPieces[3,0] = rook;
                    PositionSinglePiece(3, 0);
                    _chessPieces[0, 0] = null;
                }
                //black
                else if (lastMove[1].y == 7)
                {
                    ChessPiece rook = _chessPieces[0, 7];
                    _chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    _chessPieces[0, 7] = null;
                }
            }
            //Right rook
            else if (lastMove[1].x == 6)
            {
                //White
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = _chessPieces[7, 0];
                    _chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    _chessPieces[7, 0] = null;
                }
                //black
                else if (lastMove[1].y == 7)
                {
                    ChessPiece rook = _chessPieces[7, 7];
                    _chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    _chessPieces[7, 7] = null;
                }
            }
        }
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if(_chessPieces[x, y]!= null)
                {
                    if (_chessPieces[x, y].piece == EPiece.King && _chessPieces[x,y].team == _currentlyDragging.team) 
                    {
                        targetKing = _chessPieces[x, y];
                    }
                }
            }
        }
        SimulateForSinglePiece(_currentlyDragging, ref _availableMoves, targetKing);
    }
    private void SimulateForSinglePiece(ChessPiece chessPiece,ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        int actualX = chessPiece.currentX;
        int actualY = chessPiece.currentY;
        List<Vector2Int> movesToRemove = new();

        for (int i = 0;i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new(targetKing.currentX, targetKing.currentY);

            if (chessPiece.piece == EPiece.King)
            {
                kingPositionThisSim = new Vector2Int(simX, simY);
            }

            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPieces = new();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (_chessPieces[x,y]!=null)
                    {
                        simulation[x, y] = _chessPieces[x, y];
                        if (simulation[x, y].team != chessPiece.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }
            simulation[actualX, actualY] = null;
            chessPiece.currentX = simX;
            chessPiece.currentY = simY;
            simulation[simX,simY] = chessPiece;

            var deadPiece = simAttackingPieces.Find(c=>c.currentX == simX && c.currentY == simY);
            if (deadPiece!=null)
            {
                simAttackingPieces.Remove(deadPiece);
            }

            List<Vector2Int> simMoves = new();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation,TILE_COUNT_X,TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            if (ContainsValidMove(ref simMoves,kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            chessPiece.currentX = actualX;
            chessPiece.currentY = actualY;
        }

        for (int i = 0;i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }
    private bool CheckForCheckmate()
    {
        var lastMove = _moveList[_moveList.Count - 1];
        ETeam targetTeam = (_chessPieces[lastMove[1].x, lastMove[1].y].team == ETeam.White) ? ETeam.Black:ETeam.White;

        List<ChessPiece> attackingPieces = new();
        List<ChessPiece> defendingPieces = new();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (_chessPieces[x, y] != null)
                {
                    if (_chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(_chessPieces[x, y]);
                        if (_chessPieces[x, y].piece == EPiece.King)
                        {
                            targetKing = _chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(_chessPieces[x, y]);
                    }
                }
            }
        }

        List<Vector2Int> currentAvailableMoves = new();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref _chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int b = 0; b < pieceMoves.Count; b++)
            {
                currentAvailableMoves.Add(pieceMoves[b]);
            }
        }
        if (ContainsValidMove(ref currentAvailableMoves,new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref _chessPieces,TILE_COUNT_X,TILE_COUNT_Y);
                SimulateForSinglePiece(defendingPieces[i],ref defendingMoves,targetKing);

                if (defendingMoves.Count != 0) return false;
            }
            return true;
        }

        return false;
    }
    #endregion
    #endregion
}
[System.Serializable]
public class SpawnParameters
{
    public Material blackTileMaterial;
    public Material whiteTileMaterial;
    public float tileSize = 1;
    public Vector3 boardCenter = Vector3.zero;
    public float yOffset = 0;
}
[System.Serializable]
public class DeathParameters
{
    public float deathStartOffsetModifier = 9f;
    public float deathYOffsetModifier = 1.25f;
    public float deathDistanceOffsetModifier = 3;
    public float deathSize = 0.3f;
    public float deathSpacing = 0.3f;
    public readonly List<ChessPiece> deadWhites = new();
    public readonly List<ChessPiece> deadBlacks = new();
}
[System.Serializable]
public class Skins
{
    public ESkin whitePlayerSkin;
    public ESkin blackPlayerSkin;
}

public enum ESpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion =3
}
