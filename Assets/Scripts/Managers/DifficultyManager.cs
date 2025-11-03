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
        return new List<string> {"ㅈㄴ쉬움", "아마도 보통", "좌의 한수(불가능)"};
    }

    internal void SetSelectedDifficulty(int t)
    {
        SelectedDifficulty = (Difficulties)t;
    }

    internal string GetSelectedDifficultyName()
    {
        switch (SelectedDifficulty)
        {
            case Difficulties.Easy: return "ㅈㄴ쉬움";
            case Difficulties.Medium: return "아마도 보통";
            case Difficulties.Hard: return "좌의 한수(불가능)";
            default: return "없음";
        }
    }
}
