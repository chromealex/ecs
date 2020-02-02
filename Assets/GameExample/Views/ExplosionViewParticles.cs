using ME.ECS;
using UnityEngine;

namespace GameExample.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = ME.GameExample.Game.Entities.Explosion;
    
    public class ExplosionViewParticles : ParticleViewSource<ApplyExplosionViewParticlesStateParticle> { }
    
    [System.Serializable]
    public class ApplyExplosionViewParticlesStateParticle : ParticleView<TEntity> {

        public float lerpTime = 3f;
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
            ref var rootData = ref this.GetRootData();
            
            if (immediately == true) {

                rootData.position = data.position;

            } else {

                rootData.position = Vector3.Lerp(rootData.position, data.position, deltaTime * this.lerpTime);

            }

            this.SetRootData(ref rootData);
            
        }
        
    }
    
}