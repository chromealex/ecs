using UnityEngine;
using ME.ECS;

public class ApplyUnitStateToView : View<Unit> {

    public Renderer cubeRenderer;
    public float lerpSpeed = 3f;

    public override void OnInitialize(Unit data) {
        
        this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);
        
        var tr = this.cubeRenderer.transform;
        tr.position = data.position;

    }

    public override void OnDeInitialize(Unit data) {
        
        Material.Destroy(this.cubeRenderer.sharedMaterial);
        
    }

    public override void ApplyState(Unit data, float deltaTime, bool immediately) {

        var tr = this.cubeRenderer.transform;
        this.cubeRenderer.sharedMaterial.color = data.color;

        if (immediately == true) {

            tr.position = data.position;
            tr.rotation = data.rotation;

        } else {

            tr.position = Vector3.Lerp(tr.position, data.position, deltaTime * this.lerpSpeed);
            tr.rotation = Quaternion.Slerp(tr.rotation, data.rotation, deltaTime * this.lerpSpeed);

        }

    }

}
