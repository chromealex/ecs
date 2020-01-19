using UnityEngine;
using ME.ECS;

public class ApplyPointStateToView : View<Point> {

    public TMPro.TMP_Text text;
    public Renderer cubeRenderer;
    public float lerpSpeed = 3f;

    private float prevUnitsCount;

    public override void OnInitialize(Point data) {
        
        this.cubeRenderer.sharedMaterial = new Material(this.cubeRenderer.material);
        
        var tr = this.cubeRenderer.transform;
        tr.position = data.position;
        
    }

    public override void OnDeInitialize(Point data) {
        
        Material.Destroy(this.cubeRenderer.sharedMaterial);
        
    }

    public override void ApplyState(Point data, float deltaTime, bool immediately) {
        
        if (this.prevUnitsCount != data.unitsCount) {
        
            this.text.text = data.unitsCount.ToString();
            this.prevUnitsCount = data.unitsCount;

        }

        var tr = this.cubeRenderer.transform;
        this.cubeRenderer.sharedMaterial.color = data.color;

        if (immediately == true) {
            
            tr.position = data.position;
            
        } else {
            
            tr.position = Vector3.Lerp(tr.position, data.position, deltaTime * this.lerpSpeed);
            
        }
        
    }

}
