using ME.ECS;

namespace Warcraft.Modules {
    
    using TState = WarcraftState;
    using Warcraft.Markers;
    
    public class MouseInputModule : IModule<TState> {

        private bool dragBeginInit;
        private bool dragBegin;
        private float dragDistance;
        private UnityEngine.Vector2 dragBeginPosition;
        
        public IWorld<TState> world { get; set; }

        void IModuleBase.OnConstruct() {

            this.dragDistance = 0.2f;

        }
        
        void IModuleBase.OnDeconstruct() {}
        
        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {}

        void IModule<TState>.Update(TState state, float deltaTime) {
            
            var isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            
            var mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            var mousePosition = UnityEngine.Input.mousePosition;

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape) == true || UnityEngine.Input.GetMouseButtonDown(1) == true) {

                this.world.AddMarker(new CancelMarker());

            }
            
            if (isOverUI == false) {

                this.world.AddMarker(new InputMove() {
                    position = mousePosition.XY(),
                    worldPosition = mapFeature.GetWorldPositionFromInput(mousePosition.XY())
                });

            }

            if (isOverUI == false && UnityEngine.Input.GetMouseButtonDown(0) == true) {

                this.dragBeginPosition = mousePosition.XY();
                this.dragBeginInit = true;

            }

            if (isOverUI == false && this.dragBeginInit == true && (this.dragBeginPosition - mousePosition.XY()).sqrMagnitude >= this.dragDistance * this.dragDistance) {

                this.dragBeginInit = false;
                this.dragBegin = true;
                this.world.AddMarker(new InputDragBegin() {
                    position = mousePosition.XY(),
                    worldPosition = mapFeature.GetWorldPositionFromInput(mousePosition.XY())
                });

            }

            if (this.dragBegin == true) {

                this.world.AddMarker(new InputDragMove() {
                    beginPosition = this.dragBeginPosition,
                    beginWorldPosition = mapFeature.GetWorldPositionFromInput(this.dragBeginPosition),
                    position = mousePosition.XY(),
                    worldPosition = mapFeature.GetWorldPositionFromInput(mousePosition.XY())
                });

            }

            if (UnityEngine.Input.GetMouseButtonUp(0) == true || UnityEngine.Input.GetMouseButtonUp(1) == true) {

                if (this.dragBegin == true) {

                    this.world.AddMarker(new InputDragEnd() {
                        beginPosition = this.dragBeginPosition,
                        beginWorldPosition = mapFeature.GetWorldPositionFromInput(this.dragBeginPosition),
                        position = mousePosition.XY(),
                        worldPosition = mapFeature.GetWorldPositionFromInput(mousePosition.XY())
                    });

                } else if (isOverUI == false) {

                    if (UnityEngine.Input.GetMouseButtonUp(1) == true) {
                     
                        this.world.AddMarker(new InputRightClick() {
                            position = mousePosition.XY(),
                            worldPosition = mapFeature.GetWorldPositionFromInput(mousePosition.XY())
                        });
   
                    } else {

                        this.world.AddMarker(new InputLeftClick() {
                            position = mousePosition.XY(),
                            worldPosition = mapFeature.GetWorldPositionFromInput(mousePosition.XY())
                        });

                    }

                }

                this.dragBeginInit = false;
                this.dragBegin = false;

            }

        }
        
    }
    
}
