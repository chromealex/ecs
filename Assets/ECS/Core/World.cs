using System.Collections;
using System.Collections.Generic;
using EntityId = System.Int32;

namespace ME.ECS {

    public class World<TState> : IWorld<TState> where TState : class, IState<TState>, new() {

        private TState currentState;
        private List<ISystemBase> systems = new List<ISystemBase>();
        private Dictionary<int, IList> entitiesCache = new Dictionary<int, IList>(); // key = typeof(T:IData), value = list of T:IData
        private Dictionary<EntityId, IEntity> entitiesDirectCache = new Dictionary<EntityId, IEntity>();
        private Dictionary<int, IList> filtersCache = new Dictionary<int, IList>(); // key = typeof(T:IFilter), value = list of T:IFilter
        private Dictionary<int, IComponents> componentsCache = new Dictionary<int, IComponents>(); // key = typeof(T:IData), value = list of T:Components
        private Dictionary<int, int> capacityCache = new Dictionary<int, int>();

        public bool GetEntityData<T>(EntityId entityId, out T data) where T : IEntity {

            IEntity internalData;
            if (this.entitiesDirectCache.TryGetValue(entityId, out internalData) == true) {

                data = (T)internalData;
                return true;

            }

            /*var code = this.GetKey<T>();
            IList list;
            if (this.entitiesCache.TryGetValue(code, out list) == true) {

                for (int i = 0; i < list.Count; ++i) {

                    var item = (IEntity)list[i];
                    if (item.entity.id == entityId) {

                        data = (T)item;
                        return true;

                    }

                }

            }*/

            data = default(T);
            return false;

        }

        public void SetCapacity<T>(int capacity) where T : IEntity {

            var code = WorldUtilities.GetKey<T>();
            this.capacityCache.Add(code, capacity);

        }

        public int GetCapacity<T>() {

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

        public void AddComponents<TEntity>(ref Components<TEntity, TState> componentsRef, bool freeze, bool restore) where TEntity : IEntity {

            var code = WorldUtilities.GetKey<TEntity>();
            var capacity = 100;
            if (componentsRef == null) {

                componentsRef = new Components<TEntity, TState>();
                componentsRef.Initialize(capacity);
                componentsRef.SetFreeze(freeze);

            } else {

                componentsRef.SetFreeze(freeze);

            }

            {
                IComponents components;
                if (this.componentsCache.TryGetValue(code, out components) == true) {

                    this.componentsCache[code] = componentsRef;

                } else {

                    this.componentsCache.Add(code, componentsRef);

                }
            }

            if (restore == true) {

                var data = componentsRef.GetData();
                foreach (var item in data) {

                    var components = item.Value;
                    for (int i = 0, count = components.Count; i < count; ++i) {

                        this.AddComponent<IComponent<TState, TEntity>, TEntity>(Entity.Create<TEntity>(item.Key), components[i]);

                    }

                }

            }

        }

        public void AddFilter<T>(ref Filter<T> filterRef, bool freeze, bool restore) where T : IEntity {

            var code = WorldUtilities.GetKey<T>();
            var capacity = this.GetCapacity<T>(code);
            if (filterRef == null) {

                filterRef = new Filter<T>();
                filterRef.Initialize(capacity);
                filterRef.SetFreeze(freeze);

            } else {

                filterRef.SetFreeze(freeze);

            }

            IList list;
            if (this.filtersCache.TryGetValue(code, out list) == true) {

                ((List<Filter<T>>)list).Add(filterRef);

            } else {

                list = new List<Filter<T>>(capacity);
                ((List<Filter<T>>)list).Add(filterRef);
                this.filtersCache.Add(code, list);

            }

            if (restore == true) {

                // Update entities cache
                for (int i = 0; i < filterRef.Count; ++i) {

                    var item = filterRef[i];
                    list = new List<T>(capacity);
                    ((List<T>)list).Add(item);
                    this.AddEntity(item, updateFilters: false);

                }

                this.UpdateFilters<T>(code);

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

                for (int i = 0; i < listFilters.Count; ++i) {

                    var filter = (Filter<T>)listFilters[i];
                    filter.SetData((List<T>)listEntities);

                }

            }

        }

        public void SetState(TState state) {

            this.entitiesCache.Clear();
            this.entitiesDirectCache.Clear();
            this.filtersCache.Clear();
            this.componentsCache.Clear();

            this.currentState = state;
            state.Initialize(this, freeze: false, restore: true);

        }

        public TState GetState() {

            return this.currentState;

        }

        private Entity CreateNewEntity<T>() where T : IEntity {

            return Entity.Create<T>(++this.GetState().entityId);

        }

        public void AddEntity<T>(T data, bool updateFilters = true) where T : IEntity {

            if (data.entity.id == 0) data.entity = this.CreateNewEntity<T>();

            this.entitiesDirectCache.Add(data.entity.id, data);

            var code = WorldUtilities.GetKey(data);
            IList list;
            if (this.entitiesCache.TryGetValue(code, out list) == true) {

                ((List<T>)list).Add(data);

            } else {

                list = new List<T>(this.GetCapacity<T>(code));
                ((List<T>)list).Add(data);
                this.entitiesCache.Add(code, list);

            }

            if (updateFilters == true) {

                this.UpdateFilters<T>(code);

            }

        }

        public TData RunComponents<TData>(TData data, float deltaTime, int index) where TData : IEntity {

            var code = WorldUtilities.GetKey<TData>(data);
            return this.RunComponents(code, data, deltaTime, index);

        }

        public TData RunComponents<TData>(int code, TData data, float deltaTime, int index) where TData : IEntity {

            IComponents componentsContainer;
            if (this.componentsCache.TryGetValue(code, out componentsContainer) == true) {

                var item = (Components<TData, TState>)componentsContainer;
                var dic = item.GetData();
                List<IComponent<TState, TData>> components;
                if (dic.TryGetValue(data.entity.id, out components) == true) {

                    for (int j = 0, count = components.Count; j < count; ++j) {

                        data = components[j].AdvanceTick(this.currentState, data, deltaTime, index);

                    }

                }

            }

            return data;

        }

        /// <summary>
        /// Add component for current entity only
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        public void AddComponent<TComponent, TEntity>(Entity entity) where TComponent : class, IComponentBase, new() where TEntity : IEntity {

            var data = new TComponent();
            this.AddComponent<TComponent, TEntity>(entity, (IComponent<TState, TEntity>)data);

        }

        public void AddComponent<TComponent, TEntity>(Entity entity, IComponent<TState, TEntity> data) where TComponent : class, IComponentBase where TEntity : IEntity {

            var code = WorldUtilities.GetKey<TEntity>(entity);
            IComponents components;
            if (this.componentsCache.TryGetValue(code, out components) == true) {

                var item = (Components<TEntity, TState>)components;
                item.Add(entity, data);

            } else {

                components = new Components<TEntity, TState>();
                ((Components<TEntity, TState>)components).Add(entity, data);
                this.componentsCache.Add(code, components);

            }

        }

        /// <summary>
        /// Add component for all current and future entities with the type TEntity
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public void AddComponent<TEntity, TComponent>(TComponent data) where TEntity : IEntity where TComponent : IComponentBase { }

        public bool HasComponent<T>(Entity entity) where T : IComponent<IStateBase, IEntity> {

            return false;

        }

        /// <summary>
        /// Remove all components with type T from certain entity
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>(Entity entity) where T : IEntity { }

        /// <summary>
        /// Remove all components with type TComponent and on entities with type TEntity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponent<TEntity, TComponent>() where TEntity : IEntity where TComponent : IComponent<IStateBase, TEntity> { }

        /// <summary>
        /// Remove all components with type TComponent from all entities
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        public void RemoveComponent<TComponent>() where TComponent : IComponent<IStateBase, IEntity> { }

        public void AddSystem(ISystem<TState> instance) {

            instance.world = this;
            this.systems.Add(instance);

        }

        public void Update(float deltaTime) {

            Worlds<TState>.currentWorld = this;
            Worlds<TState>.currentState = this.GetState();

            for (int i = 0; i < this.systems.Count; ++i) {

                var system = this.systems[i] as ISystem<TState>;
                system.AdvanceTick(this.GetState(), deltaTime);

            }

        }

    }

    public static class Worlds<TState> where TState : IStateBase {

        public static IWorld<TState> currentWorld;
        public static TState currentState;

    }

    public static class WorldUtilities {

        public static int GetKey<T>() {

            return typeof(T).GetHashCode();

        }

        public static int GetKey<T>(T data) where T : IEntity {

            return data.entity.typeId;

        }

        public static int GetKey<T>(Entity data) where T : IEntity {

            return data.typeId;

        }

    }

}