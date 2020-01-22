using UnityEngine;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ViewId = System.UInt64; 
using ME.ECS;
using ME.ECS.Views.Providers;

namespace ME.Example.Game {
    
    using ME.Example.Game.Modules;
    using ME.Example.Game.Systems;
    using ME.Example.Game.Entities;

    public class Game : MonoBehaviour {

        public int worldId;
        public int worldConnectionId;
        [Range(0, 100)]
        public int dropPercent;
        public float deltaTimeMultiplier = 1f;
        public Color playerColor;
        public float moveSide;
        public int spawnUnitsCount = 10;

        public World<State> world;
        private IState<State> savedState;

        protected ViewId pointViewSourceId;
        protected ViewId unitViewSourceId;
        protected ViewId unitViewSourceId2;

        public void Update() {

            if (this.world == null) {

                // Loading level
                
                WorldUtilities.CreateWorld(ref this.world, 0.133f, this.worldId);
                this.world.AddModule<StatesHistoryModule>();
                this.world.AddModule<NetworkModule>();
                
                if (this.worldConnectionId > 0) {

                    var network = this.world.GetModule<NetworkModule>();
                    network.SetWorldConnection(this.worldConnectionId);
                    network.SetDropPercent(this.dropPercent);

                }

                this.world.SetState(WorldUtilities.CreateState<State>());
                
                this.RegisterViewSources();

                var p1 = this.world.AddEntity(new Point() { position = this.transform.position + new Vector3(0f, 0f, 3f), scale = Vector3.one, unitsCount = 99f, increaseRate = 1f });
                var p2 = this.world.AddEntity(new Point() { position = this.transform.position + new Vector3(0f, 0f, -3f), scale = Vector3.one, unitsCount = 1f, increaseRate = 1f });
                
                this.world.InstantiateView<Point>(this.pointViewSourceId, p1);
                this.world.InstantiateView<Point>(this.pointViewSourceId, p2);

                this.world.AddSystem<InputSystem>();
                this.world.AddSystem<PointsSystem>();
                this.world.AddSystem<UnitsSystem>();
                this.world.SaveResetState();

            }

            if (this.world != null) {

                var dt = Time.deltaTime * this.deltaTimeMultiplier;
                this.world.Update(dt);

            }

        }

        public virtual void RegisterViewSources() {
            
        }

        public void AddEventUIButtonClick(int pointId) {

            var input = this.world.GetSystem<InputSystem>();
            input.AddEventUIButtonClick(pointId, this.playerColor, this.moveSide);

        }

        public void AddUnitButtonClick() {
            
            var input = this.world.GetSystem<InputSystem>();
            input.AddUnitButtonClick(this.playerColor, this.spawnUnitsCount, this.unitViewSourceId, this.unitViewSourceId2);
            
        }

    }

}
