using UnityEngine;

public class CharacterBasicComponent : MonoBehaviour
{
    public UIRequestCallBubble UIRequestCall;
    public UICharacterMessageBubble UIMessageBubble;
    public Animator animator;

    public void Wave()
    {
        animator.SetBool("Wave", true);
        animator.SetLayerWeight(1, 1);
    }

    public void EndWave()
    {
        animator.SetBool("Wave", false);
        animator.SetLayerWeight(1, 0);
    }

}
