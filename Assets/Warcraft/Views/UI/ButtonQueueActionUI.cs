using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonQueueActionUI : MonoBehaviour {

    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Slider progress;
    public UnityEngine.UI.Text progressValue;
    public UnityEngine.UI.Image icon;
    
    public void SetInfo(Sprite icon, float progress, bool isActive) {

        this.icon.sprite = icon;
        this.progress.value = progress;
        this.progressValue.text = Mathf.FloorToInt(progress * 100f).ToString() + "%";
        this.progress.gameObject.SetActive(isActive);

    }

}
