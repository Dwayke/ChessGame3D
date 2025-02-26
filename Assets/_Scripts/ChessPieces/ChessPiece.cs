using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
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
        transform.position = Vector3.Lerp(transform.position,_desiredPosition,Time.deltaTime*10);
        transform.localScale = Vector3.Lerp(transform.localScale,_desiredScale,Time.deltaTime*10);
    }
    #endregion
    #region MEMBER
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        _desiredPosition = position;
        if (force)
        {
            transform.position = _desiredPosition;
        }
        else
        {

        }
    }    
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        _desiredScale = scale;
        if (force)
        {
            transform.localScale = _desiredScale;
        }
        else
        {

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

