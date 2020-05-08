using ME.ECS;

namespace Prototype.Features {
    
    using TState = PrototypeState;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitsFeature : Feature<TState> {

        public Prototype.Features.Units.Data.FeatureUnitsData resourcesData { get; private set; }

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.resourcesData = UnityEngine.Resources.Load<Prototype.Features.Units.Data.FeatureUnitsData>("FeatureUnitsData");

            this.AddSystem<Prototype.Features.Units.Systems.UnitClearFlagsOnceSystem>();
            
            this.AddSystem<Prototype.Features.Units.Systems.SquadSpawnSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitSpawnSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitBulletSpawnSystem>();

            this.AddSystem<Prototype.Features.Units.Systems.UnitSetNearestNodeSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitSetTargetSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitBuildPathSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitPickNextPointSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitCheckPathCompleteSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitMoveByPathSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitMoveToTargetSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.SquadRotateDiscreteSystem>();
            
            //this.AddSystem<Prototype.Features.Units.Systems.SquadSwapSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.SquadSwapOnCompleteSystem>();

            this.AddSystem<Prototype.Features.Units.Systems.UnitInSquadMoveSystem>();

            this.AddSystem<Prototype.Features.Units.Systems.UnitBulletSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitCancelAttackSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.SquadSearchAttackTargetSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitSearchAttackTargetSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitAttackTargetSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitHitSystem>();
            this.AddSystem<Prototype.Features.Units.Systems.UnitDeathSystem>();

        }

        protected override void OnDeconstruct() {
            
        }

        public bool Push(Entity squad, IFilter<TState, Unit> filter, int startIndex = 0) {
            
            var nearest = AstarPath.active.GetNearest(squad.GetPosition());
            var gn = (nearest.node as Pathfinding.GridNode);
            for (int i = startIndex; i < startIndex + 8; ++i) {

                var idx = i % 8;
                if (gn.HasConnectionInDirection(idx) == true) {

                    var n = gn.GetNeighbourAlongDirection(idx);
                    if (n.Walkable == false) continue;
                                
                    foreach (var sq in filter) {

                        if (sq != squad &&
                            sq.GetData<Prototype.Features.Units.Components.NearestNode>().nodeIndex == n.NodeIndex &&
                            sq.GetData<Prototype.Features.Units.Components.Owner>().value == squad.GetData<Prototype.Features.Units.Components.Owner>().value &&
                            sq.HasData<Prototype.Features.Units.Components.PathTraverse>() == false) {
                                        
                            continue;
                                        
                        }
         
                    }
         
                    squad.SetData(new Prototype.Features.Units.Components.BuildPathToTarget() {
                        value = (UnityEngine.Vector3)n.position
                    });
                    return true;

                }

            }

            return false;

        }

    }

}