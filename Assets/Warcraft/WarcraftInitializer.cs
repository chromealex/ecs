using UnityEngine;
using EntityId = System.Int32;
using RPCId = System.Int32;
using ViewId = System.UInt64; 

namespace Warcraft {
    
    using TState = WarcraftState;
    using ME.ECS;
    using ME.ECS.Views.Providers;
    using Modules;
    using Features;
    
    public class WarcraftInitializer : MonoBehaviour {

        public int activePlayerIndex;
        public float tickTime = 0.033f;
        public bool debug;

        private World<TState> world;
        
        public void Update() {

            if (this.world != null) {

                this.world.Update(Time.deltaTime);

            }

        }

        public void OnGUI() {

            if (this.world != null) return;
            
            if (GUILayout.Button("Init world") == true) {
                
                this.InitializeWorld();
                
            }

        }

        private void InitializeWorld() {
            
            if (this.world == null) {

                // Initialize world with 0.033 time step
                WorldUtilities.CreateWorld(ref this.world, this.tickTime);
                {
                    // TODO: Add your modules here
                    // Ex: this.world.AddModule<TModule>();
                    this.world.AddModule<FPSModule>();
                    this.world.AddModule<StatesHistoryModule>();
                    this.world.AddModule<NetworkModule>();
                    
                    // Create new state
                    this.world.SetState(WorldUtilities.CreateState<TState>());
                    // TODO: Initialize your custom state data here
                    // Ex: this.world.GetState().yourCustomParameter = yourValue;
                    
                    // TODO: Register view sources here or in each features/systems directly
                    // Based on render engine (Now we have support for GameObject Render, Particles Render, DrawMesh Render)
                    // You need to provide MonoBehaviourViewBase/GameObject, ParticleViewSourceBase or DrawMeshViewSourceBase as a prefab type
                    // Ex: var viewSourceId = this.world.RegisterViewSource<TEntity>(prefab);
    
                    // TODO: Add your features or systems here
                    // Ex: this.world.AddFeature<TFeature>(); or this.world.AddSystem<TSystem>();
                    // btw you can pass custom parameters into features like AddFeature<TFeature, TCustomParameters>(new TCustomParameters(...));
                    this.world.AddFeature<MapFeature>();
                    this.world.AddFeature<PathfindingFeature>();
                    this.world.AddFeature<UnitsFeature>();
                    this.world.AddFeature<AIFeature>();
                    this.world.AddFeature<PlayersFeature, ConstructParameters<int>>(new ConstructParameters<int>(this.activePlayerIndex));
                    this.world.AddFeature<ForestFeature>();
                    this.world.AddFeature<FogOfWarFeature>();

                    this.world.AddFeature<PeasantsFeature>();
                    this.world.AddFeature<UnitsSelectionFeature>();
                    this.world.AddFeature<UnitsPlacementFeature>();
                    this.world.AddFeature<UnitsUpgradesFeature>();
                    this.world.AddFeature<UnitsQueueFeature>();
                    this.world.AddFeature<UnitsMovementFeature>();
                    this.world.AddFeature<UnitsAttackFeature>();

                    this.world.AddFeature<CameraFeature>();
                    this.world.AddFeature<InputFeature>();

                    if (this.debug == true) this.world.AddFeature<DebugFeature>();

                }
                // Save initialization state
                this.world.SaveResetState();

            }

        }

        public void OnDestroy() {

            // Release world
            WorldUtilities.ReleaseWorld(ref this.world);

        }

    }
    
}