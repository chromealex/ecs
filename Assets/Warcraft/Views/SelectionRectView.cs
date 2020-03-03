using ME.ECS;
using UnityEngine;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.SelectionRectEntity;
    
    public class SelectionRectView : MonoBehaviourView<TEntity> {

        public SpriteRenderer spriteRenderer;
        private Transform tr;
        
        public override void OnInitialize(in TEntity data) {

            this.tr = this.transform;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            this.tr = null;

        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            var comp = Worlds<WarcraftState>.currentWorld.GetComponent<TEntity, Warcraft.Components.SelectionRectComponent>(data.entity);
            
            this.tr.position = comp.worldPosition.XY();
            this.spriteRenderer.size = comp.size;

        }
        
    }
    
}