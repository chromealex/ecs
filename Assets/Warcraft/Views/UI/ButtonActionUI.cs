using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActionUI : MonoBehaviour {

    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Text wood;
    public UnityEngine.UI.Text gold;
    public UnityEngine.UI.Image icon;
    
    public void SetInfo(ResourcesStorage resource, Sprite icon) {

        this.wood.text = resource.wood.ToString();
        this.gold.text = resource.gold.ToString();
        this.icon.sprite = icon;

    }

}
