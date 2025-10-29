using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameController))]
public class MiniMax : MonoBehaviour
{
    private PieceType maximizer;
    private PieceType minimizer;

    private GameController gameController;

    private void Awake()
    {
        gameController = GetComponent<GameController>();
    }

    //건들거면미니멕스 공부하고 오셈 ㅈㅂ 하
    internal Move FindBestMove(PieceType[,] board, bool findBestMove)
    {
        float bestValue = -Mathf.Infinity;
        Move bestMove = new Move();

        //최대최소 정의
        DefineMaxAndMin(findBestMove);

        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == gameController.EmptyCell())
                {
                    board[row, col] = maximizer;

                    float value = GetMiniMaxValue(board, 0, false);

                    board[row, col] = gameController.EmptyCell();

                    if (value > bestValue)
                    {
                        bestValue = value;

                        bestMove.row = row;
                        bestMove.col = col;
                    }
                }
            }
        }
        return bestMove;
    }

    private void DefineMaxAndMin(bool isMaxTheNPC)
    {
        if (isMaxTheNPC)
        {
            // NPc최대
            maximizer = gameController.NPC;
            minimizer = gameController.Player;
        }
        else
        {
            // 플레이어치소
            maximizer = gameController.Player;
            minimizer = gameController.NPC;
        }
    }

    private int Evaluate(PieceType[,] board)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            if (gameController.CheckLineMatch(board, row))
            {
                if (board[row, 0] == maximizer)
                {
                    return +10;
                }
                else if (board[row, 0] == minimizer)
                {
                    return -10;
                }
            }
        }

        for (int col = 0; col < board.GetLength(1); col++)
        {
            if (gameController.CheckColMatch(board, col))
            {
                if (board[0, col] == maximizer)
                {
                    return +10;
                }
                else if (board[0, col] == minimizer)
                {
                    return -10;
                }
            }
        }

        if (gameController.CheckRightDiagnoalMatch(board))
        {
            if (board[0, 0] == maximizer)
            {
                return +10;
            }
            else if (board[0, 0] == minimizer)
            {
                return -10;
            }
        }

        if (gameController.CheckLeftDiagnoalMatch(board))
        {
            if (board[0, 2] == maximizer)
            {
                return +10;
            }
            else if (board[0, 2] == minimizer)
            {
                return -10;
            }
        }

        return 0;
    }

    private float GetMiniMaxValue(PieceType[,] board,
        int depth, bool isMax)
    {
        float bestValue;

        int value = Evaluate(board);
        if (value == 10)
        {
            return value - depth;
        }

        if (value == -10)
        {
            return value + depth;
        }
            
        //무승부
        if (gameController.IsGameEnd(board))
        {
            return 0;
        }
            
        if (isMax)
        {
            bestValue = -Mathf.Infinity;

            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == gameController.EmptyCell())
                    {
                        board[row, col] = maximizer;

                        bestValue = Mathf.Max(bestValue,
                            GetMiniMaxValue(board, depth + 1, !isMax));

                        board[row, col] = gameController.EmptyCell();
                    }
                }
            }
            return bestValue;
        }
        else
        {
            bestValue = Mathf.Infinity;

            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == gameController.EmptyCell())
                    {
                
                        board[row, col] = minimizer;


                        bestValue = Math.Min(bestValue,
                            GetMiniMaxValue(board, depth + 1, !isMax));

               
                        board[row, col] = gameController.EmptyCell();
                    }
                }
            }
            return bestValue;
        }
    }

    //단독 행동평가를 확률로써의 변환ㅎ는 과정
    internal float EvaluateMove(PieceType[,] board, int row, int col, bool forNPC)
    {
        DefineMaxAndMin(forNPC);
        if (board[row, col] != gameController.EmptyCell())
        {
            return -Mathf.Infinity;
        }
        board[row, col] = maximizer;
        float value = GetMiniMaxValue(board, 0, false);
        board[row, col] = gameController.EmptyCell();
        return value;
    }
}
