using ME.ECS;
using UnityEngine;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.SelectionEntity;
    
    public class SelectionView : MonoBehaviourView<TEntity> {

        public SpriteRenderer spriteRenderer;
        private Transform tr;
        
        public override void OnInitialize(in TEntity data) {

            this.tr = this.transform;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            this.tr = null;

        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            if (Worlds<WarcraftState>.currentWorld.GetEntityData(data.unitEntity, out Warcraft.Entities.UnitEntity unitData) == true) {

                var mapFeature = Worlds<WarcraftState>.currentWorld.GetFeature<Warcraft.Features.MapFeature>();

                var isHidden = Worlds<WarcraftState>.currentWorld.HasComponent<Warcraft.Entities.UnitEntity, Warcraft.Components.UnitHiddenView>(data.unitEntity);
                
                this.tr.position = unitData.position.XY();
                this.tr.localScale = (isHidden == true ? Vector3.zero : Vector3.one);
                this.spriteRenderer.size = mapFeature.GetWorldPositionFromMap(unitData.size);

            }

        }
        
    }
    
}