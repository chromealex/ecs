using UnityEngine;
using ME.ECS;

public class ApplyUnitStateToParticleView : ParticleViewSource<ApplyUnitStateParticle> {

}

[System.Serializable]
public class ApplyUnitStateParticle : ParticleView<Unit> {

    public float lerpSpeed = 3f;
    
    public override void ApplyState(in Unit data, float deltaTime, bool immediately) {
        
        this.particleData.startColor = data.color;

        if (immediately == true) {

            this.particleData.position = data.position;
            this.particleData.rotation3D = data.rotation.eulerAngles;
            
        } else {

            this.particleData.position = Vector3.Lerp(this.particleData.position, data.position, deltaTime * this.lerpSpeed);
            this.particleData.rotation3D = Quaternion.Slerp(Quaternion.Euler(this.particleData.rotation3D), data.rotation, deltaTime * this.lerpSpeed).eulerAngles;
            
        }
        
    }

}
