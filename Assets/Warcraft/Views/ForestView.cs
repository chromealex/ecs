using ME.ECS;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.UnitEntity;
    
    public class ForestView : NoView<TEntity> {

        private TEntity data;
        
        public override void OnInitialize(in TEntity data) {

            this.data = data;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            
            var mapFeature = world.GetFeature<Warcraft.Features.MapFeature>();
            mapFeature.CutDownTree(mapFeature.GetMapPositionFromWorld(this.data.position));

            var pathfindingFeature = world.GetFeature<Warcraft.Features.PathfindingFeature>();
            pathfindingFeature.SetWalkability(mapFeature.GetMapPositionFromWorld(this.data.position), true);
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
        }

    }
    
}