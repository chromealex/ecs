using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyUnitStateToView : MonoBehaviour {

    public Game game;
    public int entityId;
    public Renderer cubeRenderer;
    public float lerpSpeed = 3f;

    public void Start() {
        
        this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);
        
    }

    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            Unit data;
            if (this.game.world.GetEntityData(this.entityId, out data) == true) {

                var dt = Time.deltaTime;
                var tr = this.cubeRenderer.transform;
                this.cubeRenderer.sharedMaterial.color = data.color;
                tr.localPosition = Vector3.Lerp(tr.localPosition, data.position, dt * this.lerpSpeed);
                tr.localRotation = Quaternion.Lerp(tr.localRotation, data.rotation, dt * this.lerpSpeed);
                
            }

        }

    }

}
