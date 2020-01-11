using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyPointStateToView : MonoBehaviour {

    public Game game;
    public int entityId;
    public TMPro.TMP_Text text;
    public Renderer cubeRenderer;

    private float prevUnitsCount;

    public void Start() {
        
        this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);
        
    }

    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            Point data;
            if (this.game.world.GetEntityData(this.entityId, out data) == true) {

                if (this.prevUnitsCount != data.unitsCount) {
                
                    this.text.text = data.unitsCount.ToString();
                    this.prevUnitsCount = data.unitsCount;

                }

                this.cubeRenderer.sharedMaterial.color = data.color;
                this.cubeRenderer.transform.localPosition = data.position;

            }

        }

    }

}
