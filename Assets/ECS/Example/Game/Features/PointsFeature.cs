using UnityEngine;
using ME.ECS;

namespace ME.Example.Game.Features {

    using ME.Example.Game.Systems;
    using ME.Example.Game.Entities;

    public class PointsFeature : Feature<State, PointsFeatureInitParameters> {

        protected override void OnConstruct(ref PointsFeatureInitParameters parameters) {
            
            var p1 = this.world.AddEntity(new Point() { position = this.world.GetState().worldPosition + parameters.p1Position, scale = Vector3.one });
            var p2 = this.world.AddEntity(new Point() { position = this.world.GetState().worldPosition + parameters.p2Position, scale = Vector3.one });
            
            this.world.InstantiateView<Point>(parameters.pointViewSourceId, p1);
            this.world.InstantiateView<Point>(parameters.pointViewSourceId, p2);

            parameters.p1 = p1;
            parameters.p2 = p2;
            
            this.AddSystem<PointsSystem>();
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}