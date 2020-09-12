using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class InitializerBase : MonoBehaviour {

        [System.Serializable]
        public struct EndOfBaseClass { }

        public FeaturesList featuresList = new FeaturesList();
        public FeaturesListCategories featuresListCategories = new FeaturesListCategories();
        public WorldSettings worldSettings = WorldSettings.Default;
        public WorldDebugSettings worldDebugSettings = WorldDebugSettings.Default;
        public EndOfBaseClass endOfBaseClass;

        protected virtual void OnValidate() {

            if (this.featuresList.features.Count > 0 && this.featuresListCategories.items.Count == 0) {

                this.ConvertVersionFrom1To2();

            }

        }

        public void ConvertVersionFrom1To2() {
            
            this.featuresListCategories.items = new List<FeaturesListCategory>() {
                new FeaturesListCategory() {
                    features = new FeaturesList() { features = this.featuresList.features.ToList() }
                }
            };
            this.featuresList = new FeaturesList();

        }
        
        protected void Initialize(World world) {

            world.SetSettings(this.worldSettings);
            world.SetDebugSettings(this.worldDebugSettings);
            this.InitializeFeatures(world);
            
        }

        protected void InitializeFeatures(World world) {

            this.featuresListCategories.Initialize(world);

        }

        protected void DeInitializeFeatures(World world) {

            this.featuresListCategories.DeInitialize(world);

        }

    }

}
