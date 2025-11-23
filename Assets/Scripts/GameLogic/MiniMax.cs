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

    // 평가 데이터 추적
    public class MoveEvaluation
    {
        public float minimaxValue;
        public int maxDepth;
        public int nodeCount;
    }

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

    // 평가 데이터를 포함한 FindBestMove
    internal Move FindBestMoveWithEvaluation(PieceType[,] board, bool findBestMove, out MoveEvaluation evaluation)
    {
        float bestValue = -Mathf.Infinity;
        Move bestMove = new Move();
        evaluation = new MoveEvaluation { minimaxValue = -Mathf.Infinity, maxDepth = 0, nodeCount = 0 };

        DefineMaxAndMin(findBestMove);

        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == gameController.EmptyCell())
                {
                    board[row, col] = maximizer;

                    int maxDepth = 0;
                    int nodeCount = 0;
                    float value = GetMiniMaxValue(board, 0, false, ref maxDepth, ref nodeCount);

                    board[row, col] = gameController.EmptyCell();

                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestMove.row = row;
                        bestMove.col = col;
                        evaluation.minimaxValue = value;
                        evaluation.maxDepth = maxDepth;
                        evaluation.nodeCount = nodeCount;
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
        int depth, bool isMax, ref int maxDepth, ref int nodeCount)
    {
        nodeCount++;
        maxDepth = Mathf.Max(maxDepth, depth);

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
                            GetMiniMaxValue(board, depth + 1, !isMax, ref maxDepth, ref nodeCount));

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
                            GetMiniMaxValue(board, depth + 1, !isMax, ref maxDepth, ref nodeCount));

               
                        board[row, col] = gameController.EmptyCell();
                    }
                }
            }
            return bestValue;
        }
    }

    // 기존 호환성을 위한 오버로드
    private float GetMiniMaxValue(PieceType[,] board,
        int depth, bool isMax)
    {
        int maxDepth = 0;
        int nodeCount = 0;
        return GetMiniMaxValue(board, depth, isMax, ref maxDepth, ref nodeCount);
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

    // 평가 데이터를 포함한 EvaluateMove
    internal float EvaluateMoveWithData(PieceType[,] board, int row, int col, bool forNPC, out MoveEvaluation evaluation)
    {
        evaluation = new MoveEvaluation { minimaxValue = -Mathf.Infinity, maxDepth = 0, nodeCount = 0 };
        DefineMaxAndMin(forNPC);
        if (board[row, col] != gameController.EmptyCell())
        {
            evaluation.minimaxValue = -Mathf.Infinity;
            return -Mathf.Infinity;
        }
        
        // 현재 수를 둠
        board[row, col] = maximizer;
        
        // 수를 둔 후의 상태 평가 (상대방의 최선의 수를 찾음)
        int maxDepth = 0;
        int nodeCount = 0;
        float value = GetMiniMaxValue(board, 0, false, ref maxDepth, ref nodeCount);
        
        // 보드 복원
        board[row, col] = gameController.EmptyCell();
        
        // 평가 데이터 저장
        evaluation.minimaxValue = value;
        evaluation.maxDepth = maxDepth;
        evaluation.nodeCount = nodeCount;
        
        Debug.Log($"EvaluateMoveWithData: ({row}, {col}), forNPC={forNPC}, maximizer={maximizer}, minimizer={minimizer}, value={value}");
        
        return value;
    }
}
