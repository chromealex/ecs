using ME.ECS;

namespace Prototype.Features.Units.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Unit;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class UnitSetTargetSystem : ISystem<TState>, ISystemUpdate<TState> {

        private PlayersFeature playersFeature;
        private IFilter<TState, TEntity> unitsFilter;
        private RPCId dragUnitsRpcId;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.playersFeature = this.world.GetFeature<PlayersFeature>();
            Filter<TState, TEntity>.Create(ref this.unitsFilter, "Filter-UnitSetTargetSystem").WithStructComponent<IsActive>().WithStructComponent<IsSquad>().Push();
            
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RegisterObject(this, 2);

            this.dragUnitsRpcId = networkModule.RegisterRPC(new System.Action<int, Prototype.Features.Input.Markers.WorldDragEnd>(this.DragUnits_RPC).Method);

        }
        
        void ISystemBase.OnDeconstruct() {}
        
        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {

            if (this.world.GetMarker(out Prototype.Features.Input.Markers.WorldDragEnd marker) == true) {
                
                var networkModule = this.world.GetModule<NetworkModule>();
                networkModule.RPC(this, this.dragUnitsRpcId, Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, marker);
                
            }

        }

        private void DragUnits_RPC(int actorId, Prototype.Features.Input.Markers.WorldDragEnd marker) {
            
            var player = this.playersFeature.GetPlayerEntityByActorId(actorId);

            var nodeInfo = AstarPath.active.GetNearest(marker.fromWorldPos);
            //var worldPosFrom = (UnityEngine.Vector3)nodeInfo.node.position;

            var nodeInfoTo = AstarPath.active.GetNearest(marker.toWorldPos);
            var worldPosTo = (UnityEngine.Vector3)nodeInfoTo.node.position;

            var isAttackCommand = false;
            var filter = this.unitsFilter;
            foreach (var unit in filter) {

                if (unit.GetData<Owner>().value != player && unit.GetData<NearestNode>().nodeIndex == nodeInfoTo.node.NodeIndex) {

                    isAttackCommand = true;
                    break;

                }

            }

            foreach (var unit in filter) {

                if (unit.GetData<Owner>().value == player && unit.GetData<NearestNode>().nodeIndex == nodeInfo.node.NodeIndex) {

                    if (isAttackCommand == false) unit.SetData(new IsPlayerCommand());
                    unit.SetData(new BuildPathToTarget() {
                        value = worldPosTo
                    });

                }

            }

        }

    }
    
}