using System;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : GenericSingleton<DifficultyManager>
{
    internal enum Difficulties
    {
        None,
        Easy,
        Medium,
        Hard,
    }

    static private Difficulties selectedDifficulty = Difficulties.Easy;
    internal Difficulties SelectedDifficulty
    {
        get { return selectedDifficulty; }
        private set { selectedDifficulty = value; }
    }

    internal List<string> GetDifficulties()
    {
        return new List<string> { "처 골라라", "ㅈㄴ쉬움", "아마도 보통", "좌호빈은 못함" };
    }

    internal void SetSelectedDifficulty(int t)
    {
        SelectedDifficulty = (Difficulties)t;
    }

    internal string GetSelectedDifficultyName()
    {
        switch (SelectedDifficulty)
        {
            case Difficulties.None: return "처 골라라";
            case Difficulties.Easy: return "ㅈㄴ쉬움";
            case Difficulties.Medium: return "아마도 보통";
            case Difficulties.Hard: return "좌호빈은 못함";
            default: return "없음";
        }
    }
}
