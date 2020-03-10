using ME.ECS;

namespace Warcraft.Views.UI {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.PlayerEntity;
    
    public class PlayerResourcesUI : MonoBehaviourView<TEntity> {

        public UnityEngine.UI.Text woodValue;
        public UnityEngine.UI.Text goldValue;

        private int lastWoodValue = -1;
        private int lastGoldValue = -1;
        
        public override void OnInitialize(in TEntity data) {
            
        }
        
        public override void OnDeInitialize(in TEntity data) {
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            var comp = Worlds<WarcraftState>.currentWorld.GetComponent<TEntity, Warcraft.Components.PlayerResourcesComponent>(data.entity);
            if (this.lastWoodValue != comp.resources.wood) this.woodValue.text = comp.resources.wood.ToString();
            if (this.lastGoldValue != comp.resources.gold) this.goldValue.text = comp.resources.gold.ToString();
            this.lastWoodValue = comp.resources.wood;
            this.lastGoldValue = comp.resources.gold;

        }
        
    }
    
}