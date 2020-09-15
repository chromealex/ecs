//#define TICK_THREADED
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define CHECKPOINT_COLLECTOR
#endif
using System.Collections.Generic;
using Unity.Jobs;

namespace ME.ECS {
    
    using ME.ECS.Collections;

    [System.Flags]
    public enum WorldStep : byte {

        None = 0x0,
        
        // Default types
        Modules = 0x1,
        Systems = 0x2,
        Plugins = 0x4,
        
        // Tick type
        LogicTick = 0x8,
        VisualTick = 0x10,
        Simulate = 0x20,
        
        // Fast links
        ModulesVisualTick = WorldStep.Modules | WorldStep.VisualTick,
        SystemsVisualTick = WorldStep.Systems | WorldStep.VisualTick,
        ModulesLogicTick = WorldStep.Modules | WorldStep.LogicTick,
        PluginsLogicTick = WorldStep.Plugins | WorldStep.LogicTick,
        SystemsLogicTick = WorldStep.Systems | WorldStep.LogicTick,
        PluginsLogicSimulate = WorldStep.Plugins | WorldStep.Simulate | WorldStep.LogicTick,

    }

    [System.Flags]
    public enum ModuleState : byte {

        AllActive = 0x0,
        VisualInactive = 0x1,
        LogicInactive = 0x2,

    }

    #pragma warning disable
    [System.Serializable]
    public partial struct WorldViewsSettings {

    }

    [System.Serializable]
    public struct WorldSettings {

        public bool useJobsForSystems;
        public bool useJobsForViews;
        public bool turnOffViews;

        public WorldViewsSettings viewsSettings;
        
        public static WorldSettings Default => new WorldSettings() {
            useJobsForSystems = true,
            useJobsForViews = true,
            turnOffViews = false,
            viewsSettings = new WorldViewsSettings()
        };

    }

    [System.Serializable]
    public partial struct WorldDebugViewsSettings { }

    [System.Serializable]
    public struct WorldDebugSettings {

        public bool createGameObjectsRepresentation;
        public bool showViewsOnScene;
        public WorldDebugViewsSettings viewsSettings;

        public static WorldDebugSettings Default => new WorldDebugSettings() {
            createGameObjectsRepresentation = false,
            showViewsOnScene = false,
            viewsSettings = new WorldDebugViewsSettings()
        };

    }
    #pragma warning restore
    
    public interface ICheckpointCollector {

        void Reset();
        void Checkpoint(object interestObj, WorldStep step);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class WorldBase {
        
        internal WorldStep currentStep;
        internal ListCopyable<IFeatureBase> features;
        //internal List<ISystemBase> systems;
        internal ListCopyable<IModuleBase> modules;
        internal BufferArray<SystemGroup> systemGroups;

        internal ListCopyable<ModuleState> statesFeatures;
        //internal List<ModuleState> statesSystems;
        internal ListCopyable<ModuleState> statesModules;

        internal ICheckpointCollector checkpointCollector;
        
        // State cache:
        //internal Storage storagesCache;
        //internal FiltersStorage filtersStorage;

        internal float tickTime;
        internal double timeSinceStart;
        public bool isActive;

        public ISystemBase currentSystemContext { get; internal set; }
        public BufferArray<bool> currentSystemContextFiltersUsed;

        internal Tick simulationFromTick;
        internal Tick simulationToTick;

        public WorldSettings settings { get; internal set; }
        public WorldDebugSettings debugSettings { get; internal set; }

        #if WORLD_THREAD_CHECK
        internal System.Threading.Thread worldThread;
        #endif

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed partial class World : WorldBase, IWorld, IPoolableSpawn, IPoolableRecycle {

        private const int FEATURES_CAPACITY = 100;
        private const int SYSTEMS_CAPACITY = 100;
        private const int MODULES_CAPACITY = 100;
        private const int ENTITIES_CACHE_CAPACITY = 100;
        private const int WORLDS_CAPACITY = 4;
        private const int FILTERS_CACHE_CAPACITY = 10;
        
        private static class FiltersDirectCache {

            internal static BufferArray<BufferArray<bool>> dic = new BufferArray<BufferArray<bool>>(null, 0);//new bool[World.WORLDS_CAPACITY][];

        }

        private static int registryWorldId = 0;

        public int id {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private set;
        }
        
        private State resetState;
        internal State currentState;
        private uint seed;
        private int cpf; // CPF = Calculations per frame
        internal int entitiesCapacity;

        /// <summary>
        /// Returns last frame CPF
        /// </summary>
        /// <returns></returns>
        public int GetCPF() {

            return this.cpf;

        }
        
        internal void UpdateGroup(SystemGroup group) {

            this.systemGroups.arr[group.worldIndex] = group;

        }
        
        public int AddSystemGroup(ref SystemGroup group) {

            var index = this.systemGroups.Length;
            ArrayUtils.Resize(in index, ref this.systemGroups, resizeWithOffset: false);
            this.systemGroups.arr[index] = group;
            return index;

        }
        
        #if WORLD_THREAD_CHECK
        public void SetWorldThread(System.Threading.Thread thread) {

            this.worldThread = thread;

        }
        #endif
        
        void IPoolableSpawn.OnSpawn() {

            #if WORLD_THREAD_CHECK
            this.worldThread = System.Threading.Thread.CurrentThread;
            #endif

            #if UNITY_EDITOR
            Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobDebuggerEnabled = false;
            #endif
            
            this.currentSystemContextFiltersUsed = PoolArray<bool>.Spawn(World.FILTERS_CACHE_CAPACITY);
            
            this.currentState = default;
            this.resetState = default;
            this.currentStep = default;
            this.checkpointCollector = default;
            this.tickTime = default;
            this.timeSinceStart = default;
            this.entitiesCapacity = default;
            
            this.features = PoolList<IFeatureBase>.Spawn(World.FEATURES_CAPACITY);
            //this.systems = PoolList<ISystemBase>.Spawn(World.SYSTEMS_CAPACITY);
            this.modules = PoolList<IModuleBase>.Spawn(World.MODULES_CAPACITY);
            this.statesFeatures = PoolList<ModuleState>.Spawn(World.FEATURES_CAPACITY);
            //this.statesSystems = PoolList<ModuleState>.Spawn(World.SYSTEMS_CAPACITY);
            this.statesModules = PoolList<ModuleState>.Spawn(World.MODULES_CAPACITY);
            
            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);

            this.OnSpawnStructComponents();
            this.OnSpawnComponents();
            this.OnSpawnMarkers();

            this.isActive = true;

        }

        public void RecycleResetState<TState>() where TState : State, new() {
            
            WorldUtilities.ReleaseState<TState>(ref this.resetState);
            
        }

        public void RecycleStates<TState>() where TState : State, new() {
            
            WorldUtilities.ReleaseState<TState>(ref this.currentState);

        }
        
        void IPoolableRecycle.OnRecycle() {
            
            if (this.ForEachEntity(out var entities) == true) {

                for (int i = entities.FromIndex; i < entities.SizeCount; ++i) {
                    
                    if (entities.IsFree(i) == true) continue;
                    this.RemoveEntity(entities[i]);

                }
                
            }

            PoolArray<bool>.Recycle(ref this.currentSystemContextFiltersUsed);
            
            #if WORLD_THREAD_CHECK
            this.worldThread = null;
            #endif
            this.isActive = false;

            this.OnRecycleMarkers();
            this.OnRecycleComponents();
            this.OnRecycleStructComponents();
            
            PoolArray<bool>.Recycle(ref FiltersDirectCache.dic.arr[this.id]);
            
            PoolList<IFeatureBase>.Recycle(ref this.features);

            for (int i = 0; i < this.systemGroups.Length; ++i) {

                this.systemGroups.arr[i].Deconstruct();

            }
            
            for (int i = 0; i < this.modules.Count; ++i) {
                
                this.modules[i].OnDeconstruct();
                PoolModules.Recycle(this.modules[i]);

            }
            PoolList<IModuleBase>.Recycle(ref this.modules);
            
            PoolList<ModuleState>.Recycle(ref this.statesModules);
            PoolList<ModuleState>.Recycle(ref this.statesFeatures);
            
            //PoolInternalBaseNoStackPool.Clear();
            //PoolInternalBase.Clear();
            
        }

        public void SetSettings(WorldSettings settings) {

            this.settings = settings;

        }

        public void SetDebugSettings(WorldDebugSettings settings) {
            
            this.debugSettings = settings;
            
        }

        public void SetCheckpointCollector(ICheckpointCollector checkpointCollector) {

            this.checkpointCollector = checkpointCollector;

        }

        public void Checkpoint(object interestObj) {
            
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(interestObj, this.currentStep);
            #endif
            
        }

        public void SetFeatureState(IFeatureBase feature, ModuleState state) {
            
            var index = this.features.IndexOf(feature);
            if (index >= 0) {

                this.statesFeatures[index] = state;

            }
            
        }
        
        public ModuleState GetFeatureState(IFeatureBase feature) {

            var index = this.features.IndexOf(feature);
            if (index >= 0) {

                return this.statesFeatures[index];

            }

            return ModuleState.AllActive;

        }

        public void SetSystemState(ISystemBase system, ModuleState state) {

            for (int i = 0; i < this.systemGroups.Length; ++i) {

                if (this.systemGroups.arr[i].SetSystemState(system, state) == true) {

                    return;

                }

            }
            
        }
        
        public ModuleState GetSystemState(ISystemBase system) {

            for (int i = 0; i < this.systemGroups.Length; ++i) {

                if (this.systemGroups.arr[i].GetSystemState(system, out var state) == true) {

                    return state;

                }

            }
            
            return ModuleState.AllActive;

        }

        public void SetModuleState(IModuleBase module, ModuleState state) {
            
            var index = this.modules.IndexOf(module);
            if (index >= 0) {

                this.statesModules[index] = state;

            }
            
        }
        
        public ModuleState GetModuleState(IModuleBase module) {

            var index = this.modules.IndexOf(module);
            if (index >= 0) {

                return this.statesModules[index];

            }

            return ModuleState.AllActive;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetId(int forcedWorldId = 0) {

            if (forcedWorldId > 0) {

                this.id = forcedWorldId;

            } else {
                
                this.id = ++World.registryWorldId;
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public UnityEngine.Vector3 GetRandomInSphere(UnityEngine.Vector3 center, float maxRadius) {

            #if WORLD_THREAD_CHECK
            if (this.worldThread != System.Threading.Thread.CurrentThread) {
                
                WrongThreadException.Throw("Random", "this could cause sync problems");

            }
            #endif

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(this.currentState.randomState);
            var dir = ((UnityEngine.Vector3)rnd.NextFloat3(-1f, 1f)).normalized;
            var spherePoint = dir * maxRadius;
            this.currentState.randomState = rnd.state;
            #else
            UnityEngine.Random.state = this.currentState.randomState;
            var spherePoint = UnityEngine.Random.insideUnitSphere * maxRadius;
            this.currentState.randomState = UnityEngine.Random.state;
            #endif
            return spherePoint + center;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetRandomRange(int from, int to) {

            #if WORLD_THREAD_CHECK
            if (this.worldThread != System.Threading.Thread.CurrentThread) {
                
                WrongThreadException.Throw("Random", "this could cause sync problems");

            }
            #endif

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(this.currentState.randomState);
            var result = rnd.NextInt(from, to);
            this.currentState.randomState = rnd.state;
            #else
            UnityEngine.Random.state = this.currentState.randomState;
            var result = UnityEngine.Random.Range(from, to);
            this.currentState.randomState = UnityEngine.Random.state;
            #endif
            return result;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float GetRandomRange(float from, float to) {

            #if WORLD_THREAD_CHECK
            if (this.worldThread != System.Threading.Thread.CurrentThread) {
                
                WrongThreadException.Throw("Random", "this could cause sync problems");

            }
            #endif

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(this.currentState.randomState);
            var result = rnd.NextFloat(from, to);
            this.currentState.randomState = rnd.state;
            #else
            UnityEngine.Random.state = this.currentState.randomState;
            var result = UnityEngine.Random.Range(from, to);
            this.currentState.randomState = UnityEngine.Random.state;
            #endif
            return result;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float GetRandomValue() {

            #if WORLD_THREAD_CHECK
            if (this.worldThread != System.Threading.Thread.CurrentThread) {
                
                WrongThreadException.Throw("Random", "this could cause sync problems");

            }
            #endif

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(this.currentState.randomState);
            var result = rnd.NextFloat(0f, 1f);
            this.currentState.randomState = rnd.state;
            #else
            UnityEngine.Random.state = this.currentState.randomState;
            var result = UnityEngine.Random.value;
            this.currentState.randomState = UnityEngine.Random.state;
            #endif
            return result;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetSeedValue() {

            return this.GetCurrentTick();
            
        }

        public void SetSeed(uint seed) {

            this.seed = seed;

            if (this.currentState != null) {

                #if UNITY_MATHEMATICS
                var rnd = new Unity.Mathematics.Random(this.seed);
                this.currentState.randomState = rnd.state;
                #else
                UnityEngine.Random.InitState((int)this.seed);
                this.currentState.randomState = UnityEngine.Random.state;
                #endif

            }

            if (this.resetState != null) {
                
                #if UNITY_MATHEMATICS
                var rnd = new Unity.Mathematics.Random(this.seed);
                this.resetState.randomState = rnd.state;
                #else
                UnityEngine.Random.InitState((int)this.seed);
                this.resetState.randomState = UnityEngine.Random.state;
                #endif

            }

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetTickTime(float tickTime) {

            this.tickTime = tickTime;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public float GetTickTime() {

            return this.tickTime;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Tick GetStateTick() {

            return this.GetState().tick;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public double GetTimeSinceStart() {

            return this.timeSinceStart;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetTimeSinceStart(double time) {

            this.timeSinceStart = time;

        }

        partial void OnSpawnMarkers();
        partial void OnRecycleMarkers();

        partial void OnSpawnComponents();
        partial void OnRecycleComponents();

        partial void OnSpawnStructComponents();
        partial void OnRecycleStructComponents();
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetStateHash() {

            if (this.statesHistoryModule != null) {

                return this.statesHistoryModule.GetStateHash(this.GetState());

            }

            return this.currentState.GetHash();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Filter GetFilter(in int id) {

            return this.currentState.filters.filters.arr[id - 1];

        }

        internal Filter GetFilterByHashCode(int hashCode) {

            return this.currentState.filters.GetByHashCode(hashCode);

        }

        public Filter GetFilterEquals(Filter other) {
            
            return this.currentState.filters.GetFilterEquals(other);
            
        }

        public bool HasFilter(Filter filterRef) {
            
            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {
            
                return dic.arr[filterRef.id - 1] == true;

            }

            return false;

        }

        public bool HasFilter(int id) {

            var idx = id - 1;
            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null && idx >= 0 && idx < dic.Length) {
            
                return dic.arr[idx] == true;

            }

            return false;

        }

        public void Register(Filter filterRef) {

            this.currentState.filters.Register(filterRef);

            ArrayUtils.Resize(filterRef.id, ref this.currentSystemContextFiltersUsed);

            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            ArrayUtils.Resize(filterRef.id - 1, ref dic);
            dic.arr[filterRef.id - 1] = true;

            if (this.entitiesCapacity > 0) filterRef.SetEntityCapacity(this.entitiesCapacity);
            
            if (this.ForEachEntity(out var allEntities) == true) {

                for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {
                    
                    ref var item = ref allEntities[j];
                    if (allEntities.IsFree(j) == true) continue;

                    ComponentsInitializerWorld.Init(in item);
                    this.UpdateFiltersOnFilterCreate(item);
                    
                }
                
            }

        }

        public void Register(ref FiltersStorage filtersRef, bool freeze, bool restore) {

            const int capacity = 10;
            if (filtersRef == null) {

                filtersRef = PoolClass<FiltersStorage>.Spawn();
                filtersRef.Initialize(capacity);
                filtersRef.SetFreeze(freeze);

            } else {

                filtersRef.SetFreeze(freeze);

            }
            
            if (freeze == false) {

                //this.filtersStorage = filtersRef;
                
                ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
                ref var dic = ref FiltersDirectCache.dic.arr[this.id];
                if (dic.arr != null) {
                    
                    System.Array.Clear(dic.arr, 0, dic.Length);
                    for (int i = 0; i < filtersRef.filters.Length; ++i) {

                        var filterRef = filtersRef.filters.arr[i];
                        if (filterRef != null) {
                        
                            ArrayUtils.Resize(filterRef.id - 1, ref dic);
                            dic.arr[filterRef.id - 1] = true;

                        }

                    }

                }
                
            }
            
        }

        public void Register(ref Components componentsRef, bool freeze, bool restore) {
            
            const int capacity = 4;
            if (componentsRef == null) {

                componentsRef = PoolClass<Components>.Spawn();
                componentsRef.Initialize(capacity);
                componentsRef.SetFreeze(freeze);

            } else {

                componentsRef.SetFreeze(freeze);

            }

            /*if (restore == true) {

                var data = componentsRef.GetData();
                foreach (var item in data) {

                    var components = item.Value;
                    for (int i = 0, count = components.Count; i < count; ++i) {

                        this.AddComponent<TEntity, IComponent<TState, TEntity>>(Entity.Create<TEntity>(item.Key), components[i]);

                    }

                }

            }*/

        }

        public void Register(ref Storage storageRef, bool freeze, bool restore) {

            this.RegisterPluginsModuleForEntity();

            if (storageRef == null) {
                
                storageRef = PoolClass<Storage>.Spawn();
                storageRef.Initialize(World.ENTITIES_CACHE_CAPACITY);
                storageRef.SetFreeze(freeze);
                
            }

            if (freeze == false) {

                if (this.sharedEntity.id == 0 && this.sharedEntityInitialized == false) {

                    // Create shared entity which should store shared components
                    this.sharedEntityInitialized = true;
                    this.sharedEntity = this.AddEntity();

                }

            }

            if (restore == true) {

                this.BeginRestoreEntities();
                
                // Update entities cache
                if (this.ForEachEntity(out var allEntities) == true) {

                    for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {
                    
                        ref var item = ref allEntities[j];
                        if (allEntities.IsFree(j) == true) continue;
                    
                        this.UpdateFilters(item);
                        this.CreateEntityPlugins(item);

                    }
                
                }
                
                this.EndRestoreEntities();

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasResetState() {

            return this.resetState != null;

        }

        public void SaveResetState<TState>() where TState : State, new() {

            if (this.resetState != null) WorldUtilities.ReleaseState<TState>(ref this.resetState);
            this.resetState = WorldUtilities.CreateState<TState>();
            this.resetState.Initialize(this, freeze: true, restore: false);
            this.resetState.CopyFrom(this.GetState());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public State GetResetState() {

            return this.resetState;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public T GetResetState<T>() where T : State {

            return (T)this.resetState;

        }

        public void SetState<TState>(State state) where TState : State, new() {

            //System.Array.Clear(this.storagesCache, 0, this.storagesCache.Length);
            //System.Array.Clear(this.componentsCache, 0, this.componentsCache.Length);
            
            if (this.currentState != null && this.currentState != state) WorldUtilities.ReleaseState<TState>(ref this.currentState);
            this.currentState = state;
            state.Initialize(this, freeze: false, restore: true);

            this.SetSeed(this.seed);
            
        }

        public void SetStateDirect(State state) {

            this.currentState = state;
            state.Initialize(this, freeze: false, restore: false);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public State GetState() {

            return this.currentState;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public T GetState<T>() where T : State {

            return (T)this.currentState;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public WorldStep GetCurrentStep() {

            return this.currentStep;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasStep(WorldStep step) {

            return (this.currentStep & step) != 0;

        }

        public void UpdateAllFilters() {
            
            var filters = this.currentState.filters.GetData();
            for (int i = 0; i < filters.Length; ++i) {
                
                filters.arr[i].Update();
                
            }
            
        }

        public void SetEntityCapacityInFilters(int capacity) {

            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    filter.SetEntityCapacity(capacity);

                }

            }

        }
        
        public void CreateEntityInFilters(Entity entity) {

            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    filter.OnEntityCreate(entity);

                }

            }

        }
        
        public void UpdateFiltersOnFilterCreate(Entity entity) {

            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    filter.OnEntityCreate(in entity);
                    if (filter.IsForEntity(in entity.id) == false) continue;
                    filter.OnUpdate(in entity);

                }

            }

        }
        
        public void UpdateFilters(Entity entity) {

            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    if (filter.IsForEntity(in entity.id) == false) continue;
                    filter.OnUpdate(in entity);

                }

            }

        }

        public void AddComponentToFilter(Entity entity) {
            
            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    if (filter.IsForEntity(in entity.id) == false) continue;
                    filter.OnAddComponent(in entity);

                }

            }
            
        }

        public void RemoveComponentFromFilter(in Entity entity) {
            
            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    if (filter.IsForEntity(in entity.id) == false) continue;
                    filter.OnRemoveComponent(in entity);

                }

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveFromFilters_INTERNAL(Entity entity) {
            
            ArrayUtils.Resize(this.id, ref FiltersDirectCache.dic);
            ref var dic = ref FiltersDirectCache.dic.arr[this.id];
            if (dic.arr != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic.arr[i] == false) continue;
                    var filterId = i + 1;
                    var filter = this.GetFilter(in filterId);
                    filter.OnEntityDestroy(in entity);
                    if (filter.IsForEntity(in entity.id) == false) continue;
                    filter.OnRemoveEntity(in entity);

                }

            }
            
        }

        /*public void AddToFilters<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            HashSet<int> filters;
            if (dic.TryGetValue(this.id, out filters) == true) {

                foreach (var filterId in filters) {

                    ((IFilterInternal)this.GetFilter<TEntity>(filterId)).OnAdd(entity);

                }

            }
            
        }*/

        public void RemoveFromFilters(Entity data) {

            this.RemoveFromFilters_INTERNAL(data);

        }

        public bool IsAlive(in int entityId, in ushort version) {

            if (version == 0) return false;
            
            ref var entitiesList = ref this.currentState.storage.GetData();
            if (entitiesList[entityId].version == version && entitiesList.IsFree(entityId) == false) {

                return true;

            }

            return false;

        }

        public ref Entity GetEntityById(in int id) {
            
            ref var entitiesList = ref this.currentState.storage.GetData();
            ref var ent = ref entitiesList[id];
            if (this.IsAlive(in ent.id, in ent.version) == false) return ref Entity.Empty;
            
            return ref ent;

        }

        public void SetEntitiesCapacity(int capacity) {

            this.entitiesCapacity = capacity;
            this.SetEntityCapacityPlugins(capacity);
            this.SetEntityCapacityInFilters(capacity);
            
        }
        
        public Entity AddEntity(string name = null) {

            ref var entitiesList = ref this.currentState.storage.GetData();
            var nextIndex = entitiesList.GetNextIndex();
            var ent = entitiesList[nextIndex];
            var entity = new Entity(nextIndex, (ushort)(ent.version + 1));
            entitiesList.Add(entity);

            this.CreateEntityPlugins(entity);
            this.CreateEntityInFilters(entity);
            this.UpdateFilters(entity);
            
            if (name != null) {

                entity.SetData(new ME.ECS.Name.Name() {
                    value = name
                });

            }

            return entity;

        }

        public bool ForEachEntity(out RefList<Entity> output) {

            output = this.currentState.storage.GetData();
            return output != null;
            
        }

        /*public bool HasEntity<TEntity>(int entityId) where TEntity : struct, IEntity {
            
            var key = MathUtils.GetKey(this.id, entityId);
            return EntitiesDirectCache<TState, TEntity>.dataByint.ContainsKey(key);

        }*/

        /*public void RemoveEntities<TEntity>(TEntity data) where TEntity : struct, IEntity {

            var key = MathUtils.GetKey(this.id, data.entity.id);
            if (EntitiesDirectCache<TState, TEntity>.dataByint.Remove(key) == true) {

                this.RemoveFromFilters(data);
                
                RefList<TEntity> listEntities;
                if (EntitiesDirectCache<TState, TEntity>.entitiesList.TryGetValue(this.id, out listEntities) == true) {
                
                    this.DestroyEntityPlugins<TEntity>(data.entity);
                    listEntities.Remove(data);
                    this.RemoveComponents(data.entity);

                }

            }

        }*/

        public bool RemoveEntity(Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.version == 0) {

                EmptyEntityException.Throw();

            }
            #endif
            
            var data = this.currentState.storage.GetData();
            if (data.IsFree(entity.id) == false) {

                //var entityInStorage = data[entity.id];
                /*if (entityInStorage.version == entity.version)*/ {

                    data.RemoveAt(entity.id);

                    this.RemoveFromFilters(entity);
                    this.DestroyEntityPlugins(entity);
                    this.RemoveComponents(entity);
                    return true;

                }

            }

            return false;

        }
        
        /// <summary>
        /// Get first module by type
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns></returns>
        public TModule GetModule<TModule>() where TModule : IModuleBase {

            if (this.modules == null) return default;
            
            for (int i = 0, count = this.modules.Count; i < count; ++i) {

                var module = this.modules[i];
                if (module is TModule tModule) {

                    return tModule;

                }

            }

            return default;

        }
        
        public List<TModule> GetModules<TModule>(List<TModule> output) where TModule : IModuleBase {

            output.Clear();
            for (int i = 0, count = this.modules.Count; i < count; ++i) {

                var module = this.modules[i];
                if (module is TModule tModule) {

                    output.Add(tModule);

                }

            }

            return output;

        }

        public bool HasModule<TModule>() where TModule : class, IModuleBase {

            for (int i = 0, count = this.modules.Count; i < count; ++i) {

                var module = this.modules[i];
                if (module is TModule) {

                    return true;

                }

            }

            return false;

        }

        /// <summary>
        /// Add module by type
        /// Retrieve module from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns></returns>
        public bool AddModule<TModule>() where TModule : class, IModuleBase, new() {
            
            WorldUtilities.SetWorld(this);
            
            var instance = PoolModules.Spawn<TModule>();
            instance.world = this;
            if (instance is IModuleValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {

                    instance.world = null;
                    PoolModules.Recycle(ref instance);
                    //throw new System.Exception("Couldn't add new module `" + instanceValidate + "`(" + nameof(TModule) + ") because of CouldBeAdded() returns false.");
                    return false;
                    
                }

            }
            
            this.modules.Add(instance);
            this.statesModules.Add(ModuleState.AllActive);
            instance.OnConstruct();

            return true;

        }

        /// <summary>
        /// Remove modules by type
        /// Return modules into pool, OnDeconstruct() call
        /// </summary>
        public void RemoveModules<TModule>() where TModule : class, IModuleBase, new() {

            for (int i = 0, count = this.modules.Count; i < count; ++i) {

                var module = this.modules[i];
                if (module is TModule tModule) {

                    PoolModules.Recycle(tModule);
                    this.modules.RemoveAt(i);
                    this.statesModules.RemoveAt(i);
                    module.OnDeconstruct();
                    --i;
                    --count;

                }

            }

        }
        
        public bool HasFeature<TFeature>() where TFeature : class, IFeatureBase, new() {

            for (int i = 0, count = this.features.Count; i < count; ++i) {

                if (this.features[i] is TFeature) return true;

            }

            return false;

        }

        public TFeature GetFeature<TFeature>() where TFeature : IFeatureBase {

            for (int i = 0, count = this.features.Count; i < count; ++i) {

                if (this.features[i] is TFeature) return (TFeature)this.features[i];

            }

            return default;

        }

        /// <summary>
        /// Add feature manually
        /// Pool will not be used, OnConstruct() call
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        public bool AddFeature(IFeatureBase instance) {

            WorldUtilities.SetWorld(this);
            
            instance.world = this;
            if (instance is IFeatureValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {
                    
                    instance.world = null;
                    return false;
                    
                }

            }
            
            this.features.Add(instance);
            this.statesFeatures.Add(ModuleState.AllActive);
            ((FeatureBase)instance).DoConstruct();

            return true;

        }

        /// <summary>
        /// Remove feature manually
        /// Pool will not be used, OnDeconstruct() call
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveFeature(IFeatureBase instance) {

            if (this.isActive == false) return;
            
            var idx = this.features.IndexOf(instance);
            if (idx >= 0) {
                
                this.features.RemoveAt(idx);
                this.statesFeatures.RemoveAt(idx);
                ((FeatureBase)instance).DoDeconstruct();
                
            }
            
        }

        #region Systems
        /// <summary>
        /// Search TSystem in all groups
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public bool HasSystem<TSystem>() where TSystem : class, ISystemBase, new() {

            for (int i = 0; i < this.systemGroups.Length; ++i) {

                if (this.systemGroups.arr[i].HasSystem<TSystem>() == true) {

                    return true;

                }
                
            }
            
            return false;

        }

        /// <summary>
        /// Get first system by type
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public TSystem GetSystem<TSystem>() where TSystem : class, ISystemBase {

            for (int i = 0, count = this.systemGroups.Length; i < count; ++i) {

                var system = this.systemGroups.arr[i].GetSystem<TSystem>();
                if (system != null) return system;

            }

            return default;

        }
        #endregion
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool IsModuleActive(int index) {

            var step = this.currentStep;
            if ((step & WorldStep.LogicTick) != 0) {

                return (this.statesModules[index] & ModuleState.LogicInactive) == 0;

            } else if ((step & WorldStep.VisualTick) != 0) {

                return (this.statesModules[index] & ModuleState.VisualInactive) == 0;

            }

            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UpdatePhysics(float deltaTime) {
        
            for (int i = 0, count = this.modules.Count; i < count; ++i) {

                if (this.IsModuleActive(i) == true) {

                    if (this.modules[i] is IModulePhysicsUpdate module) {
                        
                        module.UpdatePhysics(deltaTime);
                        
                    }
                    
                }

            }
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UpdateLogic(float deltaTime) {
            
            if (deltaTime < 0f) return;

            ////////////////
            // Update Logic Tick
            ////////////////
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("Simulate", WorldStep.None);
            #endif

            this.Simulate(this.simulationFromTick, this.simulationToTick);

            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("Simulate", WorldStep.None);
            #endif

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UpdateVisualPre(float deltaTime) {
            
            if (deltaTime < 0f) return;

            var state = this.GetState();

            ////////////////
            this.currentStep |= WorldStep.ModulesVisualTick;
            ////////////////
            {

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules, WorldStep.VisualTick);
                #endif

                for (int i = 0, count = this.modules.Count; i < count; ++i) {

                    if (this.IsModuleActive(i) == true) {
                        
                        #if CHECKPOINT_COLLECTOR
                        if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules[i], WorldStep.VisualTick);
                        #endif

                        if (this.modules[i] is IUpdate moduleBase) {

                            moduleBase.Update(in deltaTime);

                        }

                        #if CHECKPOINT_COLLECTOR
                        if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules[i], WorldStep.VisualTick);
                        #endif

                    }

                }

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules, WorldStep.VisualTick);
                #endif

            }
            ////////////////
            this.currentStep &= ~WorldStep.ModulesVisualTick;
            ////////////////

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UpdateVisualPost(float deltaTime) {
            
            if (deltaTime < 0f) return;

            var state = this.GetState();

            ////////////////
            this.currentStep |= WorldStep.SystemsVisualTick;
            ////////////////
            {

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systemGroups.arr, WorldStep.VisualTick);
                #endif

                for (int i = 0, count = this.systemGroups.Length; i < count; ++i) {

                    ref var group = ref this.systemGroups.arr[i];
                    for (int j = 0; j < group.systems.Length; ++j) {

                        if (group.IsSystemActive(j) == true) {

                            ref var system = ref group.systems.arr[j];
                            
                            #if CHECKPOINT_COLLECTOR
                            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(system, WorldStep.VisualTick);
                            #endif

                            if (system is IUpdate sysBase) {

                                sysBase.Update(in deltaTime);

                            }

                            #if CHECKPOINT_COLLECTOR
                            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(system, WorldStep.VisualTick);
                            #endif

                        }

                    }

                }

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systemGroups.arr, WorldStep.VisualTick);
                #endif

            }
            ////////////////
            this.currentStep &= ~WorldStep.SystemsVisualTick;
            ////////////////
            
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveMarkers", WorldStep.None);
            #endif

            this.RemoveMarkers();
            
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveMarkers", WorldStep.None);
            #endif

        }

        #if TICK_THREADED
        private struct UpdateJob : Unity.Jobs.IJobParallelFor {

            public float deltaTime;

            void Unity.Jobs.IJobParallelFor.Execute(int index) {

                Worlds.currentWorld.UpdateLogic(this.deltaTime);

            }

        }
        #endif
        
        public void PreUpdate(float deltaTime) {
            
            if (deltaTime < 0f) return;

            this.UpdateVisualPre(deltaTime);

        }

        public void Update(float deltaTime) {

            if (deltaTime < 0f) return;

            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Reset();
            #endif

            // Setup current static variables
            WorldUtilities.SetWorld(this);
            Worlds.currentState = this.GetState();

            // Update time
            this.timeSinceStart += deltaTime;
            if (this.timeSinceStart < 0d) this.timeSinceStart = 0d;

            #if TICK_THREADED
            var job = new UpdateJob() {
                deltaTime = deltaTime,
            };
            var jobHandle = job.Schedule(1, 64);
            jobHandle.Complete();
            #else
            this.UpdateLogic(deltaTime);
            #endif

        }

        public void LateUpdate(float deltaTime) {
            
            this.UpdateVisualPost(deltaTime);
            
        }

        public void SetFromToTicks(Tick from, Tick to) {

            //UnityEngine.Debug.Log("Set FromTo: " + from + " >> " + to);
            this.simulationFromTick = from;
            this.simulationToTick = to;

        }

        private struct ForeachFilterJob : Unity.Jobs.IJobParallelFor {

            public Unity.Collections.NativeArray<Entity> entities;
            public float deltaTime;

            void Unity.Jobs.IJobParallelFor.Execute(int index) {

                if (Worlds.currentWorld.currentSystemContext is ISystemFilter systemContextBase) {

                    systemContextBase.AdvanceTick(this.entities[index], in this.deltaTime);
                    
                }

            }

        }

        /*[Unity.Burst.BurstCompileAttribute]
        private unsafe struct ForeachFilterJobBurst : Unity.Jobs.IJobParallelFor {

            [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestrictionAttribute]
            public void* bws;
            public Unity.Burst.FunctionPointer<SystemFilterAdvanceTick> function;
            public Unity.Collections.NativeArray<Entity> entities;
            public float deltaTime;

            void Unity.Jobs.IJobParallelFor.Execute(int index) {

                this.function.Invoke(this.entities[index], this.deltaTime, this.bws);
                
            }

        }

        public unsafe delegate void SystemFilterAdvanceTick(in Entity entity, in float deltaTime, void* burstWorldStructComponentsAccess);*/

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Simulate(Tick from, Tick to) {
            
            this.PlayTasksForFrame();

            if (from > to) {

                //UnityEngine.Debug.LogError( UnityEngine.Time.frameCount + " From: " + from + ", To: " + to);
                return;

            }

            var state = this.GetState();

            //UnityEngine.Debug.Log("Simulate " + from + " to " + to);
            this.cpf = to - from;
            var fixedDeltaTime = this.GetTickTime();
            for (state.tick = from; state.tick < to; ++state.tick) {

                ////////////////
                this.currentStep |= WorldStep.ModulesLogicTick;
                ////////////////
                {
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules, WorldStep.LogicTick);
                    #endif
                    
                    for (int i = 0, count = this.modules.Count; i < count; ++i) {

                        if (this.IsModuleActive(i) == true) {

                            #if CHECKPOINT_COLLECTOR
                            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules[i], WorldStep.LogicTick);
                            #endif
                            
                            if (this.modules[i] is IAdvanceTick moduleBase) {

                                moduleBase.AdvanceTick(in fixedDeltaTime);

                            }
                            
                            #if CHECKPOINT_COLLECTOR
                            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules[i], WorldStep.LogicTick);
                            #endif
                            
                        }

                    }
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.modules, WorldStep.LogicTick);
                    #endif

                }
                ////////////////
                this.currentStep &= ~WorldStep.ModulesLogicTick;
                ////////////////
                
                ////////////////
                this.currentStep |= WorldStep.PluginsLogicTick;
                ////////////////
                {
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("PlayPluginsForTick", WorldStep.None);
                    #endif

                    this.PlayPluginsForTick(state.tick);
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("PlayPluginsForTick", WorldStep.None);
                    #endif

                }
                ////////////////
                this.currentStep &= ~WorldStep.PluginsLogicTick;
                ////////////////
                
                ////////////////
                this.currentStep |= WorldStep.SystemsLogicTick;
                ////////////////
                {
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systemGroups.arr, WorldStep.LogicTick);
                    #endif

                    this.PlayTasksForTick();

                    for (int i = 0, count = this.systemGroups.Length; i < count; ++i) {

                        ref var group = ref this.systemGroups.arr[i];
                        for (int j = 0; j < group.systems.Length; ++j) {

                            if (group.IsSystemActive(j) == true) {

                                ref var system = ref group.systems.arr[j];
                                
                                #if CHECKPOINT_COLLECTOR
                                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(system, WorldStep.LogicTick);
                                #endif

                                this.currentSystemContext = system;
                                System.Array.Clear(this.currentSystemContextFiltersUsed.arr, 0, this.currentSystemContextFiltersUsed.Length);
                                if (system is ISystemFilter sysFilter) {

                                    /*if (sysFilter is IAdvanceTickBurst advTick) {
    
                                        // Under the development process
                                        // This should not used right now
                                        
                                        var functionPointer = Unity.Burst.BurstCompiler.CompileFunctionPointer(advTick.GetAdvanceTickForBurst());
                                        var arrEntities = sysFilter.filter.GetArray();
                                        using (var arr = new Unity.Collections.NativeArray<Entity>(arrEntities.arr, Unity.Collections.Allocator.TempJob)) {
    
                                            var length = arrEntities.Length;
                                            var burstWorldStructComponentsAccess = new BurstWorldStructComponentsAccess();
                                            unsafe {
    
                                                var bws = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref burstWorldStructComponentsAccess);
                                                PoolArray<Entity>.Recycle(ref arrEntities);
                                                var job = new ForeachFilterJobBurst() {
                                                    deltaTime = fixedDeltaTime,
                                                    entities = arr,
                                                    function = functionPointer,
                                                    bws = bws
                                                };
                                                var jobHandle = job.Schedule(length, sysFilter.jobsBatchCount);
                                                jobHandle.Complete();
                                                
                                            }
    
                                        }
                                        
                                    }*/

                                    sysFilter.filter = (sysFilter.filter != null ? sysFilter.filter : sysFilter.CreateFilter());
                                    if (sysFilter.filter != null) {

                                        if (this.settings.useJobsForSystems == true && sysFilter.jobs == true) {

                                            var arrEntities = sysFilter.filter.GetArray();
                                            using (var arr = new Unity.Collections.NativeArray<Entity>(arrEntities.arr, Unity.Collections.Allocator.TempJob)) {

                                                var length = arrEntities.Length;
                                                PoolArray<Entity>.Recycle(ref arrEntities);
                                                var job = new ForeachFilterJob() {
                                                    deltaTime = fixedDeltaTime,
                                                    entities = arr
                                                };
                                                var jobHandle = job.Schedule(length, sysFilter.jobsBatchCount);
                                                jobHandle.Complete();

                                            }

                                        } else {

                                            if (sysFilter is ISystemFilter sysFilterContext) {

                                                foreach (ref var entity in sysFilter.filter) {

                                                    if (entity.version > 0) sysFilterContext.AdvanceTick(in entity, in fixedDeltaTime);

                                                }

                                            }

                                        }

                                    }

                                } else if (system is IAdvanceTick sysBase) {

                                    sysBase.AdvanceTick(in fixedDeltaTime);

                                }

                                this.currentSystemContext = null;

                                for (int f = 1, cnt = this.currentSystemContextFiltersUsed.Length; f < cnt; ++f) {

                                    if (this.currentSystemContextFiltersUsed.arr[f] == true) {

                                        var filter = this.GetFilter(in f);
                                        filter.ApplyAllRequests();

                                    }

                                }

                                #if CHECKPOINT_COLLECTOR
                                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(system, WorldStep.LogicTick);
                                #endif

                            }

                        }

                        #if CHECKPOINT_COLLECTOR
                        if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systemGroups.arr, WorldStep.LogicTick);
                        #endif

                    }
                    
                }
                ////////////////
                this.currentStep &= ~WorldStep.SystemsLogicTick;
                ////////////////

                this.currentState.storage.ApplyPrepared();

                /*#if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveComponentsOnce", WorldStep.None);
                #endif

                this.RemoveComponentsOnce<IComponentOnce>();

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveComponentsOnce", WorldStep.None);
                #endif*/

                this.UseLifetimeStep(ComponentLifetime.NotifyAllSystemsBelow);

            }
            
            ////////////////
            this.currentStep |= WorldStep.PluginsLogicSimulate;
            ////////////////
            {
                
                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("SimulatePluginsForTicks", WorldStep.None);
                #endif

                this.SimulatePluginsForTicks(from, to);
                
                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("SimulatePluginsForTicks", WorldStep.None);
                #endif

            }
            ////////////////
            this.currentStep &= ~WorldStep.PluginsLogicSimulate;
            ////////////////

            this.UseLifetimeStep(ComponentLifetime.NotifyAllModulesBelow);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void PlayPluginsForTick(Tick tick) {
            
            this.PlayPlugin1ForTick(tick);
            this.PlayPlugin2ForTick(tick);
            this.PlayPlugin3ForTick(tick);
            this.PlayPlugin4ForTick(tick);
            this.PlayPlugin5ForTick(tick);
            this.PlayPlugin6ForTick(tick);
            this.PlayPlugin7ForTick(tick);
            this.PlayPlugin8ForTick(tick);
            this.PlayPlugin9ForTick(tick);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void SimulatePluginsForTicks(Tick from, Tick to) {
            
            this.SimulatePlugin1ForTicks(from, to);
            this.SimulatePlugin2ForTicks(from, to);
            this.SimulatePlugin3ForTicks(from, to);
            this.SimulatePlugin4ForTicks(from, to);
            this.SimulatePlugin5ForTicks(from, to);
            this.SimulatePlugin6ForTicks(from, to);
            this.SimulatePlugin7ForTicks(from, to);
            this.SimulatePlugin8ForTicks(from, to);
            this.SimulatePlugin9ForTicks(from, to);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RegisterPluginsModuleForEntity() {
            
            this.RegisterPlugin1ModuleForEntity();
            this.RegisterPlugin2ModuleForEntity();
            this.RegisterPlugin3ModuleForEntity();
            this.RegisterPlugin4ModuleForEntity();
            this.RegisterPlugin5ModuleForEntity();
            this.RegisterPlugin6ModuleForEntity();
            this.RegisterPlugin7ModuleForEntity();
            this.RegisterPlugin8ModuleForEntity();
            this.RegisterPlugin9ModuleForEntity();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void DestroyEntityPlugins(in Entity entity) {
            
            this.DestroyEntityPlugin1(entity);
            this.DestroyEntityPlugin2(entity);
            this.DestroyEntityPlugin3(entity);
            this.DestroyEntityPlugin4(entity);
            this.DestroyEntityPlugin5(entity);
            this.DestroyEntityPlugin6(entity);
            this.DestroyEntityPlugin7(entity);
            this.DestroyEntityPlugin8(entity);
            this.DestroyEntityPlugin9(entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void BeginRestoreEntities() {
            
            this.BeginRestoreEntitiesPlugins();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void BeginRestoreEntitiesPlugins() {
            
            this.BeginRestoreEntitiesPlugin1();
            this.BeginRestoreEntitiesPlugin2();
            this.BeginRestoreEntitiesPlugin3();
            this.BeginRestoreEntitiesPlugin4();
            this.BeginRestoreEntitiesPlugin5();
            this.BeginRestoreEntitiesPlugin6();
            this.BeginRestoreEntitiesPlugin7();
            this.BeginRestoreEntitiesPlugin8();
            this.BeginRestoreEntitiesPlugin9();

        }

        partial void BeginRestoreEntitiesPlugin1();
        partial void BeginRestoreEntitiesPlugin2();
        partial void BeginRestoreEntitiesPlugin3();
        partial void BeginRestoreEntitiesPlugin4();
        partial void BeginRestoreEntitiesPlugin5();
        partial void BeginRestoreEntitiesPlugin6();
        partial void BeginRestoreEntitiesPlugin7();
        partial void BeginRestoreEntitiesPlugin8();
        partial void BeginRestoreEntitiesPlugin9();

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void EndRestoreEntities() {
            
            this.EndRestoreEntitiesPlugins();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void EndRestoreEntitiesPlugins() {
            
            this.EndRestoreEntitiesPlugin1();
            this.EndRestoreEntitiesPlugin2();
            this.EndRestoreEntitiesPlugin3();
            this.EndRestoreEntitiesPlugin4();
            this.EndRestoreEntitiesPlugin5();
            this.EndRestoreEntitiesPlugin6();
            this.EndRestoreEntitiesPlugin7();
            this.EndRestoreEntitiesPlugin8();
            this.EndRestoreEntitiesPlugin9();

        }

        partial void EndRestoreEntitiesPlugin1();
        partial void EndRestoreEntitiesPlugin2();
        partial void EndRestoreEntitiesPlugin3();
        partial void EndRestoreEntitiesPlugin4();
        partial void EndRestoreEntitiesPlugin5();
        partial void EndRestoreEntitiesPlugin6();
        partial void EndRestoreEntitiesPlugin7();
        partial void EndRestoreEntitiesPlugin8();
        partial void EndRestoreEntitiesPlugin9();

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void SetEntityCapacityPlugins(in int capacity) {
            
            this.SetEntityCapacityPlugin1(capacity);
            this.SetEntityCapacityPlugin2(capacity);
            this.SetEntityCapacityPlugin3(capacity);
            this.SetEntityCapacityPlugin4(capacity);
            this.SetEntityCapacityPlugin5(capacity);
            this.SetEntityCapacityPlugin6(capacity);
            this.SetEntityCapacityPlugin7(capacity);
            this.SetEntityCapacityPlugin8(capacity);
            this.SetEntityCapacityPlugin9(capacity);

        }

        partial void SetEntityCapacityPlugin1(int capacity);
        partial void SetEntityCapacityPlugin2(int capacity);
        partial void SetEntityCapacityPlugin3(int capacity);
        partial void SetEntityCapacityPlugin4(int capacity);
        partial void SetEntityCapacityPlugin5(int capacity);
        partial void SetEntityCapacityPlugin6(int capacity);
        partial void SetEntityCapacityPlugin7(int capacity);
        partial void SetEntityCapacityPlugin8(int capacity);
        partial void SetEntityCapacityPlugin9(int capacity);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void CreateEntityPlugins(in Entity entity) {
            
            this.CreateEntityPlugin1(entity);
            this.CreateEntityPlugin2(entity);
            this.CreateEntityPlugin3(entity);
            this.CreateEntityPlugin4(entity);
            this.CreateEntityPlugin5(entity);
            this.CreateEntityPlugin6(entity);
            this.CreateEntityPlugin7(entity);
            this.CreateEntityPlugin8(entity);
            this.CreateEntityPlugin9(entity);

        }

        partial void CreateEntityPlugin1(Entity entity);
        partial void CreateEntityPlugin2(Entity entity);
        partial void CreateEntityPlugin3(Entity entity);
        partial void CreateEntityPlugin4(Entity entity);
        partial void CreateEntityPlugin5(Entity entity);
        partial void CreateEntityPlugin6(Entity entity);
        partial void CreateEntityPlugin7(Entity entity);
        partial void CreateEntityPlugin8(Entity entity);
        partial void CreateEntityPlugin9(Entity entity);

        partial void DestroyEntityPlugin1(Entity entity);
        partial void DestroyEntityPlugin2(Entity entity);
        partial void DestroyEntityPlugin3(Entity entity);
        partial void DestroyEntityPlugin4(Entity entity);
        partial void DestroyEntityPlugin5(Entity entity);
        partial void DestroyEntityPlugin6(Entity entity);
        partial void DestroyEntityPlugin7(Entity entity);
        partial void DestroyEntityPlugin8(Entity entity);
        partial void DestroyEntityPlugin9(Entity entity);

        partial void RegisterPlugin1ModuleForEntity();
        partial void RegisterPlugin2ModuleForEntity();
        partial void RegisterPlugin3ModuleForEntity();
        partial void RegisterPlugin4ModuleForEntity();
        partial void RegisterPlugin5ModuleForEntity();
        partial void RegisterPlugin6ModuleForEntity();
        partial void RegisterPlugin7ModuleForEntity();
        partial void RegisterPlugin8ModuleForEntity();
        partial void RegisterPlugin9ModuleForEntity();
        
        partial void PlayPlugin1ForTick(Tick tick);
        partial void PlayPlugin2ForTick(Tick tick);
        partial void PlayPlugin3ForTick(Tick tick);
        partial void PlayPlugin4ForTick(Tick tick);
        partial void PlayPlugin5ForTick(Tick tick);
        partial void PlayPlugin6ForTick(Tick tick);
        partial void PlayPlugin7ForTick(Tick tick);
        partial void PlayPlugin8ForTick(Tick tick);
        partial void PlayPlugin9ForTick(Tick tick);

        partial void SimulatePlugin1ForTicks(Tick from, Tick to);
        partial void SimulatePlugin2ForTicks(Tick from, Tick to);
        partial void SimulatePlugin3ForTicks(Tick from, Tick to);
        partial void SimulatePlugin4ForTicks(Tick from, Tick to);
        partial void SimulatePlugin5ForTicks(Tick from, Tick to);
        partial void SimulatePlugin6ForTicks(Tick from, Tick to);
        partial void SimulatePlugin7ForTicks(Tick from, Tick to);
        partial void SimulatePlugin8ForTicks(Tick from, Tick to);
        partial void SimulatePlugin9ForTicks(Tick from, Tick to);

    }

}