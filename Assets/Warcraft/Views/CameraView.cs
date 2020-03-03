using ME.ECS;
using UnityEngine;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.CameraEntity;
    
    public class CameraView : MonoBehaviourView<TEntity> {

        public Vector2 cameraSize;
        public Camera cameraView;
        
        protected Transform tr;
        
        public override void OnInitialize(in TEntity data) {

            this.tr = this.transform;
            Worlds<WarcraftState>.currentWorld.GetFeature<Warcraft.Features.CameraFeature>().SetCamera(this.cameraView);

        }
        
        public override void OnDeInitialize(in TEntity data) {

            this.tr = null;

        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
            if (immediately == true) {

                this.tr.position = data.position;

            } else {
                
                this.tr.position = Vector3.Lerp(this.tr.position, data.position, 10f * deltaTime);
                
            }

        }
        
    }
    
}