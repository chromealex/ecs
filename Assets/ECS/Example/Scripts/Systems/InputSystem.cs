using ME.ECS;
using UnityEngine;
using RPCId = System.Int32;
using ViewId = System.UInt64;

namespace ME.Example.Game.Systems {

    using ME.Example.Game.Modules;
    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;

    public class InputSystem : ISystem<State> {

        public IWorld<State> world { get; set; }
        private RPCId testEventCallId;
        private RPCId createUnitCallId;

        void ISystemBase.OnConstruct() {

            var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            this.testEventCallId = network.RegisterRPC(new System.Action<int, Color, float>(this.TestEvent_RPC).Method);
            this.createUnitCallId = network.RegisterRPC(new System.Action<Color, int, ViewId, ViewId>(this.CreateUnit_RPC).Method);
            network.RegisterObject(this, 1);

        }

        void ISystemBase.OnDeconstruct() {

            var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            network.UnRegisterObject(this, 1);
            network.UnRegisterRPC(this.testEventCallId);

        }

        void ISystem<State>.AdvanceTick(State state, float deltaTime) {}

        private ulong savedTick;

        void ISystem<State>.Update(State state, float deltaTime) {

            if (Input.GetKeyDown(KeyCode.Q) == true) {

                this.world.AddComponent<Point, PointIncreaseUnits>(Entity.Create<Point>(1));
                this.world.AddComponent<Point, PointIncreaseUnits>(Entity.Create<Point>(2));

            }

            if (Input.GetKey(KeyCode.F) == true) {

                var networkModule = this.world.GetModule<NetworkModule>();
                networkModule.RPC(this, this.testEventCallId, 1, Color.white);

            }

            if (this.world.id == 1) {

                if (Input.GetKeyDown(KeyCode.S) == true) {

                    this.savedTick = this.world.GetTick();

                }

                if (Input.GetKeyDown(KeyCode.R) == true) {

                    this.world.Simulate(this.savedTick);

                }

            }
            
        }

        public void AddEventUIButtonClick(int pointId, Color color, float moveSide) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.testEventCallId, pointId, color, moveSide);

        }

        public void AddUnitButtonClick(Color color, int count, ViewId viewSourceId, ViewId viewSourceId2) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.createUnitCallId, color, count, viewSourceId, viewSourceId2);

        }

        private void CreateUnit_RPC(Color color, int count, ViewId viewSourceId, ViewId viewSourceId2) {

            var p1 = Entity.Create<Point>(1);
            var p2 = Entity.Create<Point>(2);

            var p1Position = Vector3.zero;
            Point data;
            if (this.world.GetEntityData(p1.id, out data) == true) {
                p1Position = data.position;
            }

            for (int i = 0; i < count; ++i) {

                var unit = this.world.AddEntity(new Unit() {
                    position = this.world.GetRandomInSphere(p1Position, 1f), scale = Vector3.one * 0.3f, lifes = 1, speed = this.world.GetRandomRange(0.5f, 1.5f), pointFrom = p1, pointTo = p2
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

    }

}