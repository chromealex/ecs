using ME.ECS;

namespace Prototype.Features.Input.Modules {
    
    using TState = PrototypeState;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class MouseInputModule : IModule<TState> {

        private InputFeature inputFeature;
        
        public IWorld<TState> world { get; set; }

        void IModuleBase.OnConstruct() {

            this.inputFeature = this.world.GetFeature<InputFeature>();

        }
        
        void IModuleBase.OnDeconstruct() {}
        
        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {}

        private UnityEngine.Vector3 startDragPos;
        private bool isDragging;
        void IModule<TState>.Update(TState state, float deltaTime) {

            if (UnityEngine.Input.GetMouseButtonDown(0) == true) {

                var camera = UnityEngine.Camera.main;
                var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out var hitInfo, 1000f, this.inputFeature.resourcesData.groundMask) == true) {

                    this.startDragPos = hitInfo.point;
                    
                }

            }

            if (UnityEngine.Input.GetMouseButton(0) == true) {
                
                var camera = UnityEngine.Camera.main;
                var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out var hitInfo, 1000f, this.inputFeature.resourcesData.groundMask) == true) {

                    if (this.isDragging == false) {

                        var dist = (this.startDragPos - hitInfo.point).sqrMagnitude;
                        if (dist >= this.inputFeature.resourcesData.dragThreshold * this.inputFeature.resourcesData.dragThreshold) {

                            this.isDragging = true;

                            this.world.AddMarker(new WorldDragBegin() {
                                worldPos = this.startDragPos,
                            });

                        }

                    } else {
                        
                        this.world.AddMarker(new WorldDrag() {
                            fromWorldPos = this.startDragPos,
                            toWorldPos = hitInfo.point,
                        });
                        
                    }

                }
                
            }

            if (UnityEngine.Input.GetMouseButtonUp(0) == true) {

                if (this.isDragging == true) {

                    this.isDragging = false;
                    
                    var camera = UnityEngine.Camera.main;
                    var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                    if (UnityEngine.Physics.Raycast(ray, out var hitInfo, 1000f, this.inputFeature.resourcesData.groundMask) == true) {

                        this.world.AddMarker(new WorldDragEnd() {
                            fromWorldPos = this.startDragPos,
                            toWorldPos = hitInfo.point,
                        });

                    }

                } else {
                    
                    var camera = UnityEngine.Camera.main;
                    var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                    if (UnityEngine.Physics.Raycast(ray, out var hitInfo, 1000f, this.inputFeature.resourcesData.groundMask) == true) {

                        this.world.AddMarker(new WorldClick() {
                            worldPos = hitInfo.point
                        });
                    
                    }

                }

            }

        }
        
    }
    
}
