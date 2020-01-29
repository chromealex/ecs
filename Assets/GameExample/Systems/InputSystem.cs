using ME.ECS;
using UnityEngine;
using RPCId = System.Int32;
using ViewId = System.UInt64;

namespace ME.GameExample.Game.Systems {

    using ME.GameExample.Game.Modules;
    using ME.GameExample.Game.Components;
    using ME.GameExample.Game.Entities;

    public class InputSystemMarkers : ISystem<GameState> {

        public IWorld<GameState> world { get; set; }
        
        private RPCId fireEventCallId;
        private RPCId testEventCallId;
        private RPCId createUnitCallId;

        void ISystemBase.OnConstruct() {

            var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            this.testEventCallId = network.RegisterRPC(new System.Action<int, Color, float>(this.TestEvent_RPC).Method);
            this.createUnitCallId = network.RegisterRPC(new System.Action<Color, int, ViewId, ViewId, bool>(this.CreateUnit_RPC).Method);
            this.fireEventCallId = network.RegisterRPC(new System.Action<ViewId>(this.FireEvent_RPC).Method);
            network.RegisterObject(this, 1);

        }

        void ISystemBase.OnDeconstruct() {

            var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            network.UnRegisterObject(this, 1);
            network.UnRegisterRPC(this.testEventCallId);

        }

        void ISystem<GameState>.AdvanceTick(GameState state, float deltaTime) {}

        private ulong savedTick;

        void ISystem<GameState>.Update(GameState state, float deltaTime) {

            if (this.world.GetMarker(out ME.Example.Game.Components.UI.UIMove moveMarker) == true) {
                
                this.AddEventUIButtonClick(moveMarker.pointId, moveMarker.color, moveMarker.moveSide);
                
            }

            if (this.world.GetMarker(out ME.Example.Game.Components.UI.UIAddUnit createUnitMarker) == true) {
                
                this.AddUnitButtonClick(createUnitMarker.color, createUnitMarker.count, createUnitMarker.viewSourceId, createUnitMarker.viewSourceId2, createUnitMarker.topSpawn);
                
            }

            if (this.world.GetMarker(out ME.Example.Game.Components.UI.UIFire fire) == true) {
                
                this.FireUIButtonClick(fire.viewSourceId);
                
            }

        }

        public void FireUIButtonClick(ViewId viewSourceId) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.fireEventCallId, viewSourceId);

        }

        public void AddEventUIButtonClick(int pointId, Color color, float moveSide) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.testEventCallId, pointId, color, moveSide);

        }

        public void AddUnitButtonClick(Color color, int count, ViewId viewSourceId, ViewId viewSourceId2, bool topSpawn) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.createUnitCallId, color, count, viewSourceId, viewSourceId2, topSpawn);

        }

        private void CreateUnit_RPC(Color color, int count, ViewId viewSourceId, ViewId viewSourceId2, bool topSpawn) {

            var p1 = Entity.Create<Point>(topSpawn == true ? 1 : 2);
            var p2 = Entity.Create<Point>(topSpawn == true ? 2 : 1);

            var p1Position = Vector3.zero;
            Point data;
            if (this.world.GetEntityData(p1.id, out data) == true) {
                p1Position = data.position;
            }

            for (int i = 0; i < count; ++i) {

                var unit = this.world.AddEntity(new Unit() {
                    position = this.world.GetRandomInSphere(p1Position, 1f), scale = Vector3.one * 0.3f, lifes = 10, maxLifes = 10, speed = this.world.GetRandomRange(3f, 5f), pointFrom = p1, pointTo = p2
                });
                var followComponent = this.world.AddComponent<Unit, UnitFollowFromTo>(unit);
                followComponent.@from = p1;
                followComponent.to = p2;

                this.world.AddComponent<Unit, UnitGravity>(unit);
                var setColor = this.world.AddComponent<Unit, UnitSetColor>(unit);
                setColor.color = color;

                this.world.InstantiateView<Unit>(viewSourceId, unit);
                this.world.InstantiateView<Unit>(viewSourceId2, unit);

            }
            
        }

        private void TestEvent_RPC(int pointId, Color color, float moveSide) {

            var componentColor = this.world.AddComponent<Point, PointSetColor>(Entity.Create<Point>(pointId));
            componentColor.color = color;

            var componentPos = this.world.AddComponent<Point, PointAddPositionDelta>(Entity.Create<Point>(pointId));
            componentPos.positionDelta = Vector3.left * moveSide;

        }

        private void FireEvent_RPC(ViewId viewSourceId) {

            var rndCount = (int)this.world.GetRandomRange(1f, 10f);
            for (int i = 0; i < rndCount; ++i) {

                var radius = 20f;
                var damage = 2f;
                var range = 3f;
                var expData = new Explosion() {
                    position = this.world.GetRandomInSphere(Worlds<GameState>.currentState.worldPosition, radius),
                    damage = damage,
                    range = range,
                    lifetime = 5f,
                };
                var worldPosY = this.world.GetState().worldPosition.y;
                if (expData.position.y < worldPosY || expData.position.y > worldPosY) expData.position.y = worldPosY;
                var entity = this.world.AddEntity(expData);

                this.world.AddComponent<Explosion, ExplosionFire>(entity);
                this.world.AddComponent<Explosion, ExplosionFireDestroy>(entity);
                this.world.InstantiateView<Explosion>(viewSourceId, entity);

            }
            
        }

    }

}