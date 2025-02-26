using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> availableMoves = new();
        //Top Right
        for (int x = currentX + 1, y = currentY + 1;x<tileCountX && y< tileCountY; x++, y++)
        {
            if (board[x,y] == null)
            {
                availableMoves.Add(new Vector2Int(x,y));
            }
            else
            {
                if(board[x,y].team != team)
                {
                    availableMoves.Add(new(x,y));
                }
                    break;
            }
        }
        //Top Left
        for (int x = currentX - 1, y = currentY + 1; x >=0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
            {
                availableMoves.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    availableMoves.Add(new(x, y));
                }
                    break;
            }
        }
        //Bottom Right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y>=0; x++, y--)
        {
            if (board[x, y] == null)
            {
                availableMoves.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    availableMoves.Add(new(x, y));
                }
                    break;
            }
        }
        //Bottom Left
        for (int x = currentX - 1, y = currentY - 1; x >=0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
            {
                availableMoves.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    availableMoves.Add(new(x, y));
                }
                    break;
            }
        }
        return availableMoves;
    }
}
