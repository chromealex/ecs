//#define TICK_THREADED
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define CHECKPOINT_COLLECTOR
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using EntityId = System.Int32;
using Tick = System.UInt64;
using RPCId = System.Int32;

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

    public class InStateException : System.Exception {

        public InStateException() : base("[World] Could not perform action because current step is in state (" + Worlds.currentWorld.GetCurrentStep().ToString() + ").") {}

    }

    public class OutOfStateException : System.Exception {

        public OutOfStateException() : base("[World] Could not perform action because current step is out of state (" + Worlds.currentWorld.GetCurrentStep().ToString() + ").") {}

    }

    public class AllocationException : System.Exception {

        public AllocationException() : base("Allocation not allowed!") {}

    }

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
        internal const int ENTITIES_CACHE_CAPACITY = 100;
        private const int STORAGES_CAPACITY = 100;
        private const int CAPACITIES_CAPACITY = 100;
        
        private static class EntitiesDirectCache<TStateInner, TEntity> where TEntity : struct, IEntity where TStateInner : class, IState<TState> {

            internal static RefList<TEntity>[] entitiesList;
            internal static TEntity defaultRef;

        }

        private static class FiltersDirectCache<TStateInner, TEntity> where TEntity : struct, IEntity where TStateInner : class, IState<TState> {

            internal static Dictionary<int, HashSet<int>> dic = new Dictionary<int, HashSet<int>>(4);

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
        private IList[] storagesCache;
        private FiltersStorage filtersStorage;

        private float tickTime;
        private double timeSinceStart;

        public World() {

            this.currentState = null;
            this.resetState = null;
            this.currentStep = WorldStep.None;

        }

        void IPoolableSpawn.OnSpawn() {

            this.features = PoolList<IFeatureBase>.Spawn(World<TState>.FEATURES_CAPACITY);
            this.systems = PoolList<ISystem<TState>>.Spawn(World<TState>.SYSTEMS_CAPACITY);
            this.modules = PoolList<IModule<TState>>.Spawn(World<TState>.MODULES_CAPACITY);
            this.statesFeatures = PoolList<ModuleState>.Spawn(World<TState>.FEATURES_CAPACITY);
            this.statesSystems = PoolList<ModuleState>.Spawn(World<TState>.SYSTEMS_CAPACITY);
            this.statesModules = PoolList<ModuleState>.Spawn(World<TState>.MODULES_CAPACITY);
            this.storagesCache = PoolArray<IList>.Spawn(World<TState>.STORAGES_CAPACITY);

            this.OnSpawnComponents();
            this.OnSpawnMarkers();

        }

        void IPoolableRecycle.OnRecycle() {

            this.OnRecycleMarkers();
            this.OnRecycleComponents();
            
            PoolClass<FiltersStorage>.Recycle(ref this.filtersStorage);
            
            WorldUtilities.ReleaseState(ref this.resetState);
            WorldUtilities.ReleaseState(ref this.currentState);

            for (int i = 0; i < this.features.Count; ++i) {
                
                this.features[i].OnDeconstruct();
                PoolFeatures.Recycle(this.features[i]);

            }
            PoolList<IFeatureBase>.Recycle(ref this.features);

            for (int i = 0; i < this.systems.Count; ++i) {
                
                this.systems[i].OnDeconstruct();
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
            
            PoolArray<IList>.Recycle(ref this.storagesCache);

        }

        public void SetCheckpointCollector(ICheckpointCollector checkpointCollector) {

            this.checkpointCollector = checkpointCollector;

        }

        public void Checkpoint(object interestObj) {
            
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint(interestObj, this.currentStep);
            #endif
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public EntityId GetLastEntityId() {

            return this.GetState().entityId;

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

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(this.currentState.randomState);
            var dir = ((UnityEngine.Vector3)rnd.NextFloat3(-1f, 1f)).normalized;
            var spherePoint = dir * maxRadius;
            this.currentState.randomState = rnd.state;
            #else
            UnityEngine.Random.state = this.currentState.randomState;
            var spherePoint = UnityEngine.Random.insideUnitSphere * radius;
            this.currentState.randomState = UnityEngine.Random.state;
            #endif
            return spherePoint + center;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetRandomRange(int from, int to) {

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

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool GetEntityData<TEntity>(Entity entity, out TEntity data) where TEntity : struct, IEntity {

            data = default;
            
            if (entity.id == 0) {

                return false;
                
            }

            ref var list = ref EntitiesDirectCache<TState, TEntity>.entitiesList;
            if (list == null || this.id < 0 || this.id >= list.Length || list[this.id].IsFree(entity.storageIdx) == true) return false;
            
            data = list[this.id][entity.storageIdx];
            return true;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref TEntity GetEntityDataRef<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            if (entity.id == 0) {

                return ref EntitiesDirectCache<TState, TEntity>.defaultRef;
                
            }
            
            ref var list = ref EntitiesDirectCache<TState, TEntity>.entitiesList;
            if (list == null || this.id < 0 || this.id >= list.Length || list[this.id].IsFree(entity.storageIdx) == true) {
                
                return ref EntitiesDirectCache<TState, TEntity>.defaultRef;
                
            }
            
            return ref list[this.id][entity.storageIdx];
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        int IWorldBase.GetStateHash() {

            if (this.statesHistoryModule != null) {

                return this.statesHistoryModule.GetStateHash(this.GetState());

            }

            return this.currentState.GetHash();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetCapacity<TEntity>(int capacity) where TEntity : struct, IEntity {

            EntityTypes<TEntity>.capacity = capacity;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public int GetCapacity<TEntity>() where TEntity : struct, IEntity {

            return EntityTypes<TEntity>.capacity;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal Filter<TState, TEntity> GetFilter<TEntity>(int id) where TEntity : struct, IEntity {

            return (Filter<TState, TEntity>)this.filtersStorage.Get(id);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal IFilter<TState> GetFilter(int id) {

            return (IFilter<TState>)this.filtersStorage.Get(id);

        }

        public bool HasFilter<TEntity>(IFilter<TState, TEntity> filterRef) where TEntity : struct, IEntity {
            
            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            if (dic.TryGetValue(this.id, out HashSet<int> filters) == true) {

                return filters.Contains(filterRef.id);

            }

            return false;

        }

        public void Register<TEntity>(IFilter<TState, TEntity> filterRef) where TEntity : struct, IEntity {

            this.filtersStorage.Register(filterRef);

            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            if (dic.TryGetValue(this.id, out HashSet<int> filters) == true) {

                filters.Add(filterRef.id);

            } else {
                
                filters = new HashSet<int>();
                filters.Add(filterRef.id);
                dic.Add(this.id, filters);
                
            }

            if (this.ForEachEntity<TEntity>(out var allEntities) == true) {

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

            }
            
        }

        public void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : struct, IEntity {
            
            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            
            const int capacity = 4;
            if (componentsRef == null) {

                componentsRef = PoolClass<Components<TEntity, TState>>.Spawn();
                componentsRef.Initialize(capacity);
                componentsRef.SetFreeze(freeze);

            } else {

                componentsRef.SetFreeze(freeze);

            }

            if (freeze == false) {

                if (code < 0 || code >= this.componentsCache.Length) {
                
                    ArrayUtils.Resize(code, ref this.componentsCache);
                    
                }
                this.componentsCache[code] = componentsRef;
                
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

        public void Register<TEntity>(ref Storage<TEntity> storageRef, bool freeze, bool restore) where TEntity : struct, IEntity {

            this.RegisterPluginsModuleForEntity<TEntity>();

            var code = WorldUtilities.GetEntityTypeId<TEntity>();
            var capacity = this.GetCapacity<TEntity>();
            if (storageRef == null) {

                storageRef = PoolClass<Storage<TEntity>>.Spawn();
                storageRef.Initialize(capacity);
                storageRef.SetFreeze(freeze);

            } else {

                storageRef.SetFreeze(freeze);

            }

            if (freeze == false) {

                ArrayUtils.Resize(code, ref this.storagesCache);
                var list = this.storagesCache[code];
                if (list != null) {
                    
                    ((List<Storage<TEntity>>)list).Add(storageRef);
                    
                } else {
                    
                    list = PoolList<Storage<TEntity>>.Spawn(capacity);
                    ((List<Storage<TEntity>>)list).Add(storageRef);
                    this.storagesCache[code] = list;
                    
                }

            }

            if (this.sharedEntity.id == 0 && typeof(TEntity) == typeof(SharedEntity)) {
                
                // Create shared entity which should store shared components
                this.sharedEntity = this.AddEntity(new SharedEntity() { entity = Entity.Create<SharedEntity>(0, noCheck: true) }, updateStorages: false);

            }

            if (restore == true) {

                // Update entities cache
                for (int i = 0; i < storageRef.Count; ++i) {

                    var item = storageRef[i];
                    //var list = PoolList<TEntity>.Spawn(capacity);
                    //list.Add(item);
                    this.AddEntity(item, updateStorages: false);

                }

                this.UpdateStorages<TEntity>(code);

            }

        }

        public void UpdateStorages<TEntity>() where TEntity : struct, IEntity {
        
            this.UpdateStorages<TEntity>(WorldUtilities.GetEntityTypeId<TEntity>());
            
        }
        
        public void UpdateStorages<TEntity>(int code) where TEntity : struct, IEntity {

            if (code < 0 || code >= this.storagesCache.Length) return;
            
            ref var list = ref EntitiesDirectCache<TState, TEntity>.entitiesList;
            if (list == null || this.id < 0 || this.id >= list.Length) return;

            ref var listStorages = ref this.storagesCache[code];
            for (int i = 0, count = listStorages.Count; i < count; ++i) {

                var storage = (Storage<TEntity>)listStorages[i];
                storage.SetData(list[this.id]);

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

            System.Array.Clear(this.storagesCache, 0, this.storagesCache.Length);
            System.Array.Clear(this.componentsCache, 0, this.componentsCache.Length);

            if (this.currentState != null && this.currentState != state) WorldUtilities.ReleaseState(ref this.currentState);
            this.currentState = state;
            state.Initialize(this, freeze: false, restore: true);

            #if UNITY_MATHEMATICS
            var rnd = new Unity.Mathematics.Random(1u);
            state.randomState = rnd.state;
            #else
            UnityEngine.Random.InitState(0);
            state.randomState = UnityEngine.Random.state;
            #endif

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
        private Entity CreateNewEntity<T>() where T : IEntity {

            return Entity.Create<T>(++this.GetState().entityId);

        }

        public void UpdateEntityCache<TEntity>(TEntity data) where TEntity : struct, IEntity {

            /*ref var dic = ref EntitiesDirectCache<TState, TEntity>.dataByEntityId;
            var key = MathUtils.GetKey(this.id, data.entity.id);
            if (dic.ContainsKey(key) == true) {

                dic[key] = data;
                
            } else {

                dic.Add(key, data);

            }*/

            this.UpdateFilters(data);

        }

        public void UpdateFilters<TEntity>(TEntity data) where TEntity : struct, IEntity {

            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            HashSet<int> filters;
            if (dic.TryGetValue(this.id, out filters) == true) {

                foreach (var filterId in filters) {

                    ((IFilterInternal<TState>)this.GetFilter<TEntity>(filterId)).OnUpdate(data.entity);

                }

            }

        }

        public void AddComponentToFilter<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            HashSet<int> filters;
            if (dic.TryGetValue(this.id, out filters) == true) {

                foreach (var filterId in filters) {

                    ((IFilterInternal<TState>)this.GetFilter<TEntity>(filterId)).OnAddComponent(entity);

                }

            }
            
        }

        public void RemoveComponentFromFilter<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            HashSet<int> filters;
            if (dic.TryGetValue(this.id, out filters) == true) {

                foreach (var filterId in filters) {

                    ((IFilterInternal<TState>)this.GetFilter<TEntity>(filterId)).OnRemoveComponent(entity);

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

        public void RemoveFromFilters<TEntity>(TEntity data) where TEntity : struct, IEntity {
            
            this.RemoveFromFilters_INTERNAL<TEntity>(data.entity);
            
        }

        public void RemoveFromFilters<TEntity>(Entity data) where TEntity : struct, IEntity {

            this.RemoveFromFilters_INTERNAL<TEntity>(data);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveFromFilters_INTERNAL<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            ref var dic = ref FiltersDirectCache<TState, TEntity>.dic;
            HashSet<int> filters;
            if (dic.TryGetValue(this.id, out filters) == true) {

                foreach (var filterId in filters) {

                    ((IFilterInternal<TState>)this.GetFilter<TEntity>(filterId)).Remove_INTERNAL(entity);

                }

            }
            
        }

        public Entity AddEntity<TEntity>(TEntity data, bool updateStorages = true) where TEntity : struct, IEntity {

            if (data.entity.id == 0) data.entity = this.CreateNewEntity<TEntity>();

            RefList<TEntity> entitiesList;
            ArrayUtils.Resize(this.id, ref EntitiesDirectCache<TState, TEntity>.entitiesList);
            entitiesList = EntitiesDirectCache<TState, TEntity>.entitiesList[this.id];
            if (entitiesList == null) {
                
                EntitiesDirectCache<TState, TEntity>.entitiesList[this.id] = entitiesList = PoolRefList<TEntity>.Spawn(World<TState>.ENTITIES_CACHE_CAPACITY);
                
            }

            var nextIndex = entitiesList.GetNextIndex();
            var entity = data.entity;
            entity.storageIdx = nextIndex;
            data.entity = entity;
            entitiesList.Add(data);

            if (updateStorages == true) {

                var code = WorldUtilities.GetEntityTypeId<TEntity>();
                this.UpdateStorages<TEntity>(code);

            }

            //this.AddToFilters<TEntity>(data.entity); // Why we need to add empty entity into filters?
            this.UpdateEntityCache(data);

            return data.entity;

        }

        public bool ForEachEntity<TEntity>(out RefList<TEntity> output) where TEntity : struct, IEntity {

            output = null;

            ref var list = ref EntitiesDirectCache<TState, TEntity>.entitiesList;
            if (list == null || this.id < 0 || this.id >= list.Length) return false;

            output = list[this.id];
            return output != null;
            
        }

        /*public bool HasEntity<TEntity>(EntityId entityId) where TEntity : struct, IEntity {
            
            var key = MathUtils.GetKey(this.id, entityId);
            return EntitiesDirectCache<TState, TEntity>.dataByEntityId.ContainsKey(key);

        }*/

        /*public void RemoveEntities<TEntity>(TEntity data) where TEntity : struct, IEntity {

            var key = MathUtils.GetKey(this.id, data.entity.id);
            if (EntitiesDirectCache<TState, TEntity>.dataByEntityId.Remove(key) == true) {

                this.RemoveFromFilters(data);
                
                RefList<TEntity> listEntities;
                if (EntitiesDirectCache<TState, TEntity>.entitiesList.TryGetValue(this.id, out listEntities) == true) {
                
                    this.DestroyEntityPlugins<TEntity>(data.entity);
                    listEntities.Remove(data);
                    this.RemoveComponents(data.entity);

                }

            }

        }*/

        public bool RemoveEntity<TEntity>(Entity entity) where TEntity : struct, IEntity {

            ref var list = ref EntitiesDirectCache<TState, TEntity>.entitiesList;
            if (list == null || this.id < 0 || this.id >= list.Length) return false;

            list[this.id].RemoveAt(entity.storageIdx);
            
            this.DestroyEntityPlugins<TEntity>(entity);
            this.RemoveComponentsByEntityType<TEntity>(entity);
            this.RemoveFromFilters<TEntity>(entity);
            
            return false;

        }
        
        /// <summary>
        /// Get first module by type
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns></returns>
        public TModule GetModule<TModule>() where TModule : IModuleBase {

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
        /// Add feature by type
        /// Retrieve feature from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TFeature"></typeparam>
        public bool AddFeature<TFeature>() where TFeature : class, IFeature<TState, ConstructParameters>, new() {

            var instance = PoolFeatures.Spawn<TFeature>();
            var r = new ConstructParameters();
            if (this.AddFeature(instance, ref r) == false) {

                instance.world = null;
                PoolFeatures.Recycle(ref instance);
                return false;

            }

            return true;

        }

        public bool AddFeature<TFeature, TConstructParameters>(TConstructParameters parameters) where TFeature : class, IFeature<TState, TConstructParameters>, new() where TConstructParameters : IConstructParameters {

            return this.AddFeature<TFeature, TConstructParameters>(ref parameters);

        }

        /// <summary>
        /// Add feature by type
        /// Retrieve feature from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TFeature"></typeparam>
        /// <typeparam name="TConstructParameters"></typeparam>
        public bool AddFeature<TFeature, TConstructParameters>(ref TConstructParameters parameters) where TFeature : class, IFeature<TState, TConstructParameters>, new() where TConstructParameters : IConstructParameters {

            var instance = PoolFeatures.Spawn<TFeature>();
            if (this.AddFeature(instance, ref parameters) == false) {

                instance.world = null;
                PoolFeatures.Recycle(ref instance);
                return false;

            }

            return true;

        }

        /// <summary>
        /// Add feature manually
        /// Pool will not be used, OnConstruct() call
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parameters"></param>
        public bool AddFeature<TConstructParameters>(IFeature<TState, TConstructParameters> instance, ref TConstructParameters parameters) where TConstructParameters : IConstructParameters {

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
            instance.OnConstruct(ref parameters);

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
                instance.OnDeconstruct();
                
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
        public void UpdateLogic(float deltaTime, Tick prevTick) {
            
            if (deltaTime < 0f) return;

            var state = this.GetState();
            
            ////////////////
            // Update Logic Tick
            ////////////////
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("Simulate", WorldStep.None);
            #endif

            this.Simulate(prevTick + 1, state.tick + 1);

            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("Simulate", WorldStep.None);
            #endif

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UpdateVisualPre(float deltaTime) {
            
            if (deltaTime < 0f) return;

            var state = this.GetState();

            ////////////////
            this.currentStep = WorldStep.ModulesVisualTick;
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

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void UpdateVisualPost(float deltaTime) {
            
            if (deltaTime < 0f) return;

            var state = this.GetState();

            ////////////////
            this.currentStep = WorldStep.SystemsVisualTick;
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
            public Tick prevTick;

            void Unity.Jobs.IJobParallelFor.Execute(int index) {

                Worlds<TState>.currentWorld.UpdateLogic(this.deltaTime, this.prevTick);

            }

        }
        #endif
        
        public void Update(float deltaTime) {

            if (deltaTime < 0f) return;
            
            #if CHECKPOINT_COLLECTOR
            if (this.checkpointCollector != null) this.checkpointCollector.Reset();
            #endif

            var state = this.GetState();
            var prevTick = state.tick;
            
            // Setup current static variables
            WorldUtilities.SetWorld(this);
            Worlds<TState>.currentState = state;

            // Update time
            this.timeSinceStart += deltaTime;
            if (this.timeSinceStart < 0d) this.timeSinceStart = 0d;

            this.UpdateVisualPre(deltaTime);
            
            #if TICK_THREADED
            var job = new UpdateJob() {
                deltaTime = deltaTime,
                prevTick = prevTick,
            };
            var jobHandle = job.Schedule(1, 64);
            jobHandle.Complete();
            #else
            this.UpdateLogic(deltaTime, prevTick);
            #endif
            
            this.UpdateVisualPost(deltaTime);
            
            ////////////////
            this.currentStep = WorldStep.None;
            ////////////////
            
        }

        public void Simulate(Tick from, Tick to) {
            
            if (from > to) {

                //UnityEngine.Debug.LogError( UnityEngine.Time.frameCount + " From: " + from + ", To: " + to);
                return;

            }

            var state = this.GetState();
            
            var fixedDeltaTime = ((IWorldBase)this).GetTickTime();
            for (Tick tick = from; tick < to; ++tick) {

                state.tick = tick;
                
                ////////////////
                this.currentStep = WorldStep.ModulesLogicTick;
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
                            this.modules[i].AdvanceTick(state, fixedDeltaTime);
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
                this.currentStep = WorldStep.PluginsLogicTick;
                ////////////////
                {
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("PlayPluginsForTick", WorldStep.None);
                    #endif

                    this.PlayPluginsForTick(tick);
                    
                    #if CHECKPOINT_COLLECTOR
                    if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("PlayPluginsForTick", WorldStep.None);
                    #endif

                }
                ////////////////
                this.currentStep = WorldStep.SystemsLogicTick;
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

                            switch (this.systems[i]) {
                                
                                case ISystemFilter<TState> sysFilter: {

                                    var filter = (sysFilter.filter != null ? sysFilter.filter : sysFilter.CreateFilter());
                                    if (filter != null) {

                                        foreach (var entity in filter) {

                                            sysFilter.AdvanceTick(entity, state, fixedDeltaTime);

                                        }

                                    }

                                    break;
                                }

                                case ISystemAdvanceTick<TState> sys:
                                    
                                    sys.AdvanceTick(state, fixedDeltaTime);
                                    
                                    break;
                                
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

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveComponentsOnce", WorldStep.None);
                #endif

                this.RemoveComponentsOnce<IComponentOnceBase>();

                #if CHECKPOINT_COLLECTOR
                if (this.checkpointCollector != null) this.checkpointCollector.Checkpoint("RemoveComponentsOnce", WorldStep.None);
                #endif

            }
            
            ////////////////
            this.currentStep = WorldStep.PluginsLogicSimulate;
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
        private void RegisterPluginsModuleForEntity<TEntity>() where TEntity : struct, IEntity {
            
            this.RegisterPlugin1ModuleForEntity<TEntity>();
            this.RegisterPlugin2ModuleForEntity<TEntity>();
            this.RegisterPlugin3ModuleForEntity<TEntity>();
            this.RegisterPlugin4ModuleForEntity<TEntity>();
            this.RegisterPlugin5ModuleForEntity<TEntity>();
            this.RegisterPlugin6ModuleForEntity<TEntity>();
            this.RegisterPlugin7ModuleForEntity<TEntity>();
            this.RegisterPlugin8ModuleForEntity<TEntity>();
            this.RegisterPlugin9ModuleForEntity<TEntity>();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void DestroyEntityPlugins<TEntity>(Entity entity) where TEntity : struct, IEntity {
            
            this.DestroyEntityPlugin1<TEntity>(entity);
            this.DestroyEntityPlugin2<TEntity>(entity);
            this.DestroyEntityPlugin3<TEntity>(entity);
            this.DestroyEntityPlugin4<TEntity>(entity);
            this.DestroyEntityPlugin5<TEntity>(entity);
            this.DestroyEntityPlugin6<TEntity>(entity);
            this.DestroyEntityPlugin7<TEntity>(entity);
            this.DestroyEntityPlugin8<TEntity>(entity);
            this.DestroyEntityPlugin9<TEntity>(entity);

        }

        partial void DestroyEntityPlugin1<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin2<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin3<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin4<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin5<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin6<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin7<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin8<TEntity>(Entity entity) where TEntity : struct, IEntity;
        partial void DestroyEntityPlugin9<TEntity>(Entity entity) where TEntity : struct, IEntity;

        partial void RegisterPlugin1ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin2ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin3ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin4ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin5ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin6ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin7ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin8ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        partial void RegisterPlugin9ModuleForEntity<TEntity>() where TEntity : struct, IEntity;
        
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