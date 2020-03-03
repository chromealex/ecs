using ME.ECS;

namespace Warcraft.Features {
    
    using TState = WarcraftState;
    
    public class CameraFeature : Feature<TState> {

        private UnityEngine.Camera camera;

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.AddSystem<Warcraft.Systems.CameraSystem>();

            var cameraPrefab = UnityEngine.Resources.Load<Warcraft.Views.CameraView>("Camera");
            
            var cameraEntity = this.world.AddEntity(new Warcraft.Entities.CameraEntity() { cameraSize = cameraPrefab.cameraSize, position = new UnityEngine.Vector3(0f, 0f, -10f) });
            
            var cameraSourceId = this.world.RegisterViewSource<Warcraft.Entities.CameraEntity>(cameraPrefab);
            this.world.InstantiateView<Warcraft.Entities.CameraEntity>(cameraSourceId, cameraEntity);
            
        }

        protected override void OnDeconstruct() {
            
        }

        public void SetCamera(UnityEngine.Camera camera) {

            this.camera = camera;

        }

        public UnityEngine.Camera GetCamera() {

            return this.camera;

        }

    }

}