using ME.ECS;

namespace Prototype.Features.Players.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Player;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class PlayerInitializationSystem : ISystemFilter<TState> {

        private PlayersFeature playersFeature;
        private MapFeature mapFeature;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            this.playersFeature = this.world.GetFeature<PlayersFeature>();
            this.mapFeature = this.world.GetFeature<MapFeature>();

        }
        
        void ISystemBase.OnDeconstruct() {}
        
        bool ISystemFilter<TState>.jobs => false;
        int ISystemFilter<TState>.jobsBatchCount => 64;
        IFilter<TState> ISystemFilter<TState>.filter { get; set; }
        IFilter<TState> ISystemFilter<TState>.CreateFilter() {
            
            return Filter<TState, TEntity>.Create("Filter-PlayerInitializationSystem").WithStructComponent<InitializePlayer>().Push();
            
        }

        void ISystemFilter<TState>.AdvanceTick(Entity entity, TState state, float deltaTime) {

            {

                ref var player = ref this.world.GetEntityDataRef<Player>(entity);
                for (int i = 0; i < this.mapFeature.resourcesData.mapView.startPoints.Length; ++i) {

                    var playerInfo = this.mapFeature.resourcesData.mapView.startPoints[i];
                    if (playerInfo.playerIndex == player.actorId - 1) {

                        for (int j = 0; j < playerInfo.onSpawnUnits.Length; ++j) {

                            var nextId = entity.GetData<SquadsId>();
                            ++nextId.value;
                            entity.SetData(nextId);

                            var unitInfo = playerInfo.onSpawnUnits[j];
                            var squad = this.world.AddEntity(new Unit());
                            squad.SetData(new SquadsId() { value = nextId.value });
                            squad.SetData(new Prototype.Features.Units.Components.IsSquad());
                            squad.SetData(new Prototype.Features.Units.Components.InitializeSquad() {
                                owner = entity,
                                position = unitInfo.position.position,
                                data = unitInfo.unitData
                            });

                        }

                    }

                }

            }

            entity.RemoveData<InitializePlayer>();

        }

    }
    
}