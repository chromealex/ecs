using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyPointStateToView : MonoBehaviour {

    public Game game;
    public int entityId;
    public TMPro.TMP_Text text;
    public Renderer cubeRenderer;
    public float lerpSpeed = 3f;

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

                var dt = Time.deltaTime;
                var tr = this.cubeRenderer.transform;
                this.cubeRenderer.sharedMaterial.color = data.color;
                tr.localPosition = Vector3.Lerp(tr.localPosition, data.position, dt * this.lerpSpeed);

            }

        }

    }

}
