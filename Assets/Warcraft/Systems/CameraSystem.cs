using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    
    public class CameraSystem : ISystem<TState> {

        private IFilter<TState, Warcraft.Entities.CameraEntity> cameras;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            Filter<TState, Warcraft.Entities.CameraEntity>.Create(ref this.cameras, "cameras").Push();

        }
        
        void ISystemBase.OnDeconstruct() {}
        
        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {}

        void ISystem<TState>.Update(TState state, float deltaTime) {

            var dir = UnityEngine.Vector2.zero;
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftArrow) == true) {
                
                dir += UnityEngine.Vector2.left;
                
            }
            
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightArrow) == true) {
                
                dir += UnityEngine.Vector2.right;
                
            }
            
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.UpArrow) == true) {
                
                dir += UnityEngine.Vector2.up;
                
            }
            
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.DownArrow) == true) {
                
                dir += UnityEngine.Vector2.down;
                
            }

            var mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            
            foreach (var index in state.cameras) {

                ref var camera = ref state.cameras[index];
                camera.position += dir.XY() * (deltaTime * 10f);
                mapFeature.ClampCamera(camera.cameraSize, ref camera.position);

            }

        }
        
    }
    
}