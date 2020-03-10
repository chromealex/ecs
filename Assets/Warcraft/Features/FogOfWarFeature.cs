using ME.ECS;
using UnityEngine;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class FogOfWarFeature : Feature<TState> {

        private Warcraft.Systems.FogOfWarSystem fowSystem;
        private MapFeature mapFeature;

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<Warcraft.Systems.FogOfWarSystem>();
            this.fowSystem = this.world.GetSystem<Warcraft.Systems.FogOfWarSystem>();
            this.mapFeature = this.world.GetFeature<MapFeature>();

        }

        protected override void OnDeconstruct() {
            
        }

        public bool IsVisibleAny(Entity playerEntity, Vector2 worldPosition, Vector2Int size) {

            return this.fowSystem.IsVisibleAny(playerEntity, this.mapFeature.GetMapPositionFromWorld(this.mapFeature.GetWorldLeftBottomPosition(worldPosition, size)), size);

        }

        public bool IsRevealed(Entity playerEntity, Vector2 worldPosition, Vector2Int size) {

            return this.fowSystem.IsRevealedAll(playerEntity, this.mapFeature.GetMapPositionFromWorld(this.mapFeature.GetWorldLeftBottomPosition(worldPosition, size)), size);

        }

    }

}