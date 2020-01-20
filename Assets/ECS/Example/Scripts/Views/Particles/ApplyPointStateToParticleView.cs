using UnityEngine;
using ME.ECS;

public class ApplyPointStateToParticleView : ParticleViewSource<ApplyPointStateParticle> {

}

[System.Serializable]
public class ApplyPointStateParticle : ParticleView<Point> {

    public float lerpSpeed = 3f;

    public override void ApplyState(in Point data, float deltaTime, bool immediately) {
        
        this.particleData.startColor = data.color;

        if (immediately == true) {

            this.particleData.position = data.position;
            this.particleData.startSize3D = data.scale;
            
        } else {

            this.particleData.position = Vector3.Lerp(this.particleData.position, data.position, deltaTime * this.lerpSpeed);
            this.particleData.startSize3D = data.scale;
            
        }
        
    }

}
