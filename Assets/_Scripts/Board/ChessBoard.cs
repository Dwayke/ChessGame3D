using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    #region VARS
    #region ART
    [Header("Art Parameters")]
    [SerializeField] Material _blackTileMaterial;
    [SerializeField] Material _whiteTileMaterial;
    [SerializeField] float _tileSize = 1;
    [SerializeField] Vector3 _boardCenter = Vector3.zero;
    [SerializeField] float _yOffset = 0;
    [SerializeField] float _deathStartOffsetModifier = 9f;
    [SerializeField] float _deathYOffsetModifier = 1.25f;
    [SerializeField] float _deathDistanceOffsetModifier = 3;
    [SerializeField] float _deathSize = 0.3f;
    [SerializeField] float _deathSpacing = 0.3f;
    [SerializeField] float _dragYOffset =1.5f;
    #endregion
    #region PREFABS
    [Header("Prefabs & Materials")]
    [SerializeField] Skin _whitePlayerSkin;
    [SerializeField] Skin _blackPlayerSkin;
    #endregion
    #region LOGIC
    private ChessPiece[,] _chessPieces;
    private ChessPiece _currentlyDragging;
    private List<Vector2Int> _availableMoves = new();
    private readonly List<ChessPiece> _deadWhites =new();
    private readonly List<ChessPiece> _deadBlacks = new();
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
        _isWhiteTurn = true;
        GenerateAllTiles(_tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void Update()
    {
        if (!_currentCamera)
        {
            _currentCamera = Camera.main;
            return;
        }
        CheckHoverStatus();
    }

    #endregion
    #region MEMBER
    #endregion
    #region LOCAL
    #region GENERATE THE BOARD
    private void GenerateAllTiles(float tileSize,int tileCountX,int tileCountY)
    {
        _bounds = new Vector3((tileCountX/2) * tileSize, _yOffset, (tileCountY / 2) * tileSize) +_boardCenter;

        _tiles = new GameObject[tileCountX,tileCountY];
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
        GameObject tileObject = new(string.Format("X:{0}, Y:{1}",x,y));
        tileObject.transform.parent = transform;

        Mesh mesh = new();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        if ((x+y)%2==0)
        {
            tileObject.AddComponent<MeshRenderer>().material = _blackTileMaterial;
        }
        else
        {
            tileObject.AddComponent<MeshRenderer>().material = _whiteTileMaterial;
        }

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x*tileSize,_yOffset, y * tileSize) - _bounds;
        vertices[1] = new Vector3(x*tileSize,_yOffset, (y + 1) * tileSize) - _bounds;
        vertices[2] = new Vector3((x+1)*tileSize,_yOffset, y * tileSize) - _bounds;
        vertices[3] = new Vector3((x+1)*tileSize, _yOffset, (y + 1) * tileSize) - _bounds;

        int[] tris = new int[] {0,1,2,1,3,2 };

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
        _chessPieces[0, 0] = SpawnSinglePiece(Piece.Rook, Team.White, _whitePlayerSkin);
        _chessPieces[1, 0] = SpawnSinglePiece(Piece.Knight, Team.White, _whitePlayerSkin);
        _chessPieces[2, 0] = SpawnSinglePiece(Piece.Bishop, Team.White, _whitePlayerSkin);
        _chessPieces[3, 0] = SpawnSinglePiece(Piece.Queen, Team.White, _whitePlayerSkin);
        _chessPieces[4, 0] = SpawnSinglePiece(Piece.King, Team.White, _whitePlayerSkin);
        _chessPieces[5, 0] = SpawnSinglePiece(Piece.Bishop, Team.White, _whitePlayerSkin);
        _chessPieces[6, 0] = SpawnSinglePiece(Piece.Knight, Team.White, _whitePlayerSkin);
        _chessPieces[7, 0] = SpawnSinglePiece(Piece.Rook, Team.White, _whitePlayerSkin);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            _chessPieces[i, 1] = SpawnSinglePiece(Piece.Pawn, Team.White, _whitePlayerSkin);
        }
        #endregion

        #region BLACK
        _chessPieces[0, 7] = SpawnSinglePiece(Piece.Rook,   Team.Black, _blackPlayerSkin);
        _chessPieces[1, 7] = SpawnSinglePiece(Piece.Knight, Team.Black, _blackPlayerSkin);
        _chessPieces[2, 7] = SpawnSinglePiece(Piece.Bishop, Team.Black, _blackPlayerSkin);
        _chessPieces[3, 7] = SpawnSinglePiece(Piece.Queen,  Team.Black, _blackPlayerSkin);
        _chessPieces[4, 7] = SpawnSinglePiece(Piece.King,   Team.Black, _blackPlayerSkin);
        _chessPieces[5, 7] = SpawnSinglePiece(Piece.Bishop, Team.Black, _blackPlayerSkin);
        _chessPieces[6, 7] = SpawnSinglePiece(Piece.Knight, Team.Black, _blackPlayerSkin);
        _chessPieces[7, 7] = SpawnSinglePiece(Piece.Rook,   Team.Black, _blackPlayerSkin);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            _chessPieces[i, 6] = SpawnSinglePiece(Piece.Pawn, Team.Black, _blackPlayerSkin);
        }
        #endregion
    }
    private ChessPiece SpawnSinglePiece(Piece pieceType, Team team, Skin skin)
    {
        string path = $"ChessPieces3D/{team}/{skin}/{pieceType}";
        GameObject pieceObject = Resources.Load<GameObject>(path) ;
        if (pieceObject != null)
        {
            ChessPiece piece = Instantiate(pieceObject, transform).GetComponent<ChessPiece>();            
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
        for (int x = 0;x < TILE_COUNT_X; x++)
           for(int y = 0;y < TILE_COUNT_Y; y++)
              if (_chessPieces[x,y] != null)
                 PositionSinglePiece(x,y,true);  
    }
    private void PositionSinglePiece(int x, int y, bool force = false) 
    {
        _chessPieces[x,y].currentX = x;
        _chessPieces[x,y].currentY = y;
        _chessPieces[x,y].SetPosition(GetTileCenter(x, y),force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * _tileSize, _yOffset, y * _tileSize) - _bounds + new Vector3(_tileSize/2,0,_tileSize/2);
    }
    #endregion
    #region OPS
    private void CheckHoverStatus()
    {
        Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 100, LayerMask.GetMask("Tile", "Hover","Highlight")))
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
            CheckClickStatus(hitPosition);
        }
        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                _tiles[_currentHover.x, _currentHover.y].layer = (ContainsValidMove(ref _availableMoves,_currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
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
            Plane horizontalPlane = new(Vector3.up, Vector3.up * _yOffset);
            if (horizontalPlane.Raycast(ray, out float distance))
            {
                _currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * _dragYOffset);
            }
        }
    }
    private void CheckClickStatus(Vector2Int hitposition)
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            if (_chessPieces[hitposition.x, hitposition.y] != null)
            {
                if ((_chessPieces[hitposition.x, hitposition.y].team == Team.White&&_isWhiteTurn)|| (_chessPieces[hitposition.x, hitposition.y].team == Team.Black && !_isWhiteTurn))
                {
                    _currentlyDragging = _chessPieces[hitposition.x, hitposition.y];
                    _availableMoves = _currentlyDragging.GetAvailableMoves(ref _chessPieces,TILE_COUNT_X,TILE_COUNT_Y);
                    HighlightTiles();
                }
            }
        }
        if (_currentlyDragging!=null&&Input.GetMouseButtonUp(0)) 
        {
            Vector2Int previousPosition = new(_currentlyDragging.currentX, _currentlyDragging.currentY);

            bool validMove = MoveTo(_currentlyDragging, hitposition.x, hitposition.y);
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
    private bool ContainsValidMove(ref List<Vector2Int> moves,Vector2 pos)
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
        if (!ContainsValidMove(ref _availableMoves, new Vector2(x, y))) return false;
        Vector2Int previousPosition = new(cp.currentX, cp.currentY);

        if (_chessPieces[x,y]!= null)
        {
            ChessPiece ocp = _chessPieces[x, y];
            if (cp.team == ocp.team) 
            {
                return false;
            }
            else if(ocp.team == Team.White)
            {
                _deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * _deathSize);
                ocp.SetPosition(new Vector3(_deathStartOffsetModifier * _tileSize,_yOffset * _deathYOffsetModifier, -_deathDistanceOffsetModifier*_tileSize)-_bounds + new Vector3(_tileSize/2,0,_tileSize/2)+ (Vector3.left* _deathSpacing)*_deadWhites.Count);
            }            
            else if(ocp.team == Team.Black)
            {
                _deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * _deathSize);
                ocp.SetPosition(new Vector3(-(_tileSize+1f), _yOffset* _deathYOffsetModifier,  _tileSize * (_deathStartOffsetModifier + 1f)) - _bounds + new Vector3(_tileSize / 2, 0, _tileSize / 2) + (Vector3.right * _deathSpacing) * _deadBlacks.Count);
            }
        }

        _chessPieces[x, y] = cp;
        _chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        _isWhiteTurn = !_isWhiteTurn;
        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (_tiles[x,y] == hitInfo)
                {
                    return new Vector2Int(x,y);
                }
            }
        }
        return -Vector2Int.one;
    }
    #endregion
    #endregion
}
