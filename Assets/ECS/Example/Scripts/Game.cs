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

        public Vector3 p1Position = new Vector3(0f, 0f, 3f);
        public Vector3 p2Position = new Vector3(0f, 0f, -3f);
        
        protected ViewId pointViewSourceId;
        protected ViewId unitViewSourceId;
        protected ViewId unitViewSourceId2;

        public void Update() {

            if (this.world == null) {

                // Loading level
                
                WorldUtilities.CreateWorld(ref this.world, 0.133f, this.worldId);
                this.world.AddModule<FPSModule>();
                this.world.AddModule<StatesHistoryModule>();
                this.world.AddModule<NetworkModule>();
                
                if (this.worldConnectionId > 0) {

                    var network = this.world.GetModule<NetworkModule>();
                    network.SetWorldConnection(this.worldConnectionId);
                    network.SetDropPercent(this.dropPercent);

                }

                this.world.SetState(WorldUtilities.CreateState<State>());
                
                this.RegisterViewSources();

                var p1 = this.world.AddEntity(new Point() { position = this.transform.position + this.p1Position, scale = Vector3.one });
                var p2 = this.world.AddEntity(new Point() { position = this.transform.position + this.p2Position, scale = Vector3.one });
                
                this.world.InstantiateView<Point>(this.pointViewSourceId, p1);
                this.world.InstantiateView<Point>(this.pointViewSourceId, p2);

                this.world.AddSystem(new InputSystem() { p1 = p1, p2 = p2 });
                this.world.AddSystem<PointsSystem>();
                this.world.AddSystem<UnitsSystem>();
                this.world.SaveResetState();

            }

            if (this.world != null) {

                var dt = Time.deltaTime * this.deltaTimeMultiplier;
                this.world.Update(dt);

            }

        }

        public void OnDestroy() {

            WorldUtilities.ReleaseWorld(ref this.world);

        }

        public virtual void RegisterViewSources() {
            
        }

        public virtual void AddEventUIButtonClick(int pointId) {

            var input = this.world.GetSystem<InputSystem>();
            input.AddEventUIButtonClick(pointId == 1 ? input.p1 : input.p2, this.playerColor, this.moveSide);

        }

        public virtual void AddUnitButtonClick() {
            
            var input = this.world.GetSystem<InputSystem>();
            input.AddUnitButtonClick(this.playerColor, this.spawnUnitsCount, this.unitViewSourceId, this.unitViewSourceId2);
            
        }

    }

}
