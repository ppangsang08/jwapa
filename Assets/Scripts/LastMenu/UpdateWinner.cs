using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateWinner : MonoBehaviour
{
    [SerializeField]
    private Text WinnerText;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text minimaxEvaluationText;

    private void Awake()
    {
        ScoreboardManager.OnDataFromPlayerPrefs += UpdateScoreboardText;
    }

    private void Start()
    {
        UpdateWinnerText();
        // 약간의 지연을 두고 실행 (씬이 완전히 로드된 후)
        StartCoroutine(DelayedUpdateMinimaxEvaluation());
    }

    private IEnumerator DelayedUpdateMinimaxEvaluation()
    {
        yield return null; // 한 프레임 대기
        yield return null; // 한 프레임 더 대기
        UpdateMinimaxEvaluation();
    }

    private void UpdateScoreboardText((int, int, int) info)
    {
        if (scoreText == null)
        {
            return;
        }
        scoreText.text = "Losses: " + info.Item1 + "\n" +
            "Draws: " + info.Item2 + "\n" +
            "Victories: " + info.Item3 + "\n";
    }

    private void UpdateWinnerText()
    {
        if (WinnerText == null)
        {
            return;
        }
        int winner = WinManager.Instance.PlayerWin;
        if (winner  == - 1)
        {
            WinnerText.text = "You Lost!";
        }
        else if(winner == 0)
        {
            WinnerText.text = "Draw!";
        }
        else if (winner == 1)
        {
            WinnerText.text = "You Win!";
        }
        else
        {
            Debug.LogError("Winner not indentified! Winner: " + winner);
        }
    }

    private void UpdateMinimaxEvaluation()
    {
        // minimaxEvaluationText 필드가 Inspector에서 연결되어 있는지 확인
        if (minimaxEvaluationText == null)
        {
            Debug.LogError("MinimaxEvaluationText가 연결되지 않았습니다! Inspector에서 Text 컴포넌트를 연결해주세요.");
            return;
        }

        // Text 컴포넌트 강제 활성화 및 확인
        minimaxEvaluationText.enabled = true;
        minimaxEvaluationText.gameObject.SetActive(true);
        
        // WinManager 인스턴스 확인
        if (WinManager.Instance == null)
        {
            string errorText = "=== Minimax 평가 ===\n\nWinManager를 찾을 수 없습니다.";
            minimaxEvaluationText.text = errorText;
            Debug.LogError("WinManager.Instance가 null입니다!");
            return;
        }

        var playerEval = WinManager.Instance.PlayerEval;
        var npcEval = WinManager.Instance.NPCEval;

        if (playerEval == null || npcEval == null)
        {
            string errorText = "=== Minimax 평가 ===\n\n평가 데이터 객체가 null입니다.";
            minimaxEvaluationText.text = errorText;
            Debug.LogError("PlayerEval 또는 NPCEval이 null입니다!");
            return;
        }

        // 디버깅: 데이터 수집 확인
        Debug.Log($"플레이어 평가 데이터 수: {playerEval.minimaxValues.Count}");
        Debug.Log($"NPC 평가 데이터 수: {npcEval.minimaxValues.Count}");

        // 텍스트 생성
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Minimax 평가 ===");
        sb.AppendLine();

        if (playerEval.minimaxValues.Count == 0 && npcEval.minimaxValues.Count == 0)
        {
            sb.AppendLine("평가 데이터 없음");
            sb.AppendLine();
            sb.AppendLine("(게임을 플레이하면 평가 데이터가 수집됩니다)");
        }
        else
        {
            // 플레이어 평가
            if (playerEval.minimaxValues.Count > 0)
            {
                sb.AppendLine("[플레이어]");
                sb.AppendLine($"평균 Minimax 값: {playerEval.GetAverageMinimaxValue():F2}");
                sb.AppendLine($"평균 트리 깊이: {playerEval.GetAverageMaxDepth():F1}");
                sb.AppendLine($"평균 노드 수: {playerEval.GetAverageNodeCount():F1}");
                sb.AppendLine($"평균 최적성 비율: {playerEval.GetAverageOptimalityRatio() * 100:F1}%");
                sb.AppendLine($"총 수: {playerEval.minimaxValues.Count}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("[플레이어]");
                sb.AppendLine("데이터 없음");
                sb.AppendLine();
            }

            // NPC 평가
            if (npcEval.minimaxValues.Count > 0)
            {
                sb.AppendLine("[NPC]");
                sb.AppendLine($"평균 Minimax 값: {npcEval.GetAverageMinimaxValue():F2}");
                sb.AppendLine($"평균 트리 깊이: {npcEval.GetAverageMaxDepth():F1}");
                sb.AppendLine($"평균 노드 수: {npcEval.GetAverageNodeCount():F1}");
                sb.AppendLine($"평균 최적성 비율: {npcEval.GetAverageOptimalityRatio() * 100:F1}%");
                sb.AppendLine($"총 수: {npcEval.minimaxValues.Count}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("[NPC]");
                sb.AppendLine("데이터 없음");
                sb.AppendLine();
            }

            // 비교 및 승자 판단
            if (playerEval.minimaxValues.Count > 0 && npcEval.minimaxValues.Count > 0)
            {
                float playerScore = playerEval.GetAverageMinimaxValue() * playerEval.GetAverageOptimalityRatio();
                float npcScore = npcEval.GetAverageMinimaxValue() * npcEval.GetAverageOptimalityRatio();

                sb.AppendLine("[효율성 비교]");
                sb.AppendLine($"플레이어 점수: {playerScore:F2}");
                sb.AppendLine($"NPC 점수: {npcScore:F2}");

                if (Mathf.Abs(playerScore - npcScore) < 0.1f)
                {
                    sb.AppendLine("→ 비슷한 효율성");
                }
                else if (playerScore > npcScore)
                {
                    sb.AppendLine("→ 플레이어가 더 효율적으로 플레이함");
                }
                else
                {
                    sb.AppendLine("→ NPC가 더 효율적으로 플레이함");
                }
            }
        }

        string finalText = sb.ToString();
        
        // 텍스트 강제 설정 - 여러 번 시도
        minimaxEvaluationText.text = finalText;
        
        // Text 컴포넌트 설정 강제 적용
        minimaxEvaluationText.color = new Color(0, 0, 0, 1); // 검정색, 완전 불투명
        minimaxEvaluationText.fontSize = 14;
        minimaxEvaluationText.alignment = TextAnchor.UpperLeft;
        minimaxEvaluationText.horizontalOverflow = HorizontalWrapMode.Overflow;
        minimaxEvaluationText.verticalOverflow = VerticalWrapMode.Overflow;
        minimaxEvaluationText.supportRichText = false; // Rich Text 비활성화로 문제 방지
        
        // 강제로 다시 설정
        minimaxEvaluationText.text = finalText;
        
        Debug.Log($"=== Minimax 평가 텍스트 업데이트 완료 ===");
        Debug.Log($"텍스트 길이: {finalText.Length}");
        Debug.Log($"Text 컴포넌트: enabled={minimaxEvaluationText.enabled}, active={minimaxEvaluationText.gameObject.activeInHierarchy}");
        Debug.Log($"최종 텍스트 내용:\n{finalText}");
    }

    private void OnDestroy()
    {
        ScoreboardManager.OnDataFromPlayerPrefs -= UpdateScoreboardText;
    }
}
