using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ChessboardManager : NetworkBehaviour
{
    #region VARS
    #region PIECE PARAMS
    [Header("Piece Parameters")]
    public SpawnParameters spawnParameters;
    public DeathParameters deathParameters;
    #endregion
    #region PREFABS
    [Header("Prefabs & Materials")]
    [SerializeField] GameObject _environment;
    [SerializeField] GameObject _board;
    [SerializeField] Skins _skins;
    [SerializeField] AudioSource _pieceSFX;
    #endregion
    #region LOGIC
    [HideInInspector] public ChessPiece[,] chessPieces;
    [HideInInspector] public List<Vector2Int> availableMoves = new();
    [HideInInspector] public ESpecialMove eSpecialMove;
    [HideInInspector] public List<Vector2Int[]> moveList = new();
    [HideInInspector] public GameObject[,] tiles;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    Vector3 _bounds;
    #endregion
    #endregion
    #region MEMBER METHODS
    public void InstaniateChessBoard()
    {
        _environment.SetActive(true);
        GenerateAllTiles(spawnParameters.tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    public void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        _bounds = new Vector3((tileCountX / 2) * tileSize, spawnParameters.yOffset, (tileCountY / 2) * tileSize) + spawnParameters.boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
                //Spawn(tiles[x, y]);
            }
        }
    }
    [ObserversRpc]
    public void RpcAssignChessArray(List<ChessPiece> chessPieces)
    {
        if (chessPieces == null)
        {
            Debug.Log("Chess pieces null!");
            return; // Added return to prevent null reference exceptions
        }

        // Ensure the list has enough elements
        if (chessPieces.Count != TILE_COUNT_X * TILE_COUNT_Y)
        {
            Debug.LogError("Chess pieces list size does not match the board dimensions.");
            return;
        }

        ChessPiece[,] reconstructed = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                // Correct index calculation for row-major order: row * columns + column
                // Here, y is the row, x is the column
                int index = x * TILE_COUNT_X + y;
                reconstructed[x, y] = chessPieces[index];
            }
        }
        this.chessPieces = reconstructed;
    }
    public void SpawnAllPieces()
    {
        Debug.Log("initSpawn All Pieces");
        ServerSpawnAllPieces();
    }
    public void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);
    }
    public Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * spawnParameters.tileSize, spawnParameters.yOffset, y * spawnParameters.tileSize) - _bounds + new Vector3(spawnParameters.tileSize / 2, 0, spawnParameters.tileSize / 2);
    }
    #region OPS
    public void CheckMoves(ChessPiece currentlyDragging)
    {
        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
        eSpecialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
    }
    public bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
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
    public bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y))) { Debug.Log("No available moves"); return false; }
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
            {
                return false;
            }
            else if (ocp.team == ETeam.White)
            {
                if (ocp.piece == EPiece.King) { ChessManagers.Instance.GameManager.CmdCheckMate(ETeam.Black); }
                deathParameters.deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathParameters.deathSize);
                ocp.SetPosition(new Vector3(8 * spawnParameters.tileSize, spawnParameters.yOffset, -1 * spawnParameters.tileSize) - _bounds
                    + new Vector3(spawnParameters.tileSize / 2, 0, spawnParameters.tileSize / 2)
                    + (Vector3.forward * deathParameters.deathSpacing) * deathParameters.deadWhites.Count);
            }
            else if (ocp.team == ETeam.Black)
            {
                if (ocp.piece == EPiece.King) { ChessManagers.Instance.GameManager.CmdCheckMate(ETeam.White); }
                deathParameters.deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathParameters.deathSize);
                ocp.SetPosition(new Vector3(-1 * spawnParameters.tileSize, spawnParameters.yOffset, spawnParameters.tileSize * 8) - _bounds
                    + new Vector3(spawnParameters.tileSize / 2, 0, spawnParameters.tileSize / 2)
                    + (Vector3.back * deathParameters.deathSpacing) * deathParameters.deadBlacks.Count);
            }
        }

        CmdUpdateChessPiecePosition(cp, x, y);
        if (CheckForCheckmate())
        {
            ChessManagers.Instance.GameManager.CmdCheckMate(cp.team);
        }
        return true;
    }
    public Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one;
    }
    public void SimulateForSinglePiece(ChessPiece chessPiece, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        int actualX = chessPiece.currentX;
        int actualY = chessPiece.currentY;
        List<Vector2Int> movesToRemove = new();

        for (int i = 0; i < moves.Count; i++)
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
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
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
            simulation[simX, simY] = chessPiece;

            var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
            {
                simAttackingPieces.Remove(deadPiece);
            }

            List<Vector2Int> simMoves = new();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            chessPiece.currentX = actualX;
            chessPiece.currentY = actualY;
        }

        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }
    #endregion
    #endregion
    #region LOCAL METHODS
    #region GENERATE THE BOARD
    [Server]
    private void ServerSpawnBoard(ESkin skin)
    {
        string path = $"ChessPieces3D/Boards/{skin}";
        Debug.Log(path);
        GameObject board = Resources.Load<GameObject>(path);
        if (board != null)
        {
            board = Instantiate(board);
            board.transform.SetParent(_board.transform);
            Spawn(board);
            RpcSpawnBoard(skin);
        }
        else
        {
            Debug.Log("board null");
        }
    }
    [ObserversRpc]
    private void RpcSpawnBoard(ESkin skin)
    {
        string path = $"ChessPieces3D/Boards/{skin}";
        Debug.Log(path);
        GameObject board = Resources.Load<GameObject>(path);
        if (board != null)
        {
            board = Instantiate(board);
            board.transform.SetParent(_board.transform);
            Spawn(board);
        }
        else
        {
            Debug.Log("board null");
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = _board.transform;

        Mesh mesh = new();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        //tileObject.AddComponent<NetworkObject>();
        if ((x + y) % 2 == 0)
        {
            tileObject.AddComponent<MeshRenderer>().material = spawnParameters.blackTileMaterial;
        }
        else
        {
            tileObject.AddComponent<MeshRenderer>().material = spawnParameters.whiteTileMaterial;
        }

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, spawnParameters.yOffset, y * tileSize) - _bounds;
        vertices[1] = new Vector3(x * tileSize, spawnParameters.yOffset, (y + 1) * tileSize) - _bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, spawnParameters.yOffset, y * tileSize) - _bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, spawnParameters.yOffset, (y + 1) * tileSize) - _bounds;

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
    [Server]
    private void ServerSpawnAllPieces()
    {
        Debug.Log("Spawn All Pieces");
        chessPieces = new ChessPiece[TILE_COUNT_Y, TILE_COUNT_Y];
        #region WHITE
        chessPieces[0, 0] = SpawnSinglePiece(EPiece.Rook, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[1, 0] = SpawnSinglePiece(EPiece.Knight, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[2, 0] = SpawnSinglePiece(EPiece.Bishop, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[3, 0] = SpawnSinglePiece(EPiece.Queen, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[4, 0] = SpawnSinglePiece(EPiece.King, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[5, 0] = SpawnSinglePiece(EPiece.Bishop, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[6, 0] = SpawnSinglePiece(EPiece.Knight, ETeam.White, _skins.whitePlayerSkin);
        chessPieces[7, 0] = SpawnSinglePiece(EPiece.Rook, ETeam.White, _skins.whitePlayerSkin);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(EPiece.Pawn, ETeam.White, _skins.whitePlayerSkin);
        }
        #endregion

        #region BLACK                                                     
        chessPieces[0, 7] = SpawnSinglePiece(EPiece.Rook, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[1, 7] = SpawnSinglePiece(EPiece.Knight, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[2, 7] = SpawnSinglePiece(EPiece.Bishop, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[3, 7] = SpawnSinglePiece(EPiece.Queen, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[4, 7] = SpawnSinglePiece(EPiece.King, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[5, 7] = SpawnSinglePiece(EPiece.Bishop, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[6, 7] = SpawnSinglePiece(EPiece.Knight, ETeam.Black, _skins.blackPlayerSkin);
        chessPieces[7, 7] = SpawnSinglePiece(EPiece.Rook, ETeam.Black, _skins.blackPlayerSkin);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(EPiece.Pawn, ETeam.Black, _skins.blackPlayerSkin);
        }
        #endregion
        var flattenedChessPieces = new ChessPiece[TILE_COUNT_X * TILE_COUNT_Y];
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            for (int j = 0; j < TILE_COUNT_Y; j++)
            {
                flattenedChessPieces[i * TILE_COUNT_Y + j] = chessPieces[i, j];
            }
        }
        Debug.Log($"Assigning pieces, first piece: {chessPieces[0, 0]}");
        RpcAssignChessArray(flattenedChessPieces.ToList());
        ServerSpawnBoard(_skins.boardSkin);
    }

    private ChessPiece SpawnSinglePiece(EPiece pieceType, ETeam team, EPieceSkin skin)
    {
        string path = $"ChessPieces3D/{team}/{skin}/{pieceType}";
        GameObject pieceObject = Resources.Load<GameObject>(path);
        if (pieceObject != null)
        {
            ChessPiece piece = Instantiate(pieceObject).GetComponent<ChessPiece>();
            Spawn(piece.gameObject);
            return piece;
        }
        else
        {
            Debug.LogError($"Prefab not found at path: {path}");
            return null;
        }
    }
    #endregion
    #region POSITIONING
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        _pieceSFX.Play();
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        StartCoroutine(DelayedCmdChessPieceSetPosition(x, y, force));
    }
    private IEnumerator DelayedCmdChessPieceSetPosition(int x, int y, bool force = false)
    {
        while (!IsClientInitialized)
        {
            yield return null;
        }
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);

    }

    [ObserversRpc]
    private void RpcUpdateChessPiecePosition(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPosition = new(cp.currentX, cp.currentY);

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y);
        ChessManagers.Instance.TurnManager.SwitchTurn();
        ChessManagers.Instance.TurnManager.ResetTimer();
        moveList.Add(new Vector2Int[] { previousPosition, new(x, y) });

        ProcessSpecialMove();
        if (CheckForCheckmate())
        {
            ChessManagers.Instance.GameManager.CmdCheckMate(cp.team);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CmdUpdateChessPiecePosition(ChessPiece cp, int x, int y)
    {
        Vector2Int previousPosition = new(cp.currentX, cp.currentY);

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y);
        ChessManagers.Instance.TurnManager.SwitchTurn();
        ChessManagers.Instance.TurnManager.ResetTimer();
        moveList.Add(new Vector2Int[] { previousPosition, new(x, y) });

        ProcessSpecialMove();

        RpcUpdateChessPiecePosition(cp, x, y);
    }
    #endregion
    #region SPECIAL MOVES
    private void ProcessSpecialMove()
    {
        if (eSpecialMove == ESpecialMove.EnPassant)
        {
            Debug.Log("En passant");

            var newMove = moveList[^1];
            ChessPiece playerPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[^2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (playerPawn.currentX == enemyPawn.currentX && Mathf.Abs(playerPawn.currentY - enemyPawn.currentY) == 1)
            {
                if (enemyPawn.team == ETeam.White)
                {
                    if (enemyPawn.piece == EPiece.King) { ChessManagers.Instance.GameManager.CmdCheckMate(ETeam.Black); }
                    deathParameters.deadWhites.Add(enemyPawn);
                    enemyPawn.SetScale(Vector3.one * deathParameters.deathSize);
                    enemyPawn.SetPosition(new Vector3(deathParameters.deathStartOffsetModifier * spawnParameters.tileSize, spawnParameters.yOffset * deathParameters.deathYOffsetModifier, -deathParameters.deathDistanceOffsetModifier * spawnParameters.tileSize) - _bounds + new Vector3(spawnParameters.tileSize / 2, 0, spawnParameters.tileSize / 2) + (Vector3.left * deathParameters.deathSpacing) * deathParameters.deadWhites.Count);
                }
                if (enemyPawn.team == ETeam.Black)
                {
                    if (enemyPawn.piece == EPiece.King) { ChessManagers.Instance.GameManager.CmdCheckMate(ETeam.White); }
                    deathParameters.deadBlacks.Add(enemyPawn);
                    enemyPawn.SetScale(Vector3.one * deathParameters.deathSize);
                    enemyPawn.SetPosition(new Vector3(-(spawnParameters.tileSize + 1f), spawnParameters.yOffset * deathParameters.deathYOffsetModifier, spawnParameters.tileSize * (deathParameters.deathStartOffsetModifier + 1f)) - _bounds + new Vector3(spawnParameters.tileSize / 2, 0, spawnParameters.tileSize / 2) + (Vector3.right * deathParameters.deathSpacing) * deathParameters.deadBlacks.Count);
                }
                chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
            }
        }
        if (eSpecialMove == ESpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[^1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.piece == EPiece.Pawn)
            {
                if (targetPawn.team == ETeam.White && lastMove[1].y == 7)
                {
                    CmdPromotePawn(lastMove, ETeam.White);
                }
                if (targetPawn.team == ETeam.Black && lastMove[1].y == 0)
                {
                    CmdPromotePawn(lastMove, ETeam.Black);
                }
            }
        }
        if (eSpecialMove == ESpecialMove.Castling)
        {
            Debug.Log("Castling");

            Vector2Int[] lastMove = moveList[^1];
            //Left rook
            if (lastMove[1].x == 2)
            {
                //White
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                //black
                else if (lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            //Right rook
            else if (lastMove[1].x == 6)
            {
                //White
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                //black
                else if (lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CmdPromotePawn(Vector2Int[] lastMove, ETeam team)
    {
        Despawn(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
        ChessPiece nuQueen = SpawnSinglePiece(EPiece.Queen, team, _skins.whitePlayerSkin);
        nuQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
        chessPieces[lastMove[1].x, lastMove[1].y] = nuQueen;
        PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
        RpcPromotePawn(lastMove, nuQueen);
    }
    [ObserversRpc]
    private void RpcPromotePawn(Vector2Int[] lastMove, ChessPiece nuQueen)
    {
        nuQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
        chessPieces[lastMove[1].x, lastMove[1].y] = nuQueen;
        PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[^1];
        ETeam targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == ETeam.White) ? ETeam.Black : ETeam.White;
        List<ChessPiece> attackingPieces = new();
        List<ChessPiece> defendingPieces = new();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].piece == EPiece.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        List<Vector2Int> currentAvailableMoves = new();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            if (attackingPieces[i])
            {
                var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    currentAvailableMoves.Add(pieceMoves[b]);
                }
            }
        }
        if (currentAvailableMoves == null) Debug.Log("Current Available Moves is null in check for checkmate");
        if (targetKing == null) Debug.Log("Target King is null in check for checkmate");
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);

                if (defendingMoves.Count != 0) return false;
            }
            return true;
        }

        return false;
    }
    #endregion
    #endregion
}
