using System;
using System.Collections.Generic;
using UnityEngine;

using static DifficultyManager;
using Random = UnityEngine.Random;



[RequireComponent(typeof(MiniMax))]
[RequireComponent(typeof(GameController))]
public class NPCController : MonoBehaviour
{
    [SerializeField] private MinimaxDebugger minimaxDebugger;
    internal Difficulties Difficulty { private get; set;  }

    [Tooltip("Value in %")]
    [SerializeField]
    private float mediumHardnessProbability = 70;

    private MiniMax miniMax;
    private GameController gameController;

    private void Awake()
    {
        miniMax = GetComponent<MiniMax>();
        gameController = GetComponent<GameController>();
    }

    internal List<(int, int, float)> GetMoveProbabilities(PieceType[,] board)
    {
        Move _;
        return GetMoveProbabilities(board, out _);
    }

    internal List<(int, int, float)> GetMoveProbabilities(PieceType[,] board, out Move bestMove)
    {
        List<(int, int)> empty = gameController.FindEmptyPlaces(board);
        bestMove = new Move();
        List<(int, int, float)> valued = new List<(int, int, float)>();

        if (empty.Count == 0)
        {
            return valued;
        }

        // 계산 제대로 되는거 맞노?
        float maxValue = -Mathf.Infinity;
        for (int i = 0; i < empty.Count; i++)
        {
            int r = empty[i].Item1;
            int c = empty[i].Item2;
            float v = miniMax.EvaluateMove(board, r, c, true);
            valued.Add((r, c, v));
            if (v > maxValue)
            {
                maxValue = v;
                bestMove.row = r;
                bestMove.col = c;
            }
        }

        const float temperature = 1.0f;
        float denom = 0f;
        float maxForStability = maxValue;
        for (int i = 0; i < valued.Count; i++)
        {
            float z = Mathf.Exp((valued[i].Item3 - maxForStability) / Mathf.Max(0.001f, temperature));
            denom += z;
            valued[i] = (valued[i].Item1, valued[i].Item2, z);
        }
        if (denom <= 0f)
        {
            for (int i = 0; i < valued.Count; i++)
            {
                valued[i] = (valued[i].Item1, valued[i].Item2, Mathf.Approximately(valued[i].Item3, maxValue) ? 1f : 0f);
            }
            return valued;
        }
        for (int i = 0; i < valued.Count; i++)
        {
            valued[i] = (valued[i].Item1, valued[i].Item2, valued[i].Item3 / denom);
        }
        if (minimaxDebugger != null)
        {
            string txt = "=== Minimax 평가 ===\n";
            foreach (var p in valued)
            {
                txt += $"({p.Item1}, {p.Item2}) → value={p.Item3:F2}\n";
            }
            txt += $"Best Move: ({bestMove.row}, {bestMove.col})";
            minimaxDebugger.UpdateText(txt);
        }
        return valued;
    }

    internal Move Play(PieceType[,] board)
    {
        Move move = new Move();

        if(Difficulty == Difficulties.Easy)
        {
            move = EasyMove(board);
        }
        else if(Difficulty == Difficulties.Medium)
        {
            move = MediumMove(board);
        }
        else if (Difficulty == Difficulties.Hard)
        {
            move = HardMove(board);
        }
        else
        {

        }

        return move;
    }

    private Move EasyMove(PieceType[,] board)
    {
        // Soft, mistake-prone behavior: often avoid best move and sometimes play fully random
        return ChooseMoveWithSampling(board, temperature: 2.5f, avoidBestProbability: 0.7f, randomMoveProbability: 0.3f);
    }

    private Move MediumMove(PieceType[,] board)
    {
        // 미니멕스 알고리즘의 손실률을 허물하게 만들어서 난이도를 급격히 낮추는 작업. 하기 싫노 하
        return ChooseMoveWithSampling(board, temperature: 1.2f, avoidBestProbability: 0.4f, randomMoveProbability: 0.1f);
    }

    private Move HardMove(PieceType[,] board)
    {
        Move move = new Move();

        if(gameController.IsFirstMove(board))
        {

            move.row = 0;
            move.col = 0;
        }
        else if(gameController.MovesPlayed(board) == 1 
            && board[1, 1] == gameController.EmptyCell())
        {
            move.row = 1;
            move.col = 1;
        }
        else
        {
            move = miniMax.FindBestMove(board, true);
        }

        return move;
    }

    private Move GetRandomMove(List<(int, int)> emptyPlaces)
    {
        int count = emptyPlaces.Count;
        int index = Random.Range(0, count);

        Move move = new Move(emptyPlaces[index].Item1, emptyPlaces[index].Item2);

        return move;
    }

    private Move ChooseMoveWithSampling(PieceType[,] board, float temperature, float avoidBestProbability, float randomMoveProbability)
    {
        List<(int, int)> emptyPlaces = gameController.FindEmptyPlaces(board);
        if (emptyPlaces.Count == 0)
        {
            return new Move();
        }

        
        if (Random.value < Mathf.Clamp01(randomMoveProbability))
        {
            return GetRandomMove(emptyPlaces);
        }

        // 알고리즘을 사용한 미니멕스 트리 점수 계산
        List<(int, int, float)> valued = new List<(int, int, float)>();
        float maxValue = -Mathf.Infinity;
        int bestR = -1;
        int bestC = -1;
        for (int i = 0; i < emptyPlaces.Count; i++)
        {
            int r = emptyPlaces[i].Item1;
            int c = emptyPlaces[i].Item2;
            float v = miniMax.EvaluateMove(board, r, c, true);
            valued.Add((r, c, v));
            if (v > maxValue)
            {
                maxValue = v;
                bestR = r;
                bestC = c;
            }
        }

        // 온도 변화에 따른 소프트맥스 분배
        float denom = 0f;
        float maxForStability = maxValue;
        for (int i = 0; i < valued.Count; i++)
        {
            float z = Mathf.Exp((valued[i].Item3 - maxForStability) / Mathf.Max(0.001f, temperature));
            denom += z;
            valued[i] = (valued[i].Item1, valued[i].Item2, z);
        }
        if (denom <= 0f)
        {

            return new Move(bestR, bestC);
        }

        // 최고 난이도. 좌호빈은 못함. 최적수를 계산해서 계산량을 극대화. 즉 절대 안짐
        bool avoidBest = Random.value < Mathf.Clamp01(avoidBestProbability);
        float adjustedDenom = 0f;
        if (avoidBest)
        {
            for (int i = 0; i < valued.Count; i++)
            {
                if (valued[i].Item1 == bestR && valued[i].Item2 == bestC)
                {
                    valued[i] = (valued[i].Item1, valued[i].Item2, 0f);
                }
                adjustedDenom += valued[i].Item3;
            }
            if (adjustedDenom <= 0f)
            {
                return GetRandomMove(emptyPlaces);
            }
        }
        else
        {
            adjustedDenom = denom;
        }

        float target = Random.value;
        float acc = 0f;
        for (int i = 0; i < valued.Count; i++)
        {
            float p = valued[i].Item3 / adjustedDenom;
            acc += p;
            if (target <= acc)
            {
                return new Move(valued[i].Item1, valued[i].Item2);
            }
        }

        return new Move(bestR, bestC);
    }
}
