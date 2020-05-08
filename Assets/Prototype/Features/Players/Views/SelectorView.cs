using ME.ECS;

namespace Prototype.Features.Players.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Prototype.Entities.Unit;
    
    public class SelectorView : MonoBehaviourView<TEntity> {
        
        public override void OnInitialize(in TEntity data) {
            
        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            this.transform.position = data.entity.GetPosition();
            this.transform.rotation = data.entity.GetRotation();

        }
        
    }
    
}