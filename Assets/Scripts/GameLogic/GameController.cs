using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(NPCController))]
public class GameController : MonoBehaviour
{
    internal static Action<bool> OnPlayerTurn;
    internal static Action<PieceTemplate> OnPieceSelected;
    internal static Action<string> OnGameEnd;

    internal bool IsRunning { get; private set; } = true;
    internal PieceType NPC { get; set; }
    internal PieceType Player { get; set; }

    [SerializeField]
    private AudioClip win;
    [SerializeField]
    private AudioClip draw;
    [SerializeField]
    private AudioClip lost;
    [SerializeField]
    private PieceTemplate cross;
    [SerializeField]
    private PieceTemplate circle;
    [SerializeField]
    private  Slot[] slots;

    private PieceTemplate npcPiece;
    private PieceTemplate playerPiece; 
    private bool isPlayerTurn;
    private PieceType[,] board = new PieceType[3, 3];
    private Move lastPlayerMove = new Move(-1, -1);

    private PlayerController playerController;
    private NPCController npcController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        npcController = GetComponent<NPCController>();

        if (slots == null || slots.Length == 0)
        {
            slots = GetComponentsInChildren<Slot>(true);
        }

        // 평가 데이터 초기화
        WinManager.Instance.ResetEvaluations();

        RandomPlayerSelecter();
    }

    private void Start()
    {
        npcController.Difficulty = DifficultyManager.Instance.SelectedDifficulty;

        // 건들지마셈. 화남
        CreateMap();
        if (!IsPlayerTurn)
        {
            StartCoroutine(PrepareNPCTurnWithHints());
        }
    }

    private void CreateMap()
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                board[row, col] = PieceType.None;
            }
        }
    }

    private void RandomPlayerSelecter()
    {
        if (Random.Range(0, 100 + 1) <= 50)
        {
            IsPlayerTurn = true;

            Player = PieceType.X;
            playerPiece = cross;

            NPC = PieceType.O;
            npcPiece = circle;
        }
        else
        {
            IsPlayerTurn = false;

            Player = PieceType.O;
            playerPiece = circle;

            NPC = PieceType.X;
            npcPiece = cross;
        }

        OnPieceSelected?.Invoke(playerPiece);
    }

    private void NPCTurn()
    {
        Move move = npcController.Play(board);
        
        // NPC 수의 minimax 평가 (수를 두기 전에 평가)
        EvaluateMoveForNPC(move);
        
        board[move.row, move.col] = NPC;

        UpdateMapView(move, npcPiece.GetSprite());

        if (CheckMatch())
        {
            IsRunning = false;

            SoundManager.Instance.PlaySound(lost);
            WinManager.Instance.PlayerWin = -1;
        }
        else
        {
            if (!CheckDraw())
            {
                IsPlayerTurn = true;
            }
        }
    }

    internal void PlayerMove(Move move)
    {
        if(board[move.row, move.col] != PieceType.None)
        {
            return;
        }

        // 플레이어 수의 minimax 평가 (수를 두기 전에 평가)
        EvaluateMoveForPlayer(move);

        board[move.row, move.col] = Player;

        UpdateMapView(move, playerPiece.GetSprite());

        if (CheckMatch())
        {
            IsRunning = false;

            SoundManager.Instance.PlaySound(win);
            WinManager.Instance.PlayerWin = 1;
        }
        else
        {
            if (!CheckDraw())
            {
                lastPlayerMove = move;
                IsPlayerTurn = false;
                StartCoroutine(PrepareNPCTurnWithHints());
            }
        }
    }

    private void UpdateMapView(Move newMove, Sprite sprite)
    {
        foreach(var slot in slots)
        {
            if (slot.GetRow() == newMove.row && slot.GetColumn() == newMove.col)
            {
                var img = slot.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                    img.enabled = true;
                    img.color = new Color(1f, 1f, 1f, 1f);
                    img.preserveAspect = true;
                }
                break;
            }
        } 
    }

    private IEnumerator PrepareNPCTurnWithHints()
    {
        // 확률 뿌리고 잠깐 기다렸다가 한 방 꽂는다. 끝.
        Move chosen = new Move();
        var probs = npcController.GetMoveProbabilities(board, out chosen);

        foreach (var entry in probs)
        {
            int r = entry.Item1;
            int c = entry.Item2;
            if (lastPlayerMove.row == r && lastPlayerMove.col == c)
            {
                continue;
            }

            foreach (var slot in slots)
            {
                if (slot.GetRow() == r && slot.GetColumn() == c)
                {
                    SetHintOnSlot(slot, Mathf.RoundToInt(entry.Item3 * 100f).ToString() + "%");
                    break;
                }
            }
        }

        yield return new WaitForSeconds(1.5f);

        foreach (var slot in slots)
        {
            SetHintOnSlot(slot, string.Empty);
        }

        if (IsRunning)
        {
            NPCTurn(chosen);
        }
    }

    private void NPCTurn(Move forcedMove)
    {
        Move move = forcedMove;
        
        // NPC 수의 minimax 평가
        EvaluateMoveForNPC(move);
        
        board[move.row, move.col] = NPC;

        UpdateMapView(move, npcPiece.GetSprite());

        if (CheckMatch())
        {
            IsRunning = false;

            SoundManager.Instance.PlaySound(lost);
            WinManager.Instance.PlayerWin = -1;
        }
        else
        {
            if (!CheckDraw())
            {
                IsPlayerTurn = true;
            }
        }
    }

    private void SetHintOnSlot(Slot slot, string text)
    {
        var txt = slot.GetComponentInChildren<Text>(true);
        if (txt != null)
        {
            if (!string.IsNullOrEmpty(text))
            {
                txt.text = text;
                txt.enabled = true;
                txt.color = new Color(1f, 1f, 1f, 0.9f);
                if (!txt.gameObject.activeSelf)
                {
                    txt.gameObject.SetActive(true);
                }
            }
            else
            {
                txt.text = string.Empty;
                txt.enabled = false;
                if (txt.gameObject.activeSelf)
                {
                    txt.gameObject.SetActive(false);
                }
            }
            return;
        }

        var tmp = slot.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            if (!string.IsNullOrEmpty(text))
            {
                tmp.text = text;
                tmp.enabled = true;
                tmp.color = new Color(1f, 1f, 1f, 0.9f);
                if (!tmp.gameObject.activeSelf)
                {
                    tmp.gameObject.SetActive(true);
                }
            }
            else
            {
                tmp.text = string.Empty;
                tmp.enabled = false;
                if (tmp.gameObject.activeSelf)
                {
                    tmp.gameObject.SetActive(false);
                }
            }
        }
    }

    private bool CheckDraw()
    {
        if (IsGameEnd(board))
        {
            IsRunning = false;

            SoundManager.Instance.PlaySound(draw);
            WinManager.Instance.PlayerWin = 0;

            return true;
        }
        return false;
    }

    private bool CheckMatch()
    {
        bool match = false;


        for (int row = 0; row < board.GetLength(0); row++)
        {
            if (CheckLineMatch(board, row))
            {
                match = true;
                PlayEndAnim("L" + (row + 1));

                break;
            }
        }


        for (int col = 0; col < board.GetLength(1); col++)
        {
            if (CheckColMatch(board, col))
            {
                match = true;
                PlayEndAnim("C" + (col + 1));

                break;
            }
        }


        bool rightDiagnoal = CheckRightDiagnoalMatch(board);
        bool leftDiagnoal = CheckLeftDiagnoalMatch(board);
        if (rightDiagnoal || leftDiagnoal)
        {
            match = true;

            PlayEndAnim("D" + (rightDiagnoal ? "Right" : "Left"));
        }

        return match;
    }

    private void PlayEndAnim(string animID)
    {
        OnGameEnd?.Invoke(animID);
    }

    #region BOARD_LOGIC_PUBLIC_FUCNS

    internal bool CheckLineMatch(PieceType[,] board, int row)
    {
        return board[row, 0] != EmptyCell() &&
            board[row, 0] == board[row, 1] &&
            board[row, 1] == board[row, 2];
    }

    internal bool CheckColMatch(PieceType[,] board, int col)
    {
        return board[0, col] != EmptyCell() &&
            board[0, col] == board[1, col] &&
            board[1, col] == board[2, col];
    }

    internal bool CheckRightDiagnoalMatch(PieceType[,] board)
    {
        return board[0, 0] != EmptyCell() &&
            board[0, 0] == board[1, 1] &&
            board[1, 1] == board[2, 2];
    }

    internal bool CheckLeftDiagnoalMatch(PieceType[,] board)
    {
        return board[0, 2] != EmptyCell() &&
            board[0, 2] == board[1, 1] &&
            board[1, 1] == board[2, 0];
    }

    internal PieceType EmptyCell()
    {
        return PieceType.None;
    }

    internal bool IsGameEnd(PieceType[,] board)
    {
        foreach (var pos in board)
        {
            if (pos == EmptyCell())
            {
                return false;
            }
        }
        return true;
    }

    internal bool IsFirstMove(PieceType[,] board)
    {
        bool empty = true;

        foreach (var pos in board)
        {
            if (pos != EmptyCell())
            {
                empty = false;
            }
        }
        return empty;
    }

    internal int MovesPlayed(PieceType[,] board)
    {
        int count = 0;

        foreach (var pos in board)
        {
            if (pos != EmptyCell())
            {
                count++;
            }
        }
        return count;
    }

    internal List<(int, int)> FindEmptyPlaces(PieceType[,] board)
    {
        List<(int, int)> emptyPlaces = new List<(int, int)>();

        // 빈 공간 확인
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == PieceType.None)
                {
                    emptyPlaces.Add((i, j));
                }
            }
        }

        return emptyPlaces;
    }

    #endregion


    internal bool IsPlayerTurn
    {
        get
        {
            return isPlayerTurn;
        }

        private set
        {
            isPlayerTurn = value;

            OnPlayerTurn?.Invoke(isPlayerTurn);
        }
    }


    private void EvaluateMoveForPlayer(Move move)
    {
        var miniMax = GetComponent<MiniMax>();
        if (miniMax == null)
        {
            Debug.LogWarning("MiniMax 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // 현재 수의 평가 (수를 두기 전 상태에서 평가)
        MiniMax.MoveEvaluation currentEval;
        float currentValue = miniMax.EvaluateMoveWithData(board, move.row, move.col, false, out currentEval);

        // 최적 수 찾기
        MiniMax.MoveEvaluation bestEval;
        Move bestMove = miniMax.FindBestMoveWithEvaluation(board, false, out bestEval);

        // 평가 데이터 저장
        WinManager.Instance.PlayerEval.minimaxValues.Add(currentEval.minimaxValue);
    // store also the optimal (best) move's minimax value so we can show what the position's optimal value was
    WinManager.Instance.PlayerEval.bestMinimaxValues.Add(bestEval.minimaxValue);
        WinManager.Instance.PlayerEval.maxDepths.Add(currentEval.maxDepth);
        WinManager.Instance.PlayerEval.nodeCounts.Add(currentEval.nodeCount);

        Debug.Log($"플레이어 수 평가: ({move.row}, {move.col}) - Minimax값: {currentEval.minimaxValue}, 최적값: {bestEval.minimaxValue}, 깊이: {currentEval.maxDepth}, 노드: {currentEval.nodeCount}");

        // 최적 수 대비 효율성 계산
        if (Mathf.Abs(bestEval.minimaxValue) > 0.01f) // 0이 아닌 경우
        {
            float optimalityRatio = currentEval.minimaxValue / bestEval.minimaxValue;
            WinManager.Instance.PlayerEval.optimalityRatios.Add(optimalityRatio);
            Debug.Log($"플레이어 최적성 비율: {optimalityRatio:F2} ({currentEval.minimaxValue} / {bestEval.minimaxValue})");
        }
        else if (Mathf.Abs(currentEval.minimaxValue) < 0.01f && Mathf.Abs(bestEval.minimaxValue) < 0.01f)
        {
            // 둘 다 0에 가까우면 최적 (무승부 상태)
            WinManager.Instance.PlayerEval.optimalityRatios.Add(1.0f);
            Debug.Log($"플레이어 최적성 비율: 1.0 (둘 다 무승부 상태)");
        }
        else
        {
            // 최적이 아닌 경우
            WinManager.Instance.PlayerEval.optimalityRatios.Add(0.0f);
            Debug.Log($"플레이어 최적성 비율: 0.0 (최적이 아님)");
        }
    }

    private void EvaluateMoveForNPC(Move move)
    {
        var miniMax = GetComponent<MiniMax>();
        if (miniMax == null)
        {
            Debug.LogWarning("MiniMax 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // 현재 수의 평가 (수를 두기 전 상태에서 평가)
        MiniMax.MoveEvaluation currentEval;
        float currentValue = miniMax.EvaluateMoveWithData(board, move.row, move.col, true, out currentEval);

        // 최적 수 찾기
        MiniMax.MoveEvaluation bestEval;
        Move bestMove = miniMax.FindBestMoveWithEvaluation(board, true, out bestEval);

        // 평가 데이터 저장
        WinManager.Instance.NPCEval.minimaxValues.Add(currentEval.minimaxValue);
    // store also the optimal (best) move's minimax value for NPC
    WinManager.Instance.NPCEval.bestMinimaxValues.Add(bestEval.minimaxValue);
        WinManager.Instance.NPCEval.maxDepths.Add(currentEval.maxDepth);
        WinManager.Instance.NPCEval.nodeCounts.Add(currentEval.nodeCount);

        Debug.Log($"NPC 수 평가: ({move.row}, {move.col}) - Minimax값: {currentEval.minimaxValue}, 최적값: {bestEval.minimaxValue}, 깊이: {currentEval.maxDepth}, 노드: {currentEval.nodeCount}");

        // 최적 수 대비 효율성 계산
        if (Mathf.Abs(bestEval.minimaxValue) > 0.01f) // 0이 아닌 경우
        {
            float optimalityRatio = currentEval.minimaxValue / bestEval.minimaxValue;
            WinManager.Instance.NPCEval.optimalityRatios.Add(optimalityRatio);
            Debug.Log($"NPC 최적성 비율: {optimalityRatio:F2} ({currentEval.minimaxValue} / {bestEval.minimaxValue})");
        }
        else if (Mathf.Abs(currentEval.minimaxValue) < 0.01f && Mathf.Abs(bestEval.minimaxValue) < 0.01f)
        {
            // 둘 다 0에 가까우면 최적 (무승부 상태)
            WinManager.Instance.NPCEval.optimalityRatios.Add(1.0f);
            Debug.Log($"NPC 최적성 비율: 1.0 (둘 다 무승부 상태)");
        }
        else
        {
            // 최적이 아닌 경우
            WinManager.Instance.NPCEval.optimalityRatios.Add(0.0f);
            Debug.Log($"NPC 최적성 비율: 0.0 (최적이 아님)");
        }
    }

    private void PrintMap()
    {
        Debug.Log("-- 시작 -- ");

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                Debug.Log(board[i, j]);
            }
        }

        Debug.Log("-- End printing -- ");
    }
}
