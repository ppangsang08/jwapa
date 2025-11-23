
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
    /// Reset / clear evaluation data before a new game.
    /// Called from GameController.Awake()
    /// </summary>
    internal void ResetEvaluations()
    {
        PlayerEval.Reset();
        NPCEval.Reset();
    }

    // Simple container for collected minimax evaluation metrics
    internal class EvaluationData
    {
        public List<float> minimaxValues = new List<float>();
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

    // Ensure this manager persists across scene loads so collected evaluation data
    // remains available on the final scene.
    internal override void Init()
    {
        DontDestroyOnLoad(gameObject);
        // optional debug to confirm persistence; harmless if left in
        Debug.Log("WinManager initialized and set to DontDestroyOnLoad");
    }
}
