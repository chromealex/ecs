using System.Collections;
using System.Collections.Generic;
using EntityId = System.Int32;
using Tick = System.UInt64;
using RPCId = System.Int32;

namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World<TState> : IWorld<TState>, IPoolableSpawn, IPoolableRecycle where TState : class, IState<TState>, new() {

        private static int worldId = 0;

        internal static class EntitiesDirectCache<TStateInner, T> where T : struct, IEntity where TStateInner : class, IState<TState> {

            internal static Dictionary<long, T> data = new Dictionary<long, T>(100);

        }

        internal static class EntitiesCache<TStateInner, T> where T : struct, IEntity where TStateInner : class, IState<TState> {

            internal static Dictionary<long, T> data = new Dictionary<long, T>(100);

        }

        private TState resetState;
        private TState currentState;
        private List<ISystem<TState>> systems;
        private List<IModule<TState>> modules;
        private Dictionary<int, int> capacityCache;
        public int id { get; private set; }

        // State cache:
        private Dictionary<int, IList> entitiesCache; // key = typeof(T:IData), value = list of T:IData
        //private Dictionary<EntityId, IEntity> entitiesDirectCache;
        private Dictionary<int, IList> filtersCache; // key = typeof(T:IFilter), value = list of T:IFilter
        private Dictionary<int, IComponents> componentsCache; // key = typeof(T:IData), value = list of T:Components
        
        private float tickTime;
        private double timeSinceStart;

        public World() {

            this.currentState = null;
            this.resetState = null;

        }

        public void SetId(int forcedWorldId = 0) {

            if (forcedWorldId > 0) {

                this.id = forcedWorldId;

            } else {
                
                this.id = ++World<TState>.worldId;
                
            }

        }

        public int GetRandomRange(int from, int to) {

            UnityEngine.Random.state = this.currentState.randomState;
            var result = UnityEngine.Random.Range(from, to);
            this.currentState.randomState = UnityEngine.Random.state;
            return result;

        }

        public float GetRandomRange(float from, float to) {

            UnityEngine.Random.state = this.currentState.randomState;
            var result = UnityEngine.Random.Range(from, to);
            this.currentState.randomState = UnityEngine.Random.state;
            return result;

        }

        public float GetRandomValue() {

            UnityEngine.Random.state = this.currentState.randomState;
            var result = UnityEngine.Random.value;
            this.currentState.randomState = UnityEngine.Random.state;
            return result;
            
        }

        void IWorldBase.SetTickTime(float tickTime) {

            this.tickTime = tickTime;

        }
        
        float IWorldBase.GetTickTime() {

            return this.tickTime;

        }

        double IWorldBase.GetTimeSinceStart() {

            return this.timeSinceStart;

        }

        void IWorldBase.SetTimeSinceStart(double time) {

            this.timeSinceStart = time;

        }
        
        void IPoolableSpawn.OnSpawn() {
            
            this.systems = PoolList<ISystem<TState>>.Spawn(100);
            this.modules = PoolList<IModule<TState>>.Spawn(100);
            this.entitiesCache = PoolDictionary<int, IList>.Spawn(100);
            //this.entitiesDirectCache = PoolDictionary<EntityId, IEntity>.Spawn(100);
            this.filtersCache = PoolDictionary<int, IList>.Spawn(100);
            this.componentsCache = PoolDictionary<int, IComponents>.Spawn(100);
            this.capacityCache = PoolDictionary<int, int>.Spawn(100);

        }

        void IPoolableRecycle.OnRecycle() {
            
            this.ReleaseState(ref this.resetState);
            this.ReleaseState(ref this.currentState);

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
            
            PoolDictionary<int, IList>.Recycle(ref this.entitiesCache);
            //PoolDictionary<EntityId, IEntity>.Recycle(ref this.entitiesDirectCache);
            PoolDictionary<int, IList>.Recycle(ref this.filtersCache);
            PoolDictionary<int, IComponents>.Recycle(ref this.componentsCache);
            PoolDictionary<int, int>.Recycle(ref this.capacityCache);

        }

        public bool GetEntityData<T>(EntityId entityId, out T data) where T : struct, IEntity {

            T internalData;
            if (EntitiesDirectCache<TState, T>.data.TryGetValue(MathUtils.GetKey(this.id, entityId), out internalData) == true) {

                data = internalData;
                return true;

            }

            data = default(T);
            return false;

        }

        public void SetCapacity<T>(int capacity) where T : IEntity {

            var code = WorldUtilities.GetKey<T>();
            this.capacityCache.Add(code, capacity);

        }

        public int GetCapacity<T>() where T : IEntity {

            var code = WorldUtilities.GetKey<T>();
            return this.GetCapacity<T>(code);

        }

        public int GetCapacity<T>(int code) {

            int cap;
            if (this.capacityCache.TryGetValue(code, out cap) == true) {

                return cap;

            }

            return 100;

        }

        public void Register<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey<TEntity>();
            var capacity = 100;
            if (componentsRef == null) {

                componentsRef = PoolClass<Components<TEntity, TState>>.Spawn();
                componentsRef.Initialize(capacity);
                componentsRef.SetFreeze(freeze);

            } else {

                componentsRef.SetFreeze(freeze);

            }

            if (freeze == false) {

                if (this.componentsCache.ContainsKey(code) == true) {

                    this.componentsCache[code] = componentsRef;

                } else {

                    this.componentsCache.Add(code, componentsRef);

                }
                
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

        public void Register<TEntity>(ref Filter<TEntity> filterRef, bool freeze, bool restore) where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey<TEntity>();
            var capacity = this.GetCapacity<TEntity>(code);
            if (filterRef == null) {

                filterRef = PoolClass<Filter<TEntity>>.Spawn();
                filterRef.Initialize(capacity);
                filterRef.SetFreeze(freeze);

            } else {

                filterRef.SetFreeze(freeze);

            }

            if (freeze == false) {

                IList list;
                if (this.filtersCache.TryGetValue(code, out list) == true) {

                    ((List<Filter<TEntity>>)list).Add(filterRef);

                } else {

                    list = PoolList<Filter<TEntity>>.Spawn(capacity);
                    ((List<Filter<TEntity>>)list).Add(filterRef);
                    this.filtersCache.Add(code, list);

                }

            }

            if (restore == true) {

                // Update entities cache
                for (int i = 0; i < filterRef.Count; ++i) {

                    var item = filterRef[i];
                    var list = PoolList<TEntity>.Spawn(capacity);
                    list.Add(item);
                    this.AddEntity(item, updateFilters: false);

                }

                this.UpdateFilters<TEntity>(code);

            }

        }

        public void UpdateFilters<T>() where T : IEntity {

            this.UpdateFilters<T>(WorldUtilities.GetKey<T>());

        }

        public void UpdateFilters<T>(int code) where T : IEntity {

            IList listEntities;
            this.entitiesCache.TryGetValue(code, out listEntities);

            IList listFilters;
            if (this.filtersCache.TryGetValue(code, out listFilters) == true) {

                for (int i = 0, count = listFilters.Count; i < count; ++i) {

                    var filter = (Filter<T>)listFilters[i];
                    filter.SetData((List<T>)listEntities);

                }

            }

        }

        public void SaveResetState() {

            if (this.resetState != null) this.ReleaseState(ref this.resetState);
            this.resetState = this.CreateState();
            this.resetState.Initialize(this, freeze: true, restore: false);
            this.resetState.CopyFrom(this.GetState());

        }

        public TState GetResetState() {

            return this.resetState;

        }

        public TState CreateState() {

            var state = PoolClass<TState>.Spawn();
            state.entityId = default;
            state.tick = default;
            return state;

        }

        public void ReleaseState(ref TState state) {

            //UnityEngine.Debug.LogWarning(UnityEngine.Time.frameCount + " Release state: " + state.tick);
            state.entityId = default;
            state.tick = default;
            PoolClass<TState>.Recycle(ref state);
            
        }

        public void SetState(TState state) {

            //UnityEngine.Debug.Log(UnityEngine.Time.frameCount + " World SetState(): " + state.tick);
            
            this.entitiesCache.Clear();
            //this.entitiesDirectCache.Clear();
            this.filtersCache.Clear();
            this.componentsCache.Clear();

            if (this.currentState != null && this.currentState != state) this.ReleaseState(ref this.currentState);
            this.currentState = state;
            state.Initialize(this, freeze: false, restore: true);

            UnityEngine.Random.InitState(0);
            state.randomState = UnityEngine.Random.state;

            if (this.resetState == null) this.SaveResetState();

        }

        public TState GetState() {

            return this.currentState;

        }

        private Entity CreateNewEntity<T>() where T : IEntity {

            return Entity.Create<T>(++this.GetState().entityId);

        }

        public void UpdateEntityCache<T>(T data) where T : struct, IEntity {

            var key = MathUtils.GetKey(this.id, data.entity.id);
            if (EntitiesDirectCache<TState, T>.data.ContainsKey(key) == true) {

                EntitiesDirectCache<TState, T>.data[key] = data;

            } else {
                
                EntitiesDirectCache<TState, T>.data.Add(key, data);
                
            }

        }

        public Entity AddEntity<T>(T data, bool updateFilters = true) where T : struct, IEntity {

            if (data.entity.id == 0) data.entity = this.CreateNewEntity<T>();

            var code = WorldUtilities.GetKey(data);
            IList list;
            if (this.entitiesCache.TryGetValue(code, out list) == true) {

                ((List<T>)list).Add(data);

            } else {

                list = PoolList<T>.Spawn(this.GetCapacity<T>(code));
                ((List<T>)list).Add(data);
                this.entitiesCache.Add(code, list);

            }

            if (updateFilters == true) {

                this.UpdateFilters<T>(code);

            }
            
            this.UpdateEntityCache(data);

            return data.entity;

        }

        public void RemoveEntity<T>(T data) where T : struct, IEntity {

            var key = MathUtils.GetKey(this.id, data.entity.id);
            EntitiesDirectCache<TState, T>.data.Remove(key);
            
            var code = WorldUtilities.GetKey(data);
            IList list;
            if (this.entitiesCache.TryGetValue(code, out list) == true) {
                
                ((List<T>)list).Remove(data);
                this.RemoveComponents(data.entity);
                
            }
            
        }

        public void RemoveEntity<T>(Entity entity) where T : struct, IEntity {

            var key = MathUtils.GetKey(this.id, entity.id);
            if (EntitiesDirectCache<TState, T>.data.Remove(key) == true) {

                var code = WorldUtilities.GetKey(entity);
                IList list;
                if (this.entitiesCache.TryGetValue(code, out list) == true) {

                    for (int i = 0, count = list.Count; i < count; ++i) {

                        if (((IEntity)list[i]).entity.id == entity.id) {

                            list.RemoveAt(i);
                            this.RemoveComponents(entity);
                            break;

                        }

                    }

                }

            }

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

        /// <summary>
        /// Add module by type
        /// Retrieve module from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <returns></returns>
        public bool AddModule<TModule>() where TModule : class, IModule<TState>, new() {
            
            var instance = PoolModules.Spawn<TModule>();
            instance.world = this;
            if (instance is IModuleValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {

                    UnityEngine.Debug.LogError("Couldn't add new module `" + instanceValidate + "`(" + nameof(TModule) + ") because of CouldBeAdded() returns false.");
                    instance.world = null;
                    PoolModules.Recycle(ref instance);
                    return false;
                    
                }

            }
            
            this.modules.Add(instance);
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
                    module.OnDeconstruct();
                    --i;
                    --count;

                }

            }

        }
        
        /// <summary>
        /// Add system by type
        /// Retrieve system from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        public bool AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new() {

            var instance = PoolSystems.Spawn<TSystem>();
            instance.world = this;
            if (instance is ISystemValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {

                    instance.world = null;
                    PoolSystems.Recycle(ref instance);
                    return false;
                    
                }

            }
            
            this.systems.Add(instance);
            instance.OnConstruct();

            return true;

        }

        /// <summary>
        /// Add system manually
        /// Pool will not be used, OnConstruct() call
        /// </summary>
        /// <param name="instance"></param>
        public bool AddSystem(ISystem<TState> instance) {

            instance.world = this;
            if (instance is ISystemValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {
                    
                    instance.world = null;
                    return false;
                    
                }

            }
            
            this.systems.Add(instance);
            instance.OnConstruct();

            return true;

        }

        /// <summary>
        /// Remove system manually
        /// Pool will not be used, OnDeconstruct() call
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveSystem(ISystem<TState> instance) {

            instance.world = null;
            this.systems.Remove(instance);
            instance.OnDeconstruct();

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
                    system.OnDeconstruct();
                    --i;
                    --count;

                }

            }

        }
        
        public TEntity RunComponents<TEntity>(TEntity data, float deltaTime, int index) where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey(data);
            IComponents componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {

                var item = (Components<TEntity, TState>)componentsContainer;
                var dic = item.GetData();
                List<IComponent<TState, TEntity>> components;
                if (dic.TryGetValue(data.entity.id, out components) == true) {

                    for (int j = 0, count = components.Count; j < count; ++j) {

                        data = components[j].AdvanceTick(this.currentState, data, deltaTime, index);

                    }

                }

            }

            return data;

        }

        public void Update(float deltaTime) {

            var state = this.GetState();
            
            Worlds<TState>.currentWorld = this;
            Worlds<TState>.currentState = state;

            var prevTick = state.tick;
            this.timeSinceStart += deltaTime;

            for (int i = 0, count = this.modules.Count; i < count; ++i) {
                
                this.modules[i].Update(state, deltaTime);
                
            }

            this.Simulate(prevTick + 1, state.tick + 1);

            for (int i = 0, count = this.systems.Count; i < count; ++i) {
                
                this.systems[i].Update(state, deltaTime);
                
            }
            
        }

        public void Simulate(Tick from, Tick to) {
            
            if (from > to) {

                //UnityEngine.Debug.LogError( UnityEngine.Time.frameCount + " From: " + from + ", To: " + to);
                //((IWorldBase)this).Simulate(currentTick);
                return;

            }

            var state = this.GetState();
            
            //UnityEngine.Debug.LogWarning("Simulate[" + this.id + "]: " + from + " >> " + to);
            var fixedDeltaTime = ((IWorldBase)this).GetTickTime();
            for (Tick tick = from; tick < to; ++tick) {

                state.tick = tick;
                //UnityEngine.Debug.Log("Begin tick: " + tick);
                this.PlayPluginsForTick(tick);
                
                for (int i = 0, count = this.systems.Count; i < count; ++i) {

                    this.systems[i].AdvanceTick(state, fixedDeltaTime);

                }
                
                this.RemoveComponents<IComponentOnceBase>();

                //UnityEngine.Debug.Log("End tick: " + tick);

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

        partial void PlayPlugin1ForTick(Tick tick);
        partial void PlayPlugin2ForTick(Tick tick);
        partial void PlayPlugin3ForTick(Tick tick);
        partial void PlayPlugin4ForTick(Tick tick);
        partial void PlayPlugin5ForTick(Tick tick);
        partial void PlayPlugin6ForTick(Tick tick);
        partial void PlayPlugin7ForTick(Tick tick);
        partial void PlayPlugin8ForTick(Tick tick);
        partial void PlayPlugin9ForTick(Tick tick);

        /// <summary>
        /// Add component for current entity only (create component data)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TEntity, TComponent>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : struct, IEntity {

            var data = PoolComponents.Spawn<TComponent>();
            return this.AddComponent<TEntity, TComponent>(entity, (IComponent<TState, TEntity>)data);

        }

        /// <summary>
        /// Add component for entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public TComponent AddComponent<TEntity, TComponent>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey(entity);
            IComponents components;
            if (this.componentsCache.TryGetValue(code, out components) == true) {

                var item = (Components<TEntity, TState>)components;
                item.Add(entity, data);

            } else {

                components = PoolClass<Components<TEntity, TState>>.Spawn();
                ((Components<TEntity, TState>)components).Add(entity, data);
                this.componentsCache.Add(code, components);

            }

            return (TComponent)data;

        }

        /// <summary>
        /// Check is component exists on entity
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public bool HasComponent<TEntity, TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> where TEntity : struct, IEntity {

            var code = WorldUtilities.GetKey(entity);
            IComponents components;
            if (this.componentsCache.TryGetValue(code, out components) == true) {

                return ((Components<TEntity, TState>)components).Contains<TComponent>(entity);

            }

            return false;

        }

        /// <summary>
        /// Remove all components from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents(Entity entity) {
            
            var code = WorldUtilities.GetKey(entity);
            IComponents componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {
                
                componentsContainer.RemoveAll(entity);
                
            }

        }

        /// <summary>
        /// Remove all components with type from certain entity
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveComponents<TComponent>(Entity entity) where TComponent : class, IComponentBase {
            
            var code = WorldUtilities.GetKey(entity);
            IComponents componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {
                
                componentsContainer.RemoveAll<TComponent>(entity);
                
            }

        }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponents<TComponent>() where TComponent : class, IComponentBase {
            
            foreach (var components in this.componentsCache) {
                
                components.Value.RemoveAll<TComponent>();
                
            }
            
        }

    }

    public static class Worlds<TState> where TState : class, IState<TState> {

        public static IWorld<TState> currentWorld;
        public static TState currentState;
        
        private static Dictionary<int, IWorld<TState>> cache = new Dictionary<int, IWorld<TState>>();

        public static IWorld<TState> GetWorld(int id) {

            IWorld<TState> world;
            if (Worlds<TState>.cache.TryGetValue(id, out world) == true) {

                return world;
                
            }

            return null;

        }

        public static void Register(IWorld<TState> world) {
            
            Worlds<TState>.cache.Add(world.id, world);
            
        }
        
        public static void UnRegister(IWorld<TState> world) {
            
            Worlds<TState>.cache.Remove(world.id);
            
        }

    }

    public static class MathUtils {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static long GetKey(int a1, int a2) {
            
            long b = a2;
            b <<= 32;
            b |= (uint)a1;
            return b;
            
        }

    }

    public static class WorldUtilities {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(ref Filter<T> filter) where T : IEntity {
            
            PoolClass<Filter<T>>.Recycle(ref filter);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Release<TEntity, TState>(ref Components<TEntity, TState> components) where TState : class, IState<TState>, new() where TEntity : struct, IEntity {
            
            PoolClass<Components<TEntity, TState>>.Recycle(ref components);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void CreateWorld<TState>(ref World<TState> worldRef, float tickTime, int forcedWorldId = 0) where TState : class, IState<TState>, new() {

            if (worldRef != null) WorldUtilities.ReleaseWorld(ref worldRef);
            worldRef = PoolClass<World<TState>>.Spawn();
            worldRef.SetId(forcedWorldId);
            ((IWorldBase)worldRef).SetTickTime(tickTime);
            Worlds<TState>.Register(worldRef);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ReleaseWorld<TState>(ref World<TState> world) where TState : class, IState<TState>, new() {

            Worlds<TState>.UnRegister(world);
            PoolClass<World<TState>>.Recycle(ref world);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey<T>() {

            return typeof(T).GetHashCode();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey(System.Type type) {

            return type.GetHashCode();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey<T>(T data) where T : IEntity {

            return data.entity.typeId;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int GetKey(Entity data) {

            return data.typeId;

        }

    }

}