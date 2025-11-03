using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 이 네임스페이스를 추가합니다.

public class SceneTransitionManager : MonoBehaviour
{
    // 애니메이션 이벤트에서 호출할 함수
    public void LoadNextScene()
    {
        // 다음 씬의 이름을 입력합니다. 
        // 프로젝트의 Build Settings에서 씬이 추가되어 있어야 합니다.
        SceneManager.LoadScene("InitialMenu"); 
    }
}
