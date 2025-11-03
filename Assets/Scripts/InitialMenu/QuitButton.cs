using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    public void Button_Quit()
    {
        SceneManager.LoadScene("Start");
    }
}
