using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingMainMenu : PopupEntity<UILoadingMainMenu.Entity>
{
    [SerializeField]
    public class Entity
    {
        public System.Action slider;
    }

    public Slider slider;
    protected override void OnSetEntity()
    {
        
    }

    private void Start()
    {
        StartCoroutine(LoadingAsync());
    }
    
    private IEnumerator LoadingAsync()
    {
        float progressValue = 0f;
        while (progressValue < 1f)
        {
            progressValue += Time.deltaTime / 3; // Simulate loading time
            slider.value = Mathf.Clamp01(progressValue);
            yield return null;
        }
    }
}
