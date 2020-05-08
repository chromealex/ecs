using ME.ECS;

namespace Prototype.Features.Units.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Entities.Unit;
    
    public class BulletView : MonoBehaviourView<TEntity> {
        
        private TEntity data;

        public override void OnInitialize(in TEntity data) {
            
        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            this.data = data;

            this.transform.position = data.entity.GetPosition();
            this.transform.rotation = data.entity.GetRotation();

        }
        
    }
    
}