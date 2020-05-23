using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    public abstract class InitializerBase : MonoBehaviour {

        public FeaturesList featuresList = new FeaturesList();
        public WorldSettings worldSettings = WorldSettings.Default;
        public WorldDebugSettings worldDebugSettings = WorldDebugSettings.Default;

        protected void Initialize<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            world.SetSettings(this.worldSettings);
            world.SetDebugSettings(this.worldDebugSettings);
            this.InitializeFeatures(world);
            
        }

        protected void InitializeFeatures<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            this.featuresList.Initialize(world);

        }

        protected void DeInitializeFeatures<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            this.featuresList.DeInitialize(world);

        }

    }

}
