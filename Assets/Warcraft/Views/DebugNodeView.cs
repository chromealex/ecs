using ME.ECS;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.DebugEntity;
    
    public class DebugNodeView : MonoBehaviourView<TEntity> {

        public UnityEngine.TextMesh text;
        
        public override void OnInitialize(in TEntity data) {
            
        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            this.transform.position = data.worldPos;
            this.text.text = data.data;

        }
        
    }
    
}