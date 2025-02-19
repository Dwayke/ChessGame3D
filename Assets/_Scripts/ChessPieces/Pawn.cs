using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();
        int direction = (team == Team.White) ? 1 : -1;
        //One in front
        if (board[currentX,currentY+direction] == null) 
        {
            r.Add(new Vector2Int(currentX,currentY+direction));
        }
        //Two int front
        if (board[currentX, currentY + direction] == null)
        {
            if (team == Team.White&& currentY == 1&& board[currentX,currentY+(direction*2)] == null) 
            {
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
            if (team == Team.Black&& currentY == 6&& board[currentX,currentY+(direction*2)] == null) 
            {
                r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }
        }
        //Diagonal KILL MOVE
        if (currentX!=tileCountX-1)
        {
            if (board[currentX+1,currentY+direction]!=null&& board[currentX + 1, currentY + direction].team!= team)
            {
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }        
        if (currentX!=0)
        {
            if (board[currentX-1,currentY+direction]!=null&& board[currentX - 1, currentY + direction].team!= team)
            {
                r.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }

        return r;
    }
}
