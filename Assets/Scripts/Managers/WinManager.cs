
using System.Collections.Generic;
using UnityEngine;

public class WinManager : GenericSingleton<WinManager>
{
    // -1 == Player Lost, 0 == Draw, 1 == Player Win
    static private int playerWin;

    internal int PlayerWin
    {
        get
        {
            return playerWin;
        }

        set
        {
            playerWin = value;

            // proceed to next scene (keep existing behavior)
            if (LoadSceneManager.Instance != null)
            {
                LoadSceneManager.Instance.LoadNextScene();
            }
        }
    }

    // Evaluation data collected during play
    internal EvaluationData PlayerEval { get; private set; } = new EvaluationData();
    internal EvaluationData NPCEval { get; private set; } = new EvaluationData();

    /// <summary>
    /// 새 게임 전 평가 데이터 초기화함 (GameController.Awake에서 호출)
    /// </summary>
    internal void ResetEvaluations()
    {
        PlayerEval.Reset();
        NPCEval.Reset();
    }

    // 평가 데이터 담는 컨테이너
    internal class EvaluationData
    {
        public List<float> minimaxValues = new List<float>();
    // 최적 수의 minimax 값도 보관함
    public List<float> bestMinimaxValues = new List<float>();
        public List<int> maxDepths = new List<int>();
        public List<int> nodeCounts = new List<int>();
        public List<float> optimalityRatios = new List<float>();

        public void Reset()
        {
            minimaxValues.Clear();
            maxDepths.Clear();
            nodeCounts.Clear();
            optimalityRatios.Clear();
        }

        public float GetAverageMinimaxValue()
        {
            if (minimaxValues == null || minimaxValues.Count == 0) return 0f;
            float sum = 0f;
            foreach (var v in minimaxValues) sum += v;
            return sum / minimaxValues.Count;
        }

        public float GetAverageBestMinimaxValue()
        {
            if (bestMinimaxValues == null || bestMinimaxValues.Count == 0) return 0f;
            float sum = 0f;
            foreach (var v in bestMinimaxValues) sum += v;
            return sum / bestMinimaxValues.Count;
        }

        public float GetAverageMaxDepth()
        {
            if (maxDepths == null || maxDepths.Count == 0) return 0f;
            float sum = 0f;
            foreach (var v in maxDepths) sum += v;
            return sum / maxDepths.Count;
        }

        public float GetAverageNodeCount()
        {
            if (nodeCounts == null || nodeCounts.Count == 0) return 0f;
            float sum = 0f;
            foreach (var v in nodeCounts) sum += v;
            return sum / nodeCounts.Count;
        }

        public float GetAverageOptimalityRatio()
        {
            if (optimalityRatios == null || optimalityRatios.Count == 0) return 0f;
            float sum = 0f;
            foreach (var v in optimalityRatios) sum += v;
            return sum / optimalityRatios.Count;
        }
    }

    // 씬 전환해도 이 매니저는 유지되어 평가 데이터가 남아있게 함
    internal override void Init()
    {
        DontDestroyOnLoad(gameObject);
    // (디버그) 유지 확인용 로그
        Debug.Log("WinManager initialized and set to DontDestroyOnLoad");
    }
}
