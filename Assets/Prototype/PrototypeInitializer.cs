using UnityEngine;

#region Namespaces
namespace Prototype.Entities {}
namespace Prototype.Systems {}
namespace Prototype.Components {}
namespace Prototype.Modules {}
namespace Prototype.Features {}
namespace Prototype.Markers {}
namespace Prototype.Views {}
#endregion

namespace Prototype {
    
    using TState = PrototypeState;
    using ME.ECS;
    using ME.ECS.Views.Providers;
    using Prototype.Modules;
    
    public class PrototypeInitializer : MonoBehaviour {

        public static bool playWithBot;
        public string roomName;
        
        private World<TState> world;

        public void Update() {

            if (this.world == null) {

                UnityEngine.Application.targetFrameRate = 120;
                
                // Initialize world with 0.033 time step
                WorldUtilities.CreateWorld(ref this.world, 0.033f);
                {
                    // TODO: Add your modules here
                    // Ex: this.world.AddModule<TModule>();
                    this.world.AddModule<FPSModule>();
                    this.world.AddModule<StatesHistoryModule>();
                    this.world.AddModule<NetworkModule>();
                    var networkModule = this.world.GetModule<NetworkModule>();
                    networkModule.SetRoomName(this.roomName);
                    
                    // Create new state
                    this.world.SetState(WorldUtilities.CreateState<TState>());
                    
                    this.world.AddFeature<Prototype.Features.InputFeature>();
                    this.world.AddFeature<Prototype.Features.MapFeature>();
                    this.world.AddFeature<Prototype.Features.PlayersFeature>();
                    this.world.AddFeature<Prototype.Features.UnitsFeature>();
                    this.world.AddFeature<Prototype.Features.BuildingsFeature>();

                }
                // Save initialization state
                this.world.SaveResetState();

            }

            if (this.world != null) {

                if (Input.GetKeyDown(KeyCode.B) == true) {

                    PrototypeInitializer.playWithBot = true;

                }

                this.world.Update(Time.deltaTime);

            }

        }
        
        public void OnDestroy() {

            // Release world
            WorldUtilities.ReleaseWorld(ref this.world);

        }

    }
    
}