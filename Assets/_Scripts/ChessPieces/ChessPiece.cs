using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChessPiece : NetworkBehaviour
{
    #region VARS
    public int currentX;
    public int currentY;
    public ETeam team;
    public EPieceSkin skin;
    public EPiece piece;

    private Vector3 _desiredPosition;
    private Vector3 _desiredScale = Vector3.one;
    #endregion
    #region ENGINE
    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == ETeam.White)? Vector3.zero: new Vector3(0,180,0));
        CmdApplyTransform();
    }
    #endregion
    #region MEMBER

    [ServerRpc(RequireOwnership =false)]
    public void CmdSetPosition(Vector3 position, bool force = false)
    {
        CmdApplyTransform();
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
        CmdApplyTransform();
        RpcSetPosition(position, force);
        _desiredPosition = position;
        if (force)
        {
            transform.position = _desiredPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CmdSetScale(Vector3 scale, bool force = false)
    {
        CmdApplyTransform();
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
        CmdApplyTransform();
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
    [ServerRpc(RequireOwnership = false)]
    private void CmdApplyTransform()
    {
        if (piece == EPiece.Knight)
        {
            transform.DOJump(_desiredPosition, 3, 1, 1);
        }
        else
        {
            transform.DOMove(_desiredPosition, 1);
        }
        transform.DOScale(_desiredScale, 1);

        RpcApplyTransform();
    }
    [ObserversRpc]
    private void RpcApplyTransform()
    {
        if (IsClientInitialized)
        {
            if (piece == EPiece.Knight)
            {
                transform.DOJump(_desiredPosition, 3, 1, 1);
            }
            else
            {
                transform.DOMove(_desiredPosition, 1);
            }
            transform.DOScale(_desiredScale, 1);
        }
    }
    [ObserversRpc]
    private void RpcSetPosition(Vector3 position, bool force = false)
    {
        if (IsClientInitialized)
        {
            _desiredPosition = position;
            if (force)
            {
                transform.position = _desiredPosition;
            }
        }

    }
    [ObserversRpc]
    private void RpcSetScale(Vector3 scale, bool force = false)
    {
        if (IsClientInitialized)
        {
            _desiredScale = scale;
            if (force)
            {
                transform.localScale = _desiredScale;
            }
        }

    }
    #endregion
}
public enum ETeam
{
    None = 0,
    Black = 1,
    White = 2
}
public enum EPieceSkin
{
    None = 0,
    HighPoly = 1,
    HighPolyWood = 2,
    LowerPoly = 3,
    LowerPolyWood = 4,
    LowPoly = 5,
    LowPolyWood = 6
}
public enum ESkin
{
    Classic = 0,
    Glass = 1,
    Silver = 2,
    Gold = 3,
    Crimson = 4
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

