using UnityEngine;
using ME.ECS;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ViewId = System.UInt64;
using ME.ECS.Views.Providers;

namespace ME.GameExample.Game {

    using ME.GameExample.Game.Modules;
    using ME.GameExample.Game.Systems;
    using ME.GameExample.Game.Entities;

    public class GameExample : MonoBehaviour {

        public int worldId;
        public int worldConnectionId;
        [Range(0, 100)] public int dropPercent;
        public float deltaTimeMultiplier = 1f;
        public Color playerColor;
        public float moveSide;
        public int spawnUnitsCount = 10;

        public World<GameState> world;

        public Vector3 p1Position = new Vector3(0f, 0f, 3f);
        public Vector3 p2Position = new Vector3(0f, 0f, -3f);

        public ViewId pointViewSourceId;
        public ViewId unitViewSourceId;
        public ViewId unitViewSourceId2;
        public ViewId explosionSourceId;

        public ParticleViewSourceBase pointSourceGameObject;
        public ParticleViewSourceBase unitSource;
        public ParticleViewSourceBase unitSource2;
        public ParticleViewSourceBase explosionSource;

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

                var state = WorldUtilities.CreateState<GameState>();
                state.worldPosition = this.transform.position;
                this.world.SetState(state);

                this.RegisterViewSources();

                var p1 = this.world.AddEntity(new Point() { position = this.transform.position + this.p1Position, scale = Vector3.one });
                var p2 = this.world.AddEntity(new Point() { position = this.transform.position + this.p2Position, scale = Vector3.one });

                this.world.InstantiateView<Point>(this.pointViewSourceId, p1);
                this.world.InstantiateView<Point>(this.pointViewSourceId, p2);

                this.world.AddSystem<InputSystemMarkers>();
                this.world.AddSystem<PointsSystem>();
                this.world.AddSystem<UnitsSystem>();
                this.world.AddSystem<ExplosionsSystem>();
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

        public void RegisterViewSources() {

            this.pointViewSourceId = this.world.RegisterViewSource<Point>(this.pointSourceGameObject);
            this.unitViewSourceId = this.world.RegisterViewSource<Unit>(this.unitSource);
            this.unitViewSourceId2 = this.world.RegisterViewSource<Unit>(this.unitSource2);
            this.explosionSourceId = this.world.RegisterViewSource<Explosion>(this.explosionSource);

        }

    }

}