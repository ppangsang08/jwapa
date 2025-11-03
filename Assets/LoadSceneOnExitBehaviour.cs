using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnExitBehaviour : StateMachineBehaviour
{
    // 인스펙터에서 상태별로 씬 이름 지정하려면 SerializeField가 안 되므로,
    // 코드로 하드코딩하거나 퍼블릭 static 이용, 또는 Animator parameter로 구분.
    public string sceneName = "InitialMenu";

    // OnStateExit는 상태가 끝나고 다른 상태로 전환될 때 호출됨
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // normalizedTime >=1 인 경우도 있지만 OnStateExit자체가 끝났을 때 호출되므로 바로 로드해도 괜찮음
        SceneManager.LoadScene(sceneName);
    }
}
