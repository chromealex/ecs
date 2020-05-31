using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    public abstract class InitializerBase : MonoBehaviour {

        [System.Serializable]
        public struct EndOfBaseClass { }

        public FeaturesList featuresList = new FeaturesList();
        public WorldSettings worldSettings = WorldSettings.Default;
        public WorldDebugSettings worldDebugSettings = WorldDebugSettings.Default;
        public EndOfBaseClass endOfBaseClass;

        protected void Initialize(World world) {

            world.SetSettings(this.worldSettings);
            world.SetDebugSettings(this.worldDebugSettings);
            this.InitializeFeatures(world);
            
        }

        protected void InitializeFeatures(World world) {

            this.featuresList.Initialize(world);

        }

        protected void DeInitializeFeatures(World world) {

            this.featuresList.DeInitialize(world);

        }

    }

}
