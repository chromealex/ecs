using ME.ECS;

namespace Prototype.Features.Players.Systems {

    using TState = PrototypeState;
    using TEntity = Entities.Player;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class PlayerSelectionSystem : ISystem<TState>, ISystemUpdate<TState> {

        private PlayersFeature playersFeature;
        private IFilter<TState, Unit> selectorsFilter;
        private IFilter<TState, Unit> selectorsTargetFilter;

        private RPCId dragBeginRpcId;
        private RPCId dragMoveRpcId;
        private RPCId dragEndRpcId;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {
            
            this.playersFeature = this.world.GetFeature<PlayersFeature>();
            Filter<TState, Unit>.Create(ref this.selectorsFilter, "Filter-PlayerSelectionSystem").WithStructComponent<IsSelector>().Push();
            Filter<TState, Unit>.Create(ref this.selectorsTargetFilter, "Filter-PlayerSelectionTargetSystem").WithStructComponent<IsSelector>().WithStructComponent<IsTargetSelector>().Push();
            
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RegisterObject(this, 3);

            this.dragBeginRpcId = networkModule.RegisterRPC(new System.Action<int, Prototype.Features.Input.Markers.WorldDragBegin>(this.DragBegin_RPC).Method);
            this.dragMoveRpcId = networkModule.RegisterRPC(new System.Action<int, Prototype.Features.Input.Markers.WorldDrag>(this.DragMove_RPC).Method);
            this.dragEndRpcId = networkModule.RegisterRPC(new System.Action<int, Prototype.Features.Input.Markers.WorldDragEnd>(this.DragEnd_RPC).Method);

        }
        
        void ISystemBase.OnDeconstruct() {}
        
        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {

            if (this.world.GetMarker(out Prototype.Features.Input.Markers.WorldDragBegin markerBegin) == true) {

                var networkModule = this.world.GetModule<NetworkModule>();
                networkModule.RPC(this, this.dragBeginRpcId, Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, markerBegin);
                
            }

            if (this.world.GetMarker(out Prototype.Features.Input.Markers.WorldDrag marker) == true) {

                var pos = marker.toWorldPos;
                var nodeInfo = AstarPath.active.GetNearest(pos);

                var changed = false;
                var actorId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
                var player = this.playersFeature.GetPlayerEntityByActorId(actorId);
                foreach (var selector in this.selectorsTargetFilter) {

                    if (selector.GetData<Prototype.Features.Units.Components.Owner>().value == player) {

                        var p = selector.GetData<Prototype.Features.Units.Components.NearestNode>();
                        if (p.nodeIndex != nodeInfo.node.NodeIndex) {

                            selector.SetData(new Prototype.Features.Units.Components.NearestNode() { nodeIndex = nodeInfo.node.NodeIndex });
                            changed = true;
                            break;

                        }

                    }
                
                }

                if (changed == true) {

                    var networkModule = this.world.GetModule<NetworkModule>();
                    networkModule.RPC(this, this.dragMoveRpcId, Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, marker);

                }
                
            }
            
            if (this.world.GetMarker(out Prototype.Features.Input.Markers.WorldDragEnd markerEnd) == true) {

                var networkModule = this.world.GetModule<NetworkModule>();
                networkModule.RPC(this, this.dragEndRpcId, Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, markerEnd);

            }

        }

        private void DragBegin_RPC(int actorId, Prototype.Features.Input.Markers.WorldDragBegin marker) {

            var player = this.playersFeature.GetPlayerEntityByActorId(actorId);
            
            var nodeInfo = AstarPath.active.GetNearest(marker.worldPos);
            var worldPos = (UnityEngine.Vector3)nodeInfo.node.position;
            
            var selection = this.world.AddEntity(new Unit());
            selection.SetData(new IsSelector());
            selection.SetData(new Prototype.Features.Units.Components.Owner() { value = player });
            selection.SetPosition(worldPos);
                
            var viewId = this.world.RegisterViewSource<Unit>(this.playersFeature.resourcesData.selectorBegin);
            this.world.InstantiateView<Unit>(viewId, selection);
                
            var selectionTarget = this.world.AddEntity(new Unit());
            selectionTarget.SetData(new IsSelector());
            selectionTarget.SetData(new IsTargetSelector());
            selectionTarget.SetData(new Prototype.Features.Units.Components.Owner() { value = player });
            selectionTarget.SetPosition(worldPos);
            
            var viewIdTarget = this.world.RegisterViewSource<Unit>(this.playersFeature.resourcesData.selectorTarget);
            this.world.InstantiateView<Unit>(viewIdTarget, selectionTarget);

        }

        private void DragMove_RPC(int actorId, Prototype.Features.Input.Markers.WorldDrag marker) {
            
            var nodeInfo = AstarPath.active.GetNearest(marker.toWorldPos);
            var worldPos = (UnityEngine.Vector3)nodeInfo.node.position;
            
            var player = this.playersFeature.GetPlayerEntityByActorId(actorId);
            foreach (var selector in this.selectorsTargetFilter) {

                if (selector.GetData<Prototype.Features.Units.Components.Owner>().value == player) {

                    selector.SetPosition(worldPos);
                    selector.SetData(new Prototype.Features.Units.Components.NearestNode() { nodeIndex = nodeInfo.node.NodeIndex });

                }
                
            }

        }

        private void DragEnd_RPC(int actorId, Prototype.Features.Input.Markers.WorldDragEnd marker) {
            
            var player = this.playersFeature.GetPlayerEntityByActorId(actorId);
            foreach (var selector in this.selectorsFilter) {

                if (selector.GetData<Prototype.Features.Units.Components.Owner>().value == player) {

                    this.world.RemoveEntity<Unit>(selector);

                }
                
            }

        }

    }
    
}