using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> availableMoves = new();
        //Right
        if(currentX + 1 < tileCountX)
        {
            //Right
            if(board[currentX + 1,currentY] == null)
            {
                availableMoves.Add(new Vector2Int(currentX+1, currentY));
            }else if(board[currentX + 1, currentY].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX + 1, currentY));
            }
            //Top Right
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX + 1, currentY+1] == null)
                {
                    availableMoves.Add(new Vector2Int(currentX + 1, currentY+1));
                }
                else if (board[currentX + 1, currentY+1].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX + 1, currentY+1));
                }
            }
            //Bottom Right
            if (currentY - 1 >=0)
            {
                if (board[currentX + 1, currentY - 1] == null)
                {
                    availableMoves.Add(new Vector2Int(currentX + 1, currentY - 1));
                }
                else if (board[currentX + 1, currentY - 1].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX + 1, currentY - 1));
                }
            }
        }
        //Left
        if (currentX - 1 >=0)
        {
            //Left
            if (board[currentX - 1, currentY] == null)
            {
                availableMoves.Add(new Vector2Int(currentX - 1, currentY));
            }
            else if (board[currentX - 1, currentY].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX - 1, currentY));
            }
            //Top Left
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX -1, currentY + 1] == null)
                {
                    availableMoves.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
                else if (board[currentX - 1, currentY + 1].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
            }
            //Bottom Left
            if (currentY - 1 >= 0)
            {
                if (board[currentX - 1, currentY - 1] == null)
                {
                    availableMoves.Add(new Vector2Int(currentX - 1, currentY - 1));
                }
                else if (board[currentX - 1, currentY - 1].team != team)
                {
                    availableMoves.Add(new Vector2Int(currentX - 1, currentY - 1));
                }
            }
        }
        //Up
        if (currentY + 1 < tileCountY)
        {
            if (board[currentX , currentY+1] == null)
            {
                availableMoves.Add(new Vector2Int(currentX , currentY+1));
            }
            else if (board[currentX, currentY+1].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX, currentY+1));
            }
        }
        //Down
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null)
            {
                availableMoves.Add(new Vector2Int(currentX, currentY - 1));
            }
            else if (board[currentX, currentY - 1].team != team)
            {
                availableMoves.Add(new Vector2Int(currentX, currentY - 1));
            }
        }
        return availableMoves;
    }
    public override ESpecialMove GetSpecialMoves(ref ChessPiece[,] chesspiece, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        ESpecialMove specialMove = ESpecialMove.None;

        var kingMove  = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == ETeam.White) ? 0 : 7));
        var leftRook  = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == ETeam.White) ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == ETeam.White) ? 0 : 7));

        if (kingMove == null && currentX == 4)
        {
            //White team
            if (team == ETeam.White)
            {
                //Left Rook
                if (leftRook == null)
                {
                    if (chesspiece[0, 0].piece == EPiece.Rook)
                    {
                        if (chesspiece[0, 0].team == ETeam.White)
                        {
                            if (chesspiece[3, 0] == null)
                            {
                                if (chesspiece[2, 0] == null)
                                {
                                    if (chesspiece[1, 0] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        specialMove = ESpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }
                //Right Rook
                if (rightRook == null)
                {
                    if (chesspiece[7, 0].piece == EPiece.Rook)
                    {
                        if (chesspiece[7, 0].team == ETeam.White)
                        {
                            if (chesspiece[6, 0] == null)
                            {
                                if (chesspiece[5, 0] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 0));
                                    specialMove = ESpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }
            //Black team
            else 
            {
                //Left Rook
                if (leftRook == null)
                {
                    if (chesspiece[0, 7].piece == EPiece.Rook)
                    {
                        if (chesspiece[0, 7].team == ETeam.Black)
                        {
                            if (chesspiece[3, 7] == null)
                            {
                                if (chesspiece[2, 7] == null)
                                {
                                    if (chesspiece[1, 7] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        specialMove = ESpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }
                //Right Rook
                if (rightRook == null)
                {
                    if (chesspiece[7, 7].piece == EPiece.Rook)
                    {
                        if (chesspiece[7, 7].team == ETeam.Black)
                        {
                            if (chesspiece[6, 7] == null)
                            {
                                if (chesspiece[5, 7] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    specialMove = ESpecialMove.Castling;
                                }
                            }
                        }
                    }
                }
            }
        }

        return specialMove;
    }
}
