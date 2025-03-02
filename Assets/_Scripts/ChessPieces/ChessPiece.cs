using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChessPiece : NetworkBehaviour
{
    #region VARS
    public int currentX;
    public int currentY;
    public ETeam team;
    public ESkin skin;
    public EPiece piece;

    private Vector3 _desiredPosition;
    private Vector3 _desiredScale = Vector3.one;
    #endregion
    #region ENGINE
    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == ETeam.White)? Vector3.zero: new Vector3(0,180,0));
    }
    private void Update()
    {
        CmdApplyTransform();
    }
    [ServerRpc(RequireOwnership = false)]
    private void CmdApplyTransform()
    {
        transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, _desiredScale, Time.deltaTime * 10);
        RpcApplyTransform();
    }
    [ObserversRpc]
    private void RpcApplyTransform()
    {
        transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, _desiredScale, Time.deltaTime * 10);
    }
    #endregion
    #region MEMBER
    [ObserversRpc(ExcludeOwner = true)]
    private void RpcSetPosition(Vector3 position, bool force = false)
    {
        _desiredPosition = position;
        if (force)
        {
            transform.position = _desiredPosition;
        }
    }
    [ServerRpc(RequireOwnership =false)]
    private void CmdSetPosition(Vector3 position, bool force = false)
    {
        RpcSetPosition(position, force);
        _desiredPosition = position;
        if (force)
        {
            transform.position = _desiredPosition;
        }
    }
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        CmdSetPosition(position, force);
        RpcSetPosition(position, force);
        _desiredPosition = position;
        if (force)
        {
            transform.position = _desiredPosition;
        }
    }
    [ObserversRpc]
    private void RpcSetScale(Vector3 scale, bool force = false)
    {
        _desiredScale = scale;
        if (force)
        {
            transform.localScale = _desiredScale;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CmdSetScale(Vector3 scale, bool force = false)
    {
        RpcSetScale(scale, force);
        _desiredScale = scale;
        if (force)
        {
            transform.localScale = _desiredScale;
        }
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        CmdSetScale(scale, force);
        RpcSetScale(scale, force);
        _desiredScale = scale;
        if (force)
        {
            transform.localScale = _desiredScale;
        }
    }
    public virtual ESpecialMove GetSpecialMoves(ref ChessPiece[,] chesspiece, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        ESpecialMove specialMove = ESpecialMove.None;
        return specialMove;
    }
    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> availableMoves = new();
        return availableMoves;
    }
    #endregion
    #region LOCAL

    #endregion
}
public enum ETeam
{
    None = 0,
    Black = 1,
    White = 2
}
public enum ESkin
{
    None = 0,
    HighPoly = 1,
    HighPolyWood = 2,
    LowerPoly = 3,
    LowerPolyWood = 4,
    LowPoly = 5,
    LowPolyWood = 6
}
public enum EPiece
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

