using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsSelectionFeature : Feature<TState> {

        private ViewId selectionViewSourceId;
        private ViewId selectionViewRectSourceId;
        
        protected override void OnConstruct(ref ConstructParameters parameters) {

            var prefab = UnityEngine.Resources.Load<Warcraft.Views.SelectionView>("Selection");
            this.selectionViewSourceId = this.world.RegisterViewSource<SelectionEntity>(prefab);
            
            var prefabRect = UnityEngine.Resources.Load<Warcraft.Views.SelectionRectView>("SelectionRect");
            this.selectionViewRectSourceId = this.world.RegisterViewSource<SelectionRectEntity>(prefabRect);
            
            this.AddSystem<Warcraft.Systems.UnitsSelectionSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

        public void ClearSelection(Entity unitEntity) {
        
            this.world.RemoveComponents<UnitEntity, UnitSelectedComponent>(unitEntity);
            
        }

        public void AddSelection(Warcraft.Entities.UnitEntity unitEntity, int allCount = 0) {

            var selectionEntity = this.world.AddEntity(new SelectionEntity() { unitEntity = unitEntity.entity });
            this.world.InstantiateView<Warcraft.Entities.SelectionEntity>(this.selectionViewSourceId, selectionEntity);

            this.world.AddComponent<UnitEntity, UnitSelectedComponent>(unitEntity.entity);
            
            if (allCount <= 1) {

                var playerFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();

                var comp = this.world.AddComponent<PlayerEntity, PlayerSelectedUnitComponent>(playerFeature.GetActivePlayer());
                comp.unit = unitEntity.entity;

            }
            
        }

        public Entity AddSelectionRect(UnityEngine.Vector2 worldPosition) {

            var selectionEntity = this.world.AddEntity(new SelectionRectEntity());
            var comp = this.world.AddComponent<SelectionRectEntity, SelectionRectComponent>(selectionEntity);
            comp.worldPosition = worldPosition;
            comp.size = UnityEngine.Vector2.zero;
            
            this.world.InstantiateView<Warcraft.Entities.SelectionRectEntity>(this.selectionViewRectSourceId, selectionEntity);
            return selectionEntity;

        }

    }

}