using ME.ECS;

namespace Warcraft.Views.UI {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerResourcesUI : MonoBehaviourView<TEntity> {

        public UnityEngine.UI.Text woodValue;
        public UnityEngine.UI.Text goldValue;
        
        public override void OnInitialize(in TEntity data) {
            
        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            var comp = Worlds<WarcraftState>.currentWorld.GetComponent<TEntity, Warcraft.Components.PlayerResourcesComponent>(data.entity);
            this.woodValue.text = comp.resources.wood.ToString();
            this.goldValue.text = comp.resources.gold.ToString();

        }
        
    }
    
}