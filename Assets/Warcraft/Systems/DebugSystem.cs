using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Views;
    using Warcraft.Components;
    using Warcraft.Components.CharacterStates;
    
    public class DebugSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {

        private IFilter<TState, UnitEntity> allCharacterUnits;
        private ViewId debugViewSourceId;
        
        public IWorld<TState> world { get; set; }

        private Warcraft.Features.MapFeature mapFeature;

        void ISystemBase.OnConstruct() {

            Filter<TState, UnitEntity>.Create(ref this.allCharacterUnits, "allUnits").WithComponent<CharacterComponent>().Push();

            this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            var debugView = UnityEngine.Resources.Load<DebugNodeView>("DebugNodeView");
            this.debugViewSourceId = this.world.RegisterViewSource<DebugEntity>(debugView);
            
            for (int x = 0; x < this.mapFeature.mapInfo.mapSize.x; ++x) {
            
                for (int y = 0; y < this.mapFeature.mapInfo.mapSize.y; ++y) {

                    var pos = new UnityEngine.Vector2Int(x, y);
                    this.world.AddEntity(new DebugEntity() { pos = pos, worldPos = this.mapFeature.GetWorldPositionFromMap(pos) });
                    
                }
                
            }

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {

            var graph = AstarPath.active.graphs[0] as Pathfinding.GridGraph;
            foreach (var index in state.debug) {

                ref var debug = ref state.debug[index];
                var node = graph.GetNode(debug.pos.x, debug.pos.y);
                var data = (node.userData == Entity.Empty ? string.Empty : node.userData.id.ToString());
                if (debug.data != data) {

                    debug.data = data;

                    this.world.DestroyAllViews<DebugEntity>(debug.entity);
                    this.world.InstantiateView<DebugEntity>(this.debugViewSourceId, debug.entity);

                }

            }

        }

        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];
                if (this.allCharacterUnits.Contains(unit) == true) {

                    var comp = this.world.GetComponent<UnitEntity, PathfindingPathComponent>(unit.entity);
                    if (comp != null) {

                        var nodes = comp.nodes;
                        if (nodes != null && nodes.Count > 0) {

                            UnityEngine.Debug.DrawLine(unit.position.XY(), (UnityEngine.Vector3)nodes[0].position, UnityEngine.Color.yellow);
                            UnityEngine.Debug.DrawLine(unit.position.XY(), (UnityEngine.Vector3)nodes[nodes.Count - 1].position, UnityEngine.Color.cyan);

                        }

                    }

                }

            }

        }
        
    }
    
}