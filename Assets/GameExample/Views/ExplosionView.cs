using ME.ECS;
using UnityEngine;

namespace ME.GameExample.Game.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = ME.GameExample.Game.Entities.Explosion;
    
    public class ExplosionView : MonoBehaviourView<TEntity> {

        public float lerpTime = 3f;
        
        public override void OnInitialize(in TEntity data) {
            
        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
            var tr = this.transform;
            
            if (immediately == true) {

                tr.position = data.position;

            } else {

                tr.position = Vector3.Lerp(tr.position, data.position, deltaTime * this.lerpTime);

            }

        }
        
    }
    
}