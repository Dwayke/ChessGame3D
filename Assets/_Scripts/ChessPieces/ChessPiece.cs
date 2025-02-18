using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    #region VARS
    public int currentX;
    public int currentY;
    public Team team;
    public Skin skin;
    public Piece piece;

    private Vector3 _desiredPosition;
    private Vector3 _desiredScale;
    #endregion
    #region MEMBER
    //SMOOTH MOVEMENT
    #endregion
}
public enum Team
{
    None = 0,
    Black = 1,
    White = 2
}
public enum Skin
{
    None = 0,
    HighPoly = 1,
    HighPolyWood = 2,
    LowerPoly = 3,
    LowerPolyWood = 4,
    LowPoly = 5,
    LowPolyWood = 6
}
public enum Piece
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}
