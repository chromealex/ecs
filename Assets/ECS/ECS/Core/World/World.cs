//#define TICK_THREADED
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define CHECKPOINT_COLLECTOR
#endif
using System.Collections;
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
    public partial class World<TState> : IWorld<TState>, IPoolableSpawn, IPoolableRecycle where TState : class, IState<TState>, new() {

        private const int FEATURES_CAPACITY = 100;
        private const int SYSTEMS_CAPACITY = 100;
        private const int MODULES_CAPACITY = 100;
        private const int ENTITIES_CACHE_CAPACITY = 100;
        private const int WORLDS_CAPACITY = 4;
        private const int FILTERS_CACHE_CAPACITY = 10;
        
        private static class FiltersDirectCache<TStateInner> where TStateInner : class, IState<TState> {

            internal static bool[][] dic = new bool[World<TState>.WORLDS_CAPACITY][];

        }

        private static int registryWorldId = 0;

        public int id {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private set;
        }
        
        private TState resetState;
        private TState currentState;
        private WorldStep currentStep;
        private List<IFeatureBase> features;
        private List<ISystem<TState>> systems;
        private List<IModule<TState>> modules;

        private List<ModuleState> statesFeatures;
        private List<ModuleState> statesSystems;
        private List<ModuleState> statesModules;

        private ICheckpointCollector checkpointCollector;
        
        // State cache:
        internal Storage storagesCache;
        internal FiltersStorage filtersStorage;

        private float tickTime;
        private double timeSinceStart;
        public bool isActive;

        public ISystemBase currentSystemContext { get; internal set; }
        internal SortedList<int, IFilter<TState>> currentSystemContextFiltersUsed;
        
        private Tick simulationFromTick;
        private Tick simulationToTick;

        public WorldSettings settings { get; private set; }
        public WorldDebugSettings debugSettings { get; private set; }

        #if WORLD_THREAD_CHECK
        private System.Threading.Thread worldThread;
        #endif
        
        void IPoolableSpawn.OnSpawn() {

            #if WORLD_THREAD_CHECK
            this.worldThread = System.Threading.Thread.CurrentThread;
            #endif

            this.currentSystemContextFiltersUsed = PoolSortedList<int, IFilter<TState>>.Spawn(World<TState>.FILTERS_CACHE_CAPACITY);
            
            this.currentState = default;
            this.resetState = default;
            this.currentStep = default;
            this.checkpointCollector = default;
            this.filtersStorage = default;
            this.tickTime = default;
            this.timeSinceStart = default;
            
            this.features = PoolList<IFeatureBase>.Spawn(World<TState>.FEATURES_CAPACITY);
            this.systems = PoolList<ISystem<TState>>.Spawn(World<TState>.SYSTEMS_CAPACITY);
            this.modules = PoolList<IModule<TState>>.Spawn(World<TState>.MODULES_CAPACITY);
            this.statesFeatures = PoolList<ModuleState>.Spawn(World<TState>.FEATURES_CAPACITY);
            this.statesSystems = PoolList<ModuleState>.Spawn(World<TState>.SYSTEMS_CAPACITY);
            this.statesModules = PoolList<ModuleState>.Spawn(World<TState>.MODULES_CAPACITY);
            this.storagesCache = PoolClass<Storage>.Spawn();

            ArrayUtils.Resize(this.id, ref FiltersDirectCache<TState>.dic);

            this.OnSpawnStructComponents();
            this.OnSpawnComponents();
            this.OnSpawnMarkers();

            this.isActive = true;

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolSortedList<int, IFilter<TState>>.Recycle(ref this.currentSystemContextFiltersUsed);
            
            #if WORLD_THREAD_CHECK
            this.worldThread = null;
            #endif
            this.isActive = false;
            
            this.OnRecycleMarkers();
            this.OnRecycleComponents();
            this.OnRecycleStructComponents();
            
            if (FiltersDirectCache<TState>.dic[this.id] != null) PoolArray<bool>.Recycle(ref FiltersDirectCache<TState>.dic[this.id]);
            PoolClass<FiltersStorage>.Recycle(ref this.filtersStorage);
            
            WorldUtilities.ReleaseState(ref this.resetState);
            WorldUtilities.ReleaseState(ref this.currentState);

            /*for (int i = 0; i < this.features.Count; ++i) {
                
                this.features[i].DoDeconstruct();
                PoolFeatures.Recycle(this.features[i]);

            }*/
            PoolList<IFeatureBase>.Recycle(ref this.features);

            for (int i = 0; i < this.systems.Count; ++i) {
                
                this.systems[i].OnDeconstruct();
                if (this.systems[i] is ISystemFilter<TState> systemFilter) {

                    systemFilter.filter = null;

                }
                PoolSystems.Recycle(this.systems[i]);

            }
            PoolList<ISystem<TState>>.Recycle(ref this.systems);
            
            for (int i = 0; i < this.modules.Count; ++i) {
                
                this.modules[i].OnDeconstruct();
                PoolModules.Recycle(this.modules[i]);

            }
            PoolList<IModule<TState>>.Recycle(ref this.modules);
            
            PoolList<ModuleState>.Recycle(ref this.statesModules);
            PoolList<ModuleState>.Recycle(ref this.statesSystems);
            PoolList<ModuleState>.Recycle(ref this.statesFeatures);
            
            PoolClass<Storage>.Recycle(ref this.storagesCache);

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
            
            var index = this.systems.IndexOf((ISystem<TState>)system);
            if (index >= 0) {

                this.statesSystems[index] = state;

            }
            
        }
        
        public ModuleState GetSystemState(ISystemBase system) {

            var index = this.systems.IndexOf((ISystem<TState>)system);
            if (index >= 0) {

                return this.statesSystems[index];

            }

            return ModuleState.AllActive;

        }

        public void SetModuleState(IModuleBase module, ModuleState state) {
            
            var index = this.modules.IndexOf((IModule<TState>)module);
            if (index >= 0) {

                this.statesModules[index] = state;

            }
            
        }
        
        public ModuleState GetModuleState(IModuleBase module) {

            var index = this.modules.IndexOf((IModule<TState>)module);
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
                
                this.id = ++World<TState>.registryWorldId;
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public UnityEngine.Vector3 GetRandomInSphere(UnityEngine.Vector3 center, float maxRadius) {

            #if WORLD_THREAD_CHECK
            if (this.worldThread != System.Threading.Thread.CurrentThread) {
                
                throw new WrongThreadException("Can't use Random methods from non-world thread, this could cause sync problems.\nTurn off this check by disable WORLD_THREAD_CHECK.");

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
                
                throw new WrongThreadException("Can't use Random methods from non-world thread, this could cause sync problems.\nTurn off this check by disable WORLD_THREAD_CHECK.");

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
                
                throw new WrongThreadException("Can't use Random methods from non-world thread, this could cause sync problems.\nTurn off this check by disable WORLD_THREAD_CHECK.");

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
                
                throw new WrongThreadException("Can't use Random methods from non-world thread, this could cause sync problems.\nTurn off this check by disable WORLD_THREAD_CHECK.");

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

            return (int)this.GetCurrentTick();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void IWorldBase.SetTickTime(float tickTime) {

            this.tickTime = tickTime;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        float IWorldBase.GetTickTime() {

            return this.tickTime;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        Tick IWorldBase.GetStateTick() {

            return this.GetState().tick;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        double IWorldBase.GetTimeSinceStart() {

            return this.timeSinceStart;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void IWorldBase.SetTimeSinceStart(double time) {

            this.timeSinceStart = time;

        }

        partial void OnSpawnMarkers();
        partial void OnRecycleMarkers();

        partial void OnSpawnComponents();
        partial void OnRecycleComponents();

        partial void OnSpawnStructComponents();
        partial void OnRecycleStructComponents();
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        int IWorldBase.GetStateHash() {

            if (this.statesHistoryModule != null) {

                return this.statesHistoryModule.GetStateHash(this.GetState());

            }

            return this.currentState.GetHash();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Filter<TState> GetFilter(int id) {

            return (Filter<TState>)this.filtersStorage.filters[id - 1]; //.Get(id);

        }

        internal IFilter<TState> GetFilterByHashCode(int hashCode) {

            return (IFilter<TState>)this.filtersStorage.GetByHashCode(hashCode);

        }

        internal IFilter<TState> GetFilterEquals(IFilter<TState> other) {
            
            return (IFilter<TState>)this.filtersStorage.GetFilterEquals(other);
            
        }

        public bool HasFilter(IFilter<TState> filterRef) {
            
            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            if (dic != null) {
            
                return dic[filterRef.id - 1] == true;

            }

            return false;

        }

        public bool HasFilter(int id) {

            var idx = id - 1;
            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            if (dic != null && idx >= 0 && idx < dic.Length) {
            
                return dic[idx] == true;

            }

            return false;

        }

        public void Register(IFilter<TState> filterRef) {

            this.filtersStorage.Register(filterRef);

            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            ArrayUtils.Resize(filterRef.id - 1, ref dic);
            dic[filterRef.id - 1] = true;

            if (this.ForEachEntity(out var allEntities) == true) {

                for (int j = allEntities.FromIndex, jCount = allEntities.SizeCount; j < jCount; ++j) {
                    
                    ref var item = ref allEntities[j];
                    if (allEntities.IsFree(j) == true) continue;
                    
                    this.UpdateFilters(item);

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

                this.filtersStorage = filtersRef;
                
                ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
                if (dic != null) {
                    
                    System.Array.Clear(dic, 0, dic.Length);
                    for (int i = 0; i < filtersRef.filters.Length; ++i) {

                        var filterRef = filtersRef.filters[i];
                        if (filterRef != null) {
                        
                            ArrayUtils.Resize(filterRef.id - 1, ref dic);
                            dic[filterRef.id - 1] = true;

                        }

                    }

                }
                
            }
            
        }

        public void Register(ref Components<TState> componentsRef, bool freeze, bool restore) {
            
            const int capacity = 4;
            if (componentsRef == null) {

                componentsRef = PoolClass<Components<TState>>.Spawn();
                componentsRef.Initialize(capacity);
                componentsRef.SetFreeze(freeze);

            } else {

                componentsRef.SetFreeze(freeze);

            }

            if (freeze == false) {

                this.componentsCache = componentsRef;
                
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
                storageRef.Initialize(World<TState>.ENTITIES_CACHE_CAPACITY);
                storageRef.SetFreeze(freeze);
                
            }

            if (freeze == false) {

                this.storagesCache = storageRef;

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

        public void SaveResetState() {

            if (this.resetState != null) WorldUtilities.ReleaseState(ref this.resetState);
            this.resetState = WorldUtilities.CreateState<TState>();
            this.resetState.Initialize(this, freeze: true, restore: false);
            this.resetState.CopyFrom(this.GetState());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TState GetResetState() {

            return this.resetState;

        }

        public void SetState(TState state) {

            //System.Array.Clear(this.storagesCache, 0, this.storagesCache.Length);
            //System.Array.Clear(this.componentsCache, 0, this.componentsCache.Length);
            
            if (this.currentState != null && this.currentState != state) WorldUtilities.ReleaseState(ref this.currentState);
            this.currentState = state;
            state.Initialize(this, freeze: false, restore: true);

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(1u);
            state.randomState = rnd.state;
            #else
            UnityEngine.Random.InitState(1);
            state.randomState = UnityEngine.Random.state;
            #endif

        }

        public void SetStateDirect(TState state) {

            this.currentState = state;
            state.Initialize(this, freeze: false, restore: false);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TState GetState() {

            return this.currentState;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public WorldStep GetCurrentStep() {

            return this.currentStep;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasStep(WorldStep step) {

            return (this.currentStep & step) != 0;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private int CreateNewEntity() {

            return ++this.GetState().entityId; //Entity.Create(++this.GetState().entityId);

        }

        public void UpdateAllFilters() {
            
            var filters = this.filtersStorage.GetData();
            foreach (var filter in filters) {

                filter.Update();

            }

        }

        public void UpdateFilters(Entity entity) {

            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            if (dic != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic[i] == false) continue;
                    var filterId = i + 1;
                    var filter = (IFilterInternal<TState>)this.GetFilter(filterId);
                    if (filter.IsForEntity(entity) == false) continue;
                    filter.OnUpdate(entity);

                }

            }

        }

        public void AddComponentToFilter(Entity entity) {
            
            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            if (dic != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic[i] == false) continue;
                    var filterId = i + 1;
                    var filter = (IFilterInternal<TState>)this.GetFilter(filterId);
                    if (filter.IsForEntity(entity) == false) continue;
                    filter.OnAddComponent(entity);

                }

            }
            
        }

        public void RemoveComponentFromFilter(Entity entity) {
            
            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            if (dic != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic[i] == false) continue;
                    var filterId = i + 1;
                    var filter = (IFilterInternal<TState>)this.GetFilter(filterId);
                    if (filter.IsForEntity(entity) == false) continue;
                    filter.OnRemoveComponent(entity);

                }

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveFromFilters_INTERNAL(Entity entity) {
            
            ref var dic = ref FiltersDirectCache<TState>.dic[this.id];
            if (dic != null) {

                for (int i = 0; i < dic.Length; ++i) {

                    if (dic[i] == false) continue;
                    var filterId = i + 1;
                    var filter = (IFilterInternal<TState>)this.GetFilter(filterId);
                    if (filter.IsForEntity(entity) == false) continue;
                    filter.OnRemoveEntity(entity);

                }

            }
            
        }

        /*public void AddToFilters<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            HashSet<int> filters;
            if (dic.TryGetValue(this.id, out filters) == true) {

                foreach (var filterId in filters) {

                    ((IFilterInternal<TState>)this.GetFilter<TEntity>(filterId)).OnAdd(entity);

                }

            }
            
        }*/

        public void RemoveFromFilters(Entity data) {

            this.RemoveFromFilters_INTERNAL(data);

        }

        public Entity AddEntity(string name = null) {

            var entityVersion = this.CreateNewEntity();
            ref var entitiesList = ref this.storagesCache.GetData();
            var nextIndex = entitiesList.GetNextIndex();
            var entity = new Entity(nextIndex, entityVersion);
            entitiesList.Add(entity);

            //this.AddToFilters<TEntity>(data.entity); // Why we need to add empty entity into filters?
            this.CreateEntityPlugins(entity);
            this.UpdateFilters(entity);

            if (name != null) {

                entity.SetData(new ME.ECS.Name.Name() {
                    value = name
                });

            }

            return entity;

        }

        public bool ForEachEntity(out RefList<Entity> output) {

            output = this.storagesCache.GetData();
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

            var data = this.storagesCache.GetData();
            if (data.IsFree(entity.id) == false) {

                //var entityInStorage = data[entity.id];
                /*if (entityInStorage.version == entity.version)*/ {

                    data.RemoveAt(entity.id);

                    this.DestroyEntityPlugins(entity);
                    this.RemoveComponents(entity);
                    this.RemoveFromFilters(entity);
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

        public bool HasModule<TModule>() where TModule : class, IModule<TState> {

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
        public bool AddModule<TModule>() where TModule : class, IModule<TState>, new() {
            
            WorldUtilities.SetWorld(this);
            
            var instance = PoolModules.Spawn<TModule>();
            instance.world = this;
            if (instance is IModuleValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {

                    instance.world = null;
                    PoolModules.Recycle(ref instance);
                    throw new System.Exception("Couldn't add new module `" + instanceValidate + "`(" + nameof(TModule) + ") because of CouldBeAdded() returns false.");
                    //return false;
                    
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
        public void RemoveModules<TModule>() where TModule : class, IModule<TState>, new() {

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
        public bool AddFeature(IFeature<TState> instance) {

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

            var idx = this.features.IndexOf(instance);
            if (idx >= 0) {
                
                this.features.RemoveAt(idx);
                this.statesFeatures.RemoveAt(idx);
                ((FeatureBase)instance).DoDeconstruct();
                
            }
            
        }

        public bool HasSystem<TSystem>() where TSystem : class, ISystem<TState>, new() {

            for (int i = 0, count = this.systems.Count; i < count; ++i) {

                if (this.systems[i] is TSystem) return true;

            }

            return false;

        }

        /// <summary>
        /// Add system by type
        /// Retrieve system from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        public bool AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new() {

            var instance = PoolSystems.Spawn<TSystem>();
            if (this.AddSystem(instance) == false) {

                instance.world = null;
                PoolSystems.Recycle(ref instance);
                return false;

            }

            return true;

        }

        /// <summary>
        /// Add system manually
        /// Pool will not be used, OnConstruct() call
        /// </summary>
        /// <param name="instance"></param>
        public bool AddSystem(ISystem<TState> instance) {

            WorldUtilities.SetWorld(this);
            
            instance.world = this;
            if (instance is ISystemValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {
                    
                    instance.world = null;
                    return false;
                    
                }

            }
            
            this.systems.Add(instance);
            this.statesSystems.Add(ModuleState.AllActive);
            instance.OnConstruct();

            if (instance is ISystemFilter<TState> systemFilter) {

                systemFilter.filter = systemFilter.CreateFilter();

            }

            return true;

        }

        /// <summary>
        /// Remove system manually
        /// Pool will not be used, OnDeconstruct() call
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveSystem(ISystem<TState> instance) {

            var idx = this.systems.IndexOf(instance);
            if (idx >= 0) {
                
                if (instance is ISystemFilter<TState> systemFilter) {

                    systemFilter.filter = null;
                    
                }
                
                instance.world = null;
                this.systems.RemoveAt(idx);
                this.statesSystems.RemoveAt(idx);
                instance.OnDeconstruct();
                
            }
            
        }

        /// <summary>
        /// Get first system by type
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public TSystem GetSystem<TSystem>() where TSystem : ISystemBase {

            for (int i = 0, count = this.systems.Count; i < count; ++i) {

                var system = this.systems[i];
                if (system is TSystem tSystem) {

                    return tSystem;

                }

            }

            return default;

        }

        /// <summary>
        /// Remove systems by type
        /// Return systems into pool, OnDeconstruct() call
        /// </summary>
        public void RemoveSystems<TSystem>() where TSystem : class, ISystemBase, new() {

            for (int i = 0, count = this.systems.Count; i < count; ++i) {

                var system = this.systems[i];
                if (system is TSystem tSystem) {

                    PoolSystems.Recycle(tSystem);
                    this.systems.RemoveAt(i);
                    this.statesSystems.RemoveAt(i);
                    system.OnDeconstruct();
                    --i;
                    --count;

                }

            }

        }
        
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
        private bool IsSystemActive(int index) {

            var step = this.currentStep;
            if ((step & WorldStep.LogicTick) != 0) {

                return (this.statesSystems[index] & ModuleState.LogicInactive) == 0;

            } else if ((step & WorldStep.VisualTick) != 0) {

                return (this.statesSystems[index] & ModuleState.VisualInactive) == 0;

            }

            return false;

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

                        this.modules[i].Update(state, deltaTime);
                        
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
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems, WorldStep.VisualTick);
                #endif

                for (int i = 0, count = this.systems.Count; i < count; ++i) {

                    if (this.IsSystemActive(i) == true) {
                        
                        #if CHECKPOINT_COLLECTOR
                        if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems[i], WorldStep.VisualTick);
                        #endif

                        if (this.systems[i] is ISystemUpdate<TState> sys) sys.Update(state, deltaTime);

                        #if CHECKPOINT_COLLECTOR
                        if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems[i], WorldStep.VisualTick);
                        #endif
                        
                    }

                }

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems, WorldStep.VisualTick);
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

                Worlds<TState>.currentWorld.UpdateLogic(this.deltaTime);

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
            Worlds<TState>.currentState = this.GetState();

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

                var systemContext = Worlds.currentWorld.currentSystemContext as ISystemFilter<TState>;
                systemContext.AdvanceTick(this.entities[index], Worlds<TState>.currentWorld.GetState(), in this.deltaTime);
                
            }

        }

        public void Simulate(Tick from, Tick to) {
            
            if (from > to) {

                //UnityEngine.Debug.LogError( UnityEngine.Time.frameCount + " From: " + from + ", To: " + to);
                return;

            }

            var state = this.GetState();

            //UnityEngine.Debug.Log("Simulate " + from + " to " + to);
            var fixedDeltaTime = ((IWorldBase)this).GetTickTime();
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
                            this.modules[i].AdvanceTick(in state, in fixedDeltaTime);
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
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems, WorldStep.LogicTick);
                    #endif

                    for (int i = 0, count = this.systems.Count; i < count; ++i) {

                        if (this.IsSystemActive(i) == true) {
                            
                            #if CHECKPOINT_COLLECTOR
                            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems[i], WorldStep.LogicTick);
                            #endif

                            var system = this.systems[i];
                            this.currentSystemContext = system;
                            this.currentSystemContextFiltersUsed.Clear();
                            if (system is ISystemFilter<TState> sysFilter) {

                                sysFilter.filter = (sysFilter.filter != null ? sysFilter.filter : sysFilter.CreateFilter());
                                if (sysFilter.filter != null) {

                                    if (this.settings.useJobsForSystems == true && sysFilter.jobs == true) {

                                        //sysFilter.filter.SetForEachMode(true);
                                        var arrEntities = sysFilter.filter.GetArray();
                                        using (var arr = new Unity.Collections.NativeArray<Entity>(arrEntities, Unity.Collections.Allocator.TempJob)) {

                                            PoolArray<Entity>.Recycle(ref arrEntities);
                                            var job = new ForeachFilterJob() {
                                                deltaTime = fixedDeltaTime,
                                                entities = arr
                                            };
                                            var jobHandle = job.Schedule(sysFilter.filter.Count, sysFilter.jobsBatchCount);
                                            jobHandle.Complete();

                                        }
                                        //sysFilter.filter.SetForEachMode(false);
                                        
                                    } else {
                                        
                                        foreach (var entity in sysFilter.filter) {

                                            sysFilter.AdvanceTick(in entity, in state, in fixedDeltaTime);

                                        }

                                    }

                                }

                            }

                            if (system is ISystemAdvanceTick<TState> sys) {
                                
                                sys.AdvanceTick(in state, in fixedDeltaTime);
                                
                            }

                            this.currentSystemContext = null;

                            var usedFilters = this.currentSystemContextFiltersUsed.Values;
                            for (int f = 0, fCount = usedFilters.Count; f < fCount; ++f) {

                                var filter = usedFilters[f];
                                filter.ApplyAllRequests();

                            }

                            #if CHECKPOINT_COLLECTOR
                            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems[i], WorldStep.LogicTick);
                            #endif

                        }

                    }

                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(this.systems, WorldStep.LogicTick);
                    #endif

                }
                ////////////////
                this.currentStep &= ~WorldStep.SystemsLogicTick;
                ////////////////

                this.storagesCache.ApplyPrepared();

                /*#if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveComponentsOnce", WorldStep.None);
                #endif

                this.RemoveComponentsOnce<IComponentOnce<TState>>();

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveComponentsOnce", WorldStep.None);
                #endif*/

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