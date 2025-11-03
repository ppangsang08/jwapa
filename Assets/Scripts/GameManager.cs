using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject DisconnectPanel;
    private Animator animator;

    void Start()
    {
        if (DisconnectPanel != null)
        {
            animator = DisconnectPanel.GetComponent<Animator>();
        }
    }

    public void SettingOn()
    {
        animator.SetBool("Setting", true);
    }

    public void SettingOff()
    {
        animator.SetBool("Setting", false);
    }

    public void SetAnimatorTrigger(string parameterName)
    {
        if (animator != null)
        {
            animator.SetTrigger(parameterName);
        }
    }
    public void TutoOn()
        {
            animator.SetBool("Tuto", true);
        }
    
        public void TutoOff()
        {
            animator.SetBool("Tuto", false);
        }
}
