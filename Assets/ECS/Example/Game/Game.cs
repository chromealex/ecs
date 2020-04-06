using UnityEngine;
using ME.ECS;
using ME.ECS.Views.Providers;

namespace ME.Example.Game {
    
    using ME.Example.Game.Features;
    using ME.Example.Game.Modules;
    using ME.Example.Game.Systems;
    using ME.Example.Game.Entities;

    [System.Serializable]
    public struct PointsFeatureInitParameters : IConstructParameters {

        public Vector3 zone1Position;
        public Vector3 zone2Position;
        public Vector3 zone3Position;
        public Vector3 zone4Position;

        public Vector3 zone1Scale;
        public Vector3 zone2Scale;
        public Vector3 zone3Scale;
        public Vector3 zone4Scale;

        public Vector3 p1Position;
        public Vector3 p2Position;
        public ViewId pointViewSourceId;
        public ViewId playerZoneViewSourceId;
        public Color[] playersColor;

        public Entity p1;
        public Entity p2;

    }

    public class Game : MonoBehaviour {

        public float deltaTimeMultiplier = 1f;
        public Color[] playersColor;
        public Vector2[] moveSides;
        public int spawnUnitsCount = 10;
        public float inputKeysMoveSpeed = 10f;

        public World<State> world;

        public PointsFeatureInitParameters pointsFeatureInitParameters = new PointsFeatureInitParameters() { p1Position = new Vector3(0f, 0f, 3f), p2Position = new Vector3(0f, 0f, -3f) };

        protected ViewId playerZoneViewSourceId;
        protected ViewId pointViewSourceId;
        protected ViewId unitViewSourceId;
        protected ViewId unitViewSourceId2;

        public void Update() {

            if (this.world == null) {

                // Loading level
                
                WorldUtilities.CreateWorld(ref this.world, 0.033f);
                this.world.AddModule<FPSModule>();
                this.world.AddModule<StatesHistoryModule>();
                this.world.AddModule<NetworkModule>();
                
                this.world.SetState(WorldUtilities.CreateState<State>());
                this.world.GetState().worldPosition = this.transform.position;
                
                this.RegisterViewSources();

                this.pointsFeatureInitParameters.pointViewSourceId = this.pointViewSourceId;
                this.pointsFeatureInitParameters.playerZoneViewSourceId = this.playerZoneViewSourceId;
                this.pointsFeatureInitParameters.playersColor = this.playersColor;
                this.world.AddFeature<PointsFeature, PointsFeatureInitParameters>(ref this.pointsFeatureInitParameters);
                this.world.AddFeature<UnitsFeature>();
                this.world.AddFeature<InputFeature, ConstructParameters<Entity, Entity>>(new ConstructParameters<Entity, Entity>(this.pointsFeatureInitParameters.p1, this.pointsFeatureInitParameters.p2));
                this.world.SaveResetState();

            }

            if (this.world != null) {

                this.InputKeys();
                
                var dt = Time.deltaTime * this.deltaTimeMultiplier;
                this.world.Update(dt);

            }

        }

        public void OnDestroy() {

            WorldUtilities.ReleaseWorld(ref this.world);

        }

        private void InputKeys() {

            if (Photon.Pun.PhotonNetwork.IsConnected == false || Photon.Pun.PhotonNetwork.InRoom == false) return;
            
            var hasInput = false;
            var dir = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow) == true) {
                hasInput = true;
                dir += Vector2.left;
            }

            if (Input.GetKey(KeyCode.RightArrow) == true) {
                hasInput = true;
                dir += Vector2.right;
            }

            if (Input.GetKey(KeyCode.UpArrow) == true) {
                hasInput = true;
                dir += Vector2.up;
            }

            if (Input.GetKey(KeyCode.DownArrow) == true) {
                hasInput = true;
                dir += Vector2.down;
            }

            if (hasInput == true) {
                
                this.world.AddMarker(new ME.Example.Game.Components.UI.UIMove() {
                    pointId = 1,
                    color = this.playersColor[Game.Repeat(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, this.playersColor.Length)],
                    moveSide = dir * (Time.deltaTime * this.inputKeysMoveSpeed)
                });
                
            }

            if (Input.GetKeyDown(KeyCode.Space) == true) {
                
                this.AddUnitButtonClick();
                
            }

        }

        public virtual void RegisterViewSources() {
            
        }

        public virtual void AddEventUIButtonClick(int pointId) {

            this.world.AddMarker(new ME.Example.Game.Components.UI.UIMove() {
                pointId = pointId,
                color = this.playersColor[Game.Repeat(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, this.playersColor.Length)],
                moveSide = this.moveSides[Game.Repeat(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, this.playersColor.Length)]
            });
            
        }

        public virtual void AddUnitButtonClick() {

            this.world.AddMarker(new ME.Example.Game.Components.UI.UIAddUnit() {
                color = this.playersColor[Game.Repeat(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber, this.playersColor.Length)],
                count = this.spawnUnitsCount,
                viewSourceId = this.unitViewSourceId,
                viewSourceId2 = this.unitViewSourceId2,
            });

        }

        public static int Repeat(int index, int length) {

            while (index >= length) {

                index -= length;

            }

            return index;

        }

    }

}
