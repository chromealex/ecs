using ME.ECS;
using UnityEngine;

namespace ME.Example.Game.Systems {

    using ME.Example.Game.Modules;
    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;

    public class InputSystem : ISystem<State>, ISystemAdvanceTick<State>, ISystemUpdate<State> {

        public Entity p1;
        public Entity p2;
        
        public IWorld<State> world { get; set; }
        private RPCId createUnitCallId;

        void ISystemBase.OnConstruct() {

            var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            this.createUnitCallId = network.RegisterRPC(new System.Action<Color, int, ViewId, ViewId>(this.CreateUnit_RPC).Method);
            network.RegisterObject(this, 1);

        }

        void ISystemBase.OnDeconstruct() {

            var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            network.UnRegisterObject(this, 1);

        }

        void ISystemAdvanceTick<State>.AdvanceTick(State state, float deltaTime) {}

        void ISystemUpdate<State>.Update(State state, float deltaTime) {
            
            ME.Example.Game.Components.UI.UIAddUnit marker;
            if (this.world.GetMarker(out marker) == true) {
                
                this.AddUnitButtonClick(marker.color, marker.count, marker.viewSourceId, marker.viewSourceId2);
                
            }

        }

        public void AddUnitButtonClick(Color color, int count, ViewId viewSourceId, ViewId viewSourceId2) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.createUnitCallId, color, count, viewSourceId, viewSourceId2);

        }

        private void CreateUnit_RPC(Color color, int count, ViewId viewSourceId, ViewId viewSourceId2) {

            var p1Position = Vector3.zero;
            Point data;
            if (this.world.GetEntityData(this.p1, out data) == true) {
                p1Position = data.position;
            }

            for (int i = 0; i < count; ++i) {

                var unit = this.world.AddEntity(new Unit() {
                    position = this.world.GetRandomInSphere(p1Position, 1f),
                    scale = Vector3.one * 0.3f,
                    lifes = 3,
                    speed = this.world.GetRandomRange(1f, 2f)
                });
                var followComponent = this.world.AddComponent<Unit, UnitFollowFromTo>(unit);
                followComponent.@from = this.p1;
                followComponent.to = this.p2;

                this.world.AddComponent<Unit, UnitGravity>(unit);
                var setColor = this.world.AddComponent<Unit, UnitSetColor>(unit);
                setColor.color = color;
                
                this.world.InstantiateView<Unit>(viewSourceId, unit);
                //this.world.InstantiateView<Unit>(viewSourceId2, unit);

            }

            //this.world.UpdateStorages<Unit>();
            
        }

    }

}