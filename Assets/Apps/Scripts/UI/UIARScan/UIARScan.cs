using UnityEngine;
using TMPro;

public class UIARScan : PopupEntity<UIARScan.Entity>
{
    public class Entity
    {
        public string scanText;
        public ButtonEntity btnBack;
        public ButtonEntity btnSpawn;
    }

    public Animator animator;
    public TextMeshProUGUI scanText;
    public UIButtonComponent btnBack;
    public UIButtonComponent btnSpawn;

    public string startState = "Start";

    protected override void OnSetEntity()
    {
        if (animator != null)
            animator.Play(startState);

        scanText.SetText(entity.scanText);
        btnBack.SetEntity(entity.btnBack);
        btnSpawn.SetEntity(entity.btnSpawn);
    }
}
