using TMPro;
using UnityEngine;

public class MinimaxDebugger : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    public void UpdateText(string text)
    {
        debugText.text = text;
    }
}
