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
            
            {
                var entity = this.world.AddEntity(new PlayerZone() { position = this.world.GetState().worldPosition + parameters.zone1Position, scale = parameters.zone1Scale, color = parameters.playersColor[Game.Repeat(1, parameters.playersColor.Length)] });
                this.world.InstantiateView<PlayerZone>(parameters.playerZoneViewSourceId, entity);
                
                entity = this.world.AddEntity(new PlayerZone() { position = this.world.GetState().worldPosition + parameters.zone2Position, scale = parameters.zone2Scale, color = parameters.playersColor[Game.Repeat(2, parameters.playersColor.Length)] });
                this.world.InstantiateView<PlayerZone>(parameters.playerZoneViewSourceId, entity);
                
                entity = this.world.AddEntity(new PlayerZone() { position = this.world.GetState().worldPosition + parameters.zone3Position, scale = parameters.zone3Scale, color = parameters.playersColor[Game.Repeat(3, parameters.playersColor.Length)] });
                this.world.InstantiateView<PlayerZone>(parameters.playerZoneViewSourceId, entity);
                
                entity = this.world.AddEntity(new PlayerZone() { position = this.world.GetState().worldPosition + parameters.zone4Position, scale = parameters.zone4Scale, color = parameters.playersColor[Game.Repeat(4, parameters.playersColor.Length)] });
                this.world.InstantiateView<PlayerZone>(parameters.playerZoneViewSourceId, entity);
            }
            
            this.AddSystem<PointsSystem>();
            this.AddSystem<PointsColorSystem>();
            this.AddSystem<PlayerZonesSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}