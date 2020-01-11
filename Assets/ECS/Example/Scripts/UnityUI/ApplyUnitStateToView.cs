using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyUnitStateToView : MonoBehaviour {

    public Game game;
    public int entityId;
    public Renderer cubeRenderer;

    public void Start() {
        
        this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);
        
    }

    public void LateUpdate() {

        if (this.game != null && this.game.world != null) {

            Unit data;
            if (this.game.world.GetEntityData(this.entityId, out data) == true) {

                var tr = this.cubeRenderer.transform;
                tr.localPosition = data.position;
                tr.localRotation = data.rotation;
                
            }

        }

    }

}
