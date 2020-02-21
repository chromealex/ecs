using UnityEngine;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ViewId = System.UInt64; 
using ME.ECS;
using ME.ECS.Views.Providers;

namespace ME.Example.Game {
    
    using ME.Example.Game.Features;
    using ME.Example.Game.Modules;
    using ME.Example.Game.Systems;
    using ME.Example.Game.Entities;

    [System.Serializable]
    public struct PointsFeatureInitParameters : IConstructParameters {

        public Vector3 p1Position;
        public Vector3 p2Position;
        public ViewId pointViewSourceId;

        public Entity p1;
        public Entity p2;

    }

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

        public PointsFeatureInitParameters pointsFeatureInitParameters = new PointsFeatureInitParameters() { p1Position = new Vector3(0f, 0f, 3f), p2Position = new Vector3(0f, 0f, -3f) };

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
                this.world.GetState().worldPosition = this.transform.position;
                
                this.RegisterViewSources();

                this.pointsFeatureInitParameters.pointViewSourceId = this.pointViewSourceId;
                this.world.AddFeature<PointsFeature, PointsFeatureInitParameters>(ref this.pointsFeatureInitParameters);
                this.world.AddFeature<UnitsFeature>();
                this.world.AddFeature<InputFeature, ConstructParameters<Entity, Entity>>(new ConstructParameters<Entity, Entity>(this.pointsFeatureInitParameters.p1, this.pointsFeatureInitParameters.p2));
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

            this.world.AddMarker(new ME.Example.Game.Components.UI.UIMove() {
                pointId = pointId,
                color = this.playerColor,
                moveSide = this.moveSide
            });
            
        }

        public virtual void AddUnitButtonClick() {

            this.world.AddMarker(new ME.Example.Game.Components.UI.UIAddUnit() {
                color = this.playerColor,
                count = this.spawnUnitsCount,
                viewSourceId = this.unitViewSourceId,
                viewSourceId2 = this.unitViewSourceId2,
            });

        }

    }

}
