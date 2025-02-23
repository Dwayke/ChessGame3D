using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> availableMoves = new();
        int direction = (team == ETeam.White) ? 1 : -1;
        //One in front
        if (board[currentX,currentY+direction] == null) 
        {
            availableMoves.Add(new Vector2Int(currentX,currentY+direction));
        }
        //Two int front
        if (board[currentX, currentY + direction] == null)
        {
            if (team == ETeam.White&& currentY == 1&& board[currentX,currentY+(direction*2)] == null) 
            {
                availableMoves.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
            if (team == ETeam.Black&& currentY == 6&& board[currentX,currentY+(direction*2)] == null) 
            {
                availableMoves.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
        }
        //Diagonal KILL MOVE
        if (currentX!=tileCountX-1)
        {
            if (board[currentX+1,currentY+direction]!=null&& board[currentX + 1, currentY + direction].team!= team)
            {
                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }        
        if (currentX!=0)
        {
            if (board[currentX-1,currentY+direction]!=null&& board[currentX - 1, currentY + direction].team!= team)
            {
                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        return availableMoves;
    }

    public override ESpecialMove GetSpecialMoves(ref ChessPiece[,] chesspiece, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == ETeam.White) ? 1 : -1;
        if ((team == ETeam.White && currentY == 6)||(team == ETeam.Black&&currentY ==1))
        {

            return ESpecialMove.Promotion;
        }
        ESpecialMove specialMove = ESpecialMove.None;
        if (moveList.Count>0)
        {
            Vector2Int[] lastMove = moveList[^1];
            if (chesspiece[lastMove[1].x,lastMove[1].y].piece == EPiece.Pawn)
            {
                if (Mathf.Abs(lastMove[0].y- lastMove[1].y) == 2)
                {
                    if (chesspiece[lastMove[1].x, lastMove[1].y].team!= team)
                    {
                        if (lastMove[1].y==currentY)
                        {
                            if (lastMove[1].x==currentX-1)
                            {
                                availableMoves.Add(new Vector2Int(currentX-1, currentY+direction));
                                return ESpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return ESpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return specialMove;
    }
}
