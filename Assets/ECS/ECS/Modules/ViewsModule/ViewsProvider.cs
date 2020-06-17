namespace ME.ECS.Views {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    public struct ParticleSimulationSettings {

        public enum SimulationType : byte {

            RestoreHard,
            RestoreSoft,
            NoRestore,

        }

        public static ParticleSimulationSettings @default {
            get {
                
                return new ParticleSimulationSettings() {
                    simulationType = SimulationType.RestoreSoft,
                    minSimulationTime = 0.2f,
                    minEndingTime = 0.1f,
                    halfEnding = 0.5f,
                };
                
            }
        }

        public SimulationType simulationType;
        public float minSimulationTime;
        public float minEndingTime;
        [UnityEngine.RangeAttribute(0.1f, 0.9f)]
        public float halfEnding;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    public struct ParticleSimulationItem {

        public UnityEngine.ParticleSystem particleSystem;
        
        private float simulateToTime;
        private float currentTime;
        private float simulateToTimeDuration;
        private bool customLifetime;
        private float customLifetimeValue;
        
        public void SetAsCustomLifetime() {

            this.customLifetime = true;

        }

        public float GetCustomLifetime() {

            return this.customLifetimeValue;

        }
        
        public void SimulateParticles(float time, uint seed, ParticleSimulationSettings settings) {
            
            /*this.particleSystem.Stop(true);
            this.particleSystem.Pause(true);
            if (this.particleSystem.useAutoRandomSeed == true) this.particleSystem.useAutoRandomSeed = false;
            if (this.particleSystem.randomSeed != seed) this.particleSystem.randomSeed = seed;*/
            
            if (settings.simulationType == ParticleSimulationSettings.SimulationType.RestoreSoft) {

                if (time > settings.minSimulationTime) {

                    var mainModule = this.particleSystem.main;
                    var duration = mainModule.duration / mainModule.simulationSpeed;
                    var halfEnding = (duration - time) * settings.halfEnding;
                    if (halfEnding <= settings.minEndingTime) { // if ending is less than Xms - skip

                        this.Reset();
                        return;

                    }

                    this.simulateToTime = time + halfEnding;
                    this.currentTime = 0f;
                    this.simulateToTimeDuration = halfEnding;

                    this.Simulate(this.simulateToTime);
                    if (this.customLifetime == false && this.particleSystem.particleCount <= 0) {

                        this.Reset();

                    }

                    this.Simulate(0f);

                } else {
                    
                    this.Simulate(time);
                    
                }

            } else if (settings.simulationType == ParticleSimulationSettings.SimulationType.RestoreHard) {
                
                this.Simulate(time);
                
            } else if (settings.simulationType == ParticleSimulationSettings.SimulationType.NoRestore) {
                
                this.Simulate(0f);
                
            }

        }

        public bool Update(float deltaTime, ParticleSimulationSettings settings) {

            if (settings.simulationType == ParticleSimulationSettings.SimulationType.RestoreSoft && this.simulateToTimeDuration > 0f) {

                this.currentTime += deltaTime / this.simulateToTimeDuration;
                if (this.currentTime <= 1f) {
                    
                    this.Simulate(this.currentTime * this.simulateToTime);
                    
                } else {

                    this.Reset();

                }

                return true;

            }

            return false;

        }

        private void Reset() {
            
            this.simulateToTimeDuration = 0f;
            this.currentTime = 0f;
            this.simulateToTime = 0f;

        }

        private void Simulate(float time) {

            if (this.customLifetime == true) {

                this.customLifetimeValue = time;

            } else {

                this.particleSystem.Simulate(time, withChildren: true);
                this.particleSystem.Play(withChildren: true);

            }

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    public struct ParticleSystemSimulationItem {
        
        public ParticleSimulationItem particleItem;
        public ParticleSimulationSettings settings;

        [UnityEngine.SerializeField][UnityEngine.HideInInspector]
        private bool hasDefault;
        
        public void OnValidate(UnityEngine.ParticleSystem particleSystem) {

            if (this.hasDefault == false) {
                
                this.settings = ParticleSimulationSettings.@default;
                this.hasDefault = true;

            }
            
            this.particleItem = new ParticleSimulationItem();
            this.particleItem.particleSystem = particleSystem;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetAsCustomLifetime() {

            this.particleItem.SetAsCustomLifetime();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float GetCustomLifetime() {

            return this.particleItem.GetCustomLifetime();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SimulateParticles(float time, uint seed) {

            this.particleItem.SimulateParticles(time, seed, this.settings);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Update(float deltaTime) {
            
            return this.particleItem.Update(deltaTime, this.settings);

        }

        public override string ToString() {

            return "Particle System Simulation Element";
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    public struct ParticleSystemSimulation {

        public ParticleSimulationItem[] particleItems;
        public ParticleSimulationSettings settings;
        public bool hasElements;

        [UnityEngine.SerializeField][UnityEngine.HideInInspector]
        private bool hasDefault;
        
        public void OnValidate(UnityEngine.ParticleSystem[] particleSystems) {

            if (this.particleItems == null || this.particleItems.Length != particleSystems.Length) {

                if (this.hasDefault == false) {
                    
                    this.settings = ParticleSimulationSettings.@default;
                    this.hasDefault = true;

                }
                
                this.particleItems = new ParticleSimulationItem[particleSystems.Length];
                for (int i = 0, cnt = this.particleItems.Length; i < cnt; ++i) {

                    this.particleItems[i] = new ParticleSimulationItem();
                    this.particleItems[i].particleSystem = particleSystems[i];

                }

                this.hasElements = particleSystems.Length > 0;

            } else {

                this.hasElements = false;

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SimulateParticles(float time, uint seed) {

            if (this.hasElements == false) return;
            
            for (int i = 0, cnt = this.particleItems.Length; i < cnt; ++i) {

                this.particleItems[i].SimulateParticles(time, seed, this.settings);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime) {
            
            if (this.hasElements == false) return;

            for (int i = 0, cnt = this.particleItems.Length; i < cnt; ++i) {

                this.particleItems[i].Update(deltaTime, this.settings);

            }
            
        }

        public override string ToString() {

            if (this.particleItems == null) return string.Empty;
            
            return "Particle Systems Count: " + this.particleItems.Length.ToString();
            
        }

    }

    public interface IDoValidate {

        void DoValidate();

    }

    public interface IViewsProviderInitializerBase : System.IComparable<IViewsProviderInitializerBase> {}

    public interface IViewsProviderInitializer : IViewsProviderInitializerBase {

        IViewsProvider Create();
        void Destroy(IViewsProvider instance);

    }

    public interface IViewsProviderBase {
        
        void OnConstruct();
        void OnDeconstruct();

    }

    public interface IViewsProvider : IViewsProviderBase, System.IComparable<IViewsProvider> {

        World world { get; set; }

        IView Spawn(IView prefab, ViewId prefabSourceId);
        void Destroy(ref IView instance);

        void Update(ME.ECS.Collections.BufferArray<Views> list, float deltaTime, bool hasChanged);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class ViewsProvider : IViewsProvider {

        int System.IComparable<IViewsProvider>.CompareTo(IViewsProvider other) {

            return 0;

        }
        
        public World world { get; set; }
        
        public abstract void OnConstruct();
        public abstract void OnDeconstruct();

        public abstract IView Spawn(IView prefab, ViewId prefabSourceId);
        public abstract void Destroy(ref IView instance);

        public abstract void Update(ME.ECS.Collections.BufferArray<Views> list, float deltaTime, bool hasChanged);

    }

}
