using System;
using System.Collections.Generic;
using UnityEngine;

using static DifficultyManager;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MiniMax))]
[RequireComponent(typeof(GameController))]
public class NPCController : MonoBehaviour
{
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
        List<(int, int)> emptyPlaces = gameController.FindEmptyPlaces(board);

        Move move = new Move();
        move = GetRandomMove(emptyPlaces);

        return move;
    }

    private Move MediumMove(PieceType[,] board)
    {
        Move move = new Move();

        if (Random.Range(0, 100 + 1) <= mediumHardnessProbability)
        {
            move = miniMax.FindBestMove(board, true);
        }
        else
        {
            List<(int, int)> emptyPlaces = gameController.FindEmptyPlaces(board);
            move = GetRandomMove(emptyPlaces);
        }

        return move;
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
}
