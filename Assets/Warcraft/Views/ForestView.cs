using ME.ECS;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using TEntity = Warcraft.Entities.ForestEntity;
    
    public class ForestView : NoView<TEntity> {

        private TEntity data;
        
        public override void OnInitialize(in TEntity data) {

            this.data = data;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            var mapFeature = Worlds<WarcraftState>.currentWorld.GetFeature<Warcraft.Features.MapFeature>();
            mapFeature.CutDownTree(this.data.position);

            var pathfindingFeature = Worlds<WarcraftState>.currentWorld.GetFeature<Warcraft.Features.PathfindingFeature>();
            pathfindingFeature.SetWalkability(this.data.position, true);
            
        }
        
        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
        }

    }
    
}