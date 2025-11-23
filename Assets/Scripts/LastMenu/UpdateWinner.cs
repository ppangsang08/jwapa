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

    [Header("Leaderboard")]
    [SerializeField]
    private InputField playerNameInput;
    [SerializeField]
    private Button saveScoreButton;
    [SerializeField]
    private Transform leaderboardParent;
    [SerializeField]
    private GameObject leaderboardEntryPrefab; // 텍스트 포함 프리팹(선택)
    [SerializeField]
    private int leaderboardMax = 10;

    private float lastSessionScore = 0f;
    private int lastSavedIndex = -1;

    private void Awake()
    {
        ScoreboardManager.OnDataFromPlayerPrefs += UpdateScoreboardText;
    // 저장 버튼 리스너 항상 연결
        if (saveScoreButton != null)
        {
            saveScoreButton.onClick.RemoveListener(SaveSessionScore);
            saveScoreButton.onClick.AddListener(SaveSessionScore);
        }
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
                    sb.AppendLine($"평균 Minimax 값 (선택한 수): {playerEval.GetAverageMinimaxValue():F2}");
                    // 최적값도 표시(포지션 값 확인용)
                    sb.AppendLine($"평균 최적 Minimax 값: {playerEval.GetAverageBestMinimaxValue():F2}");
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
                    sb.AppendLine($"평균 Minimax 값 (선택한 수): {npcEval.GetAverageMinimaxValue():F2}");
                    sb.AppendLine($"평균 최적 Minimax 값: {npcEval.GetAverageBestMinimaxValue():F2}");
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
                    sb.AppendLine("플레이어가 더 효율적으로 플레이함");
                }
                else
                {
                    sb.AppendLine("NPC가 더 효율적으로 플레이함");
                }
            }
        }

    // 세션 점수 계산해서 패널에 추가
        float sessionScore = ComputeSessionScore(playerEval, npcEval);
        lastSessionScore = sessionScore;
        sb.AppendLine();
        sb.AppendLine($"[세션 점수] {sessionScore:F2} / 10");

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

        // Configure save button: keep it clickable; disable only visually if score is zero
        if (saveScoreButton != null)
        {
            // keep listener (added in Awake)
            saveScoreButton.interactable = true; // always allow click so user can save name even if score==0
        }

        // Refresh leaderboard UI
        RefreshLeaderboardUI();
    }

    private void SaveSessionScore()
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardManager.Instance is null. Make sure LeaderboardManager exists in the project.");
            return;
        }

        string name = "Player";
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            name = playerNameInput.text;
        }

        // Add entry and capture its index so we can scroll to it
        lastSavedIndex = LeaderboardManager.Instance.AddEntry(name, lastSessionScore);
        Debug.Log($"Saved leaderboard entry: {name} - {lastSessionScore:F2} (index {lastSavedIndex})");
        RefreshLeaderboardUI();
    }

    private void RefreshLeaderboardUI()
    {
        if (leaderboardParent == null) return;

    // 레이아웃 컴포넌트 없으면 추가(겹침 방지)
        var vlg = leaderboardParent.GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
        {
            vlg = leaderboardParent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 6f;
            vlg.childAlignment = TextAnchor.UpperLeft;
        }

        var csf = leaderboardParent.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = leaderboardParent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

    // 기존 항목(자식) 전부 제거
        for (int i = leaderboardParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(leaderboardParent.GetChild(i).gameObject);
        }

        if (LeaderboardManager.Instance == null) return;

        var entries = LeaderboardManager.Instance.GetEntries();
        int count = Mathf.Min(entries.Count, leaderboardMax);

        for (int i = 0; i < count; i++)
        {
            var e = entries[i];
            GameObject go = null;
            if (leaderboardEntryPrefab != null)
            {
                go = Instantiate(leaderboardEntryPrefab, leaderboardParent);
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = $"{i + 1}. {e.name} - {e.score:F2} ({e.date})";
                }
            }
            else
            {
                // 간단한 Text 오브젝트 생성
                var obj = new GameObject($"Leader_{i + 1}");
                obj.transform.SetParent(leaderboardParent, false);
                var txt = obj.AddComponent<Text>();
                txt.font = minimaxEvaluationText != null ? minimaxEvaluationText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = 14;
                txt.color = Color.black;
                txt.alignment = TextAnchor.MiddleLeft;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                txt.horizontalOverflow = HorizontalWrapMode.Wrap;
                txt.text = $"{i + 1}. {e.name} - {e.score:F2}\n({e.date})";
                go = obj;
            }

            // 항목 높이 위해 LayoutElement 추가
            if (go != null)
            {
                var le = go.GetComponent<LayoutElement>();
                if (le == null) le = go.AddComponent<LayoutElement>();
                le.preferredHeight = 60f;
                le.flexibleWidth = 1f;
            }
        }

    // 레이아웃 강제 갱신
        var rt = leaderboardParent as RectTransform;
        if (rt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

    // 자동 스크롤(저장된 항목 보이기)
        var scrollRect = leaderboardParent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            int total = count;
            if (total <= 0)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
            else if (lastSavedIndex >= 0 && lastSavedIndex < total)
            {
                // 인덱스를 스크롤 위치로 변환
                float pos = 1f;
                if (total > 1)
                {
                    pos = 1f - (lastSavedIndex / (float)(total - 1));
                }
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(pos);
            }
            else
            {
                // 기본은 맨 위
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

    // 스크롤 후 저장 인덱스 리셋
        lastSavedIndex = -1;
    }

    private void OnDestroy()
    {
        ScoreboardManager.OnDataFromPlayerPrefs -= UpdateScoreboardText;
    }
    private float ComputeSessionScore(WinManager.EvaluationData playerEval, WinManager.EvaluationData npcEval)
    {
        if (playerEval == null)
        {
            return 1.0f;
        }

        int winner = WinManager.Instance != null ? WinManager.Instance.PlayerWin : 0;

        float opt = 0.5f;
        if (playerEval.minimaxValues.Count > 0)
        {
            opt = Mathf.Clamp01(playerEval.GetAverageOptimalityRatio());
        }

        float avgDepth = playerEval.GetAverageMaxDepth();
        float depthFactor = (float)System.Math.Tanh(avgDepth / 6f);

    // 노드 수는 로그 스케일로 보정
        float avgNodes = playerEval.GetAverageNodeCount();
        float nodeFactor = Mathf.Clamp01(Mathf.Log10(avgNodes + 10f) / 3.0f);

    // 상대적 이점 보정 항목
        float rel = 0f;
        if (npcEval != null && npcEval.minimaxValues.Count > 0 && playerEval.minimaxValues.Count > 0)
        {
            float p = playerEval.GetAverageMinimaxValue();
            float n = npcEval.GetAverageMinimaxValue();
            rel = (float)System.Math.Tanh((p - n) / 6f);
        }

    // 성능 지표들 결합(0..1)
        float performance = opt * 0.5f + depthFactor * 0.25f + nodeFactor * 0.18f + rel * 0.02f;
        performance = Mathf.Clamp01(performance);

    // 결과별(무승부/승/패)로 점수 범위 조정
        float score;
        if (winner == 0)
        {
            // 무승부는 관대하게 7~10
            score = 7.0f + performance * 3.0f;
        }
        else if (winner == 1)
        {
            // 이기면 7.5~10
            score = 7.5f + performance * 2.5f;
        }
        else
        {
            // 지면 최소 1점, 최대 5점
            score = 1.0f + performance * 4.0f;
        }

    // 소수 둘째자리로 반올림하고 0..10 범위로 고정
        score = (float)System.Math.Round(score * 100f) / 100f;
        score = Mathf.Clamp(score, 0f, 10f);

        return score;
    }
}
