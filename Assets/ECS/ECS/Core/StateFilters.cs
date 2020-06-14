using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using ME.ECS.Collections;

    public interface IFilterNode {

        bool Execute(Entity entity);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class FiltersStorage : IPoolableRecycle {

        internal BufferArray<Filter> filters;
        private bool freeze;
        private int nextId;
        internal Archetype allFiltersArchetype;

        public int Count {
            get {
                return this.filters.Length;
            }
        }

        public int GetAllFiltersArchetypeCount() {

            return this.allFiltersArchetype.Count;

        }
        
        public bool HasInFilters<TComponent>() {

            return this.allFiltersArchetype.Has<TComponent>();

        }

        public void RegisterInAllArchetype(in Archetype archetype) {

            this.allFiltersArchetype.Add(in archetype);

        }
        
        void IPoolableRecycle.OnRecycle() {

            this.nextId = default;
            this.freeze = default;
            
            for (int i = 0, count = this.filters.Length; i < count; ++i) {
                
                if (this.filters[i] == null) continue;
                this.filters[i].Recycle();

            }

            PoolArray<Filter>.Recycle(ref this.filters);
            
        }

        public void Initialize(int capacity) {

            this.filters = PoolArray<Filter>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public BufferArray<Filter> GetData() {

            return this.filters;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref Filter Get(in int id) {

            return ref this.filters[id - 1];

        }

        public Filter GetByHashCode(int hashCode) {

            for (int i = 0; i < this.filters.Length; ++i) {

                var filter = this.filters[i];
                if (filter != null) {

                    if (filter.GetHashCode() == hashCode) {

                        return filter;

                    }

                }

            }
            
            return null;

        }

        public Filter GetFilterEquals(Filter other) {
            
            for (int i = 0; i < this.filters.Length; ++i) {

                var filter = this.filters[i];
                if (filter != null) {

                    if (filter.GetHashCode() == other.GetHashCode() && filter.IsEquals(other) == true) {

                        return filter;

                    }

                }

            }

            return null;

        }

        public void Register(Filter filter) {

            ArrayUtils.Resize(filter.id - 1, ref this.filters);
            this.filters[filter.id - 1] = filter;
            
        }

        public int GetNextId() {

            return this.nextId + 1;

        }

        public int AllocateNextId() {

            return ++this.nextId;

        }

        public void CopyFrom(FiltersStorage other) {

            this.nextId = other.nextId;
            
            /*if (this.filters != null) {

                for (int i = 0, count = this.filters.Count; i < count; ++i) {
                    
                    this.filters[i].Recycle();

                }

                PoolList<IFilterBase>.Recycle(ref this.filters);
                
            }
            this.filters = PoolList<IFilterBase>.Spawn(other.filters.Count);

            for (int i = 0, count = other.filters.Count; i < count; ++i) {
                
                var copy = other.filters[i].Clone();
                this.filters.Add(copy);
                UnityEngine.Debug.Log("Copy filter: " + i + " :: " + other.filters[i].id + " >> " + this.filters.Count + " (" + copy.id + ")");
                
            }*/

            if (this.freeze == true) {

                // Just copy if filters storage is in freeze mode
                if (this.filters.arr == null) {

                    this.filters = PoolArray<Filter>.Spawn(other.filters.Length);

                }

                for (int i = 0, count = other.filters.Length; i < count; ++i) {

                    if (other.filters[i] == null && this.filters[i] == null) {
                        
                        continue;
                        
                    }

                    if (other.filters[i] == null && this.filters[i] != null) {

                        this.filters[i].Recycle();
                        this.filters[i] = null;
                        continue;
                        
                    }

                    if (i >= this.filters.Length || this.filters[i] == null) {

                        this.Register(other.filters[i].Clone());

                    } else {

                        this.filters[i].CopyFrom(other.filters[i]);

                    }

                }

            } else {
                
                // Filters storage is not in a freeze mode, so it is an active state filters
                for (int i = 0, count = other.filters.Length; i < count; ++i) {

                    if (other.filters[i] == null && this.filters[i] == null) {
                        
                        continue;
                        
                    }

                    if (other.filters[i] == null && this.filters[i] != null) {

                        this.filters[i].Recycle();
                        this.filters[i] = null;
                        continue;
                        
                    }

                    if (i >= this.filters.Length || this.filters[i] == null && other.filters[i] != null) {

                        this.Register(other.filters[i].Clone());

                    } else {

                        this.filters[i].CopyFrom(other.filters[i]);

                    }

                }
                
            }

        }

    }

    public struct FilterEnumerator : IEnumerator<Entity> {
            
        private readonly Filter set;
        private BufferArray<Entity>.Enumerator setEnumerator;
        private BufferArray<Entity> arr;
            
        internal FilterEnumerator(Filter set) {
                
            this.set = set;
            this.arr = this.set.GetArray();
            this.setEnumerator = this.arr.GetEnumerator();
            this.set.SetForEachMode(true);

        }
 
        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Dispose() {

            PoolArray<Entity>.Recycle(this.arr);
            this.set.SetForEachMode(false);

        }
 
        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            
            return this.setEnumerator.MoveNext();
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        Entity IEnumerator<Entity>.Current {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.setEnumerator.Current;
            }
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public ref Entity Current {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.setEnumerator.Current;
            }
        }
 
        System.Object System.Collections.IEnumerator.Current {
            get {
                throw new AllocationException();
            }
        }
 
        void System.Collections.IEnumerator.Reset() {
                
        }
    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Filter : IPoolableSpawn, IPoolableRecycle, IEnumerable<Entity> {

        private const int REQUESTS_CAPACITY = 4;
        private const int NODES_CAPACITY = 4;
        private const int ENTITIES_CAPACITY = 100;

        public int id;

        public string name {
            get {
                return this.aliases[0];
            }
        }

        private World world;
        private BufferArray<IFilterNode> nodes;
        private Archetype archetypeContains;
        private Archetype archetypeNotContains;
        private int nodesCount;
        private BufferArray<bool> dataContains;
        private BufferArray<Entity> data;
        private bool forEachMode;
        private BufferArray<Entity> requests;
        private BufferArray<Entity> requestsRemoveEntity;
        private int min;
        private int max;
        
        private List<IFilterNode> tempNodes;
        private List<IFilterNode> tempNodesCustom;
        internal BufferArray<string> aliases;
        private int dataCount;

        #if UNITY_EDITOR
        private BufferArray<string> editorTypes;
        private BufferArray<string> editorStackTraceFile;
        private BufferArray<int> editorStackTraceLineNumber;
        #endif

        public Filter() {}

        internal Filter(string name) {

            this.AddAlias(name);

        }

        public void Recycle() {
            
            PoolFilters.Recycle(this);
            
        }

        public void SetEntityCapacity(int capacity) {
            
            ArrayUtils.Resize(capacity, ref this.requests);
            ArrayUtils.Resize(capacity, ref this.requestsRemoveEntity);
            ArrayUtils.Resize(capacity, ref this.data);
            ArrayUtils.Resize(capacity, ref this.dataContains);

        }
        
        public void OnEntityCreate(in Entity entity) {

            ArrayUtils.Resize(entity.id, ref this.requests);
            ArrayUtils.Resize(entity.id, ref this.requestsRemoveEntity);
            ArrayUtils.Resize(entity.id, ref this.data);
            ArrayUtils.Resize(entity.id, ref this.dataContains);

        }

        public void OnEntityDestroy(in Entity entity) {

            ArrayUtils.Resize(entity.id, ref this.requests);
            ArrayUtils.Resize(entity.id, ref this.requestsRemoveEntity);
            ArrayUtils.Resize(entity.id, ref this.data);
            ArrayUtils.Resize(entity.id, ref this.dataContains);
            
        }

        public void Update() {

            if (this.world.ForEachEntity(out RefList<Entity> list) == true) {

                for (int i = list.FromIndex; i < list.SizeCount; ++i) {

                    var entity = list[i];
                    if (list.IsFree(i) == true) continue;
                    
                    this.OnUpdate(entity);

                }

            }

        }

        public Filter Clone() {

            var instance = PoolFilters.Spawn<Filter>();
            instance.CopyFrom(this);
            return instance;

        }

        public BufferArray<string> GetAllNames() {

            return this.aliases;

        }
        
        private void AddAlias(string name) {

            if (string.IsNullOrEmpty(name) == true) return;
            
            var idx = (this.aliases.arr != null ? this.aliases.Length : 0);
            ArrayUtils.Resize(idx, ref this.aliases, resizeWithOffset: false);
            this.aliases[idx] = name;

        }

        #if UNITY_EDITOR
        public string GetEditorStackTraceFilename(int index) {

            return this.editorStackTraceFile[index];

        }

        public int GetEditorStackTraceLineNumber(int index) {

            return this.editorStackTraceLineNumber[index];

        }

        public string ToEditorTypesString() {

            return string.Join(", ", this.editorTypes.arr, 0, this.editorTypes.Length);

        }

        public void AddTypeToEditorWith<TComponent>() {
            
            var idx = (this.editorTypes.arr != null ? this.editorTypes.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorTypes, resizeWithOffset: false);
            this.editorTypes[idx] = "W<" + typeof(TComponent).Name + ">";
            
        }

        public void AddTypeToEditorWithout<TComponent>() {

            var idx = (this.editorTypes.arr != null ? this.editorTypes.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorTypes, resizeWithOffset: false);
            this.editorTypes[idx] = "WO<" + typeof(TComponent).Name + ">";

        }

        public void OnEditorFilterCreate() {

            const int frameIndex = 2;
            var st = new System.Diagnostics.StackTrace(true);
            string currentFile = st.GetFrame(frameIndex).GetFileName(); 
            int currentLine = st.GetFrame(frameIndex).GetFileLineNumber();
            
            var path = UnityEngine.Application.dataPath;
            currentFile = "Assets" + currentFile.Substring(path.Length);
            
            this.OnEditorFilterAddStackTrace(currentFile, currentLine);
            
        }

        public void OnEditorFilterAddStackTrace(string file, int lineNumber) {
            
            var idx = (this.editorStackTraceFile.arr != null ? this.editorStackTraceFile.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorStackTraceFile, resizeWithOffset: false);
            this.editorStackTraceFile[idx] = file;
            
            idx = (this.editorStackTraceLineNumber.arr != null ? this.editorStackTraceLineNumber.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorStackTraceLineNumber, resizeWithOffset: false);
            this.editorStackTraceLineNumber[idx] = lineNumber;

        }
        #endif
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public BufferArray<Entity> GetArray() {

            var data = PoolArray<Entity>.Spawn(this.dataCount);
            for (int i = 0, k = 0; i < this.data.Length; ++i) {
                if (this.data[i].version > 0) {
                    data[k++] = this.data[i];
                }
            }
            return data;

        }

        void IPoolableSpawn.OnSpawn() {

            this.requests = PoolArray<Entity>.Spawn(Filter.REQUESTS_CAPACITY);
            this.requestsRemoveEntity = PoolArray<Entity>.Spawn(Filter.REQUESTS_CAPACITY);
            this.nodes = PoolArray<IFilterNode>.Spawn(Filter.NODES_CAPACITY);
            this.data = PoolArray<Entity>.Spawn(Filter.ENTITIES_CAPACITY);
            this.dataContains = PoolArray<bool>.Spawn(Filter.ENTITIES_CAPACITY);

            this.id = default;
            if (this.aliases.arr != null) PoolArray<string>.Recycle(ref this.aliases);
            this.nodesCount = default;
            this.archetypeContains = default;
            this.archetypeNotContains = default;

            this.min = int.MaxValue;
            this.max = int.MinValue;
            
            #if UNITY_EDITOR
            if (this.editorTypes.arr != null) PoolArray<string>.Recycle(ref this.editorTypes);
            if (this.editorStackTraceFile.arr != null) PoolArray<string>.Recycle(ref this.editorStackTraceFile);
            if (this.editorStackTraceLineNumber.arr != null) PoolArray<int>.Recycle(ref this.editorStackTraceLineNumber);
            #endif
            
        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<bool>.Recycle(ref this.dataContains);
            PoolArray<Entity>.Recycle(ref this.data);
            PoolArray<IFilterNode>.Recycle(ref this.nodes);
            PoolArray<Entity>.Recycle(ref this.requestsRemoveEntity);
            PoolArray<Entity>.Recycle(ref this.requests);

            this.min = int.MaxValue;
            this.max = int.MinValue;

            this.archetypeContains = default;
            this.archetypeNotContains = default;
            this.nodesCount = default;
            this.id = default;
            if (this.aliases.arr != null) PoolArray<string>.Recycle(ref this.aliases);
            
            #if UNITY_EDITOR
            if (this.editorTypes.arr != null) PoolArray<string>.Recycle(ref this.editorTypes);
            if (this.editorStackTraceFile.arr != null) PoolArray<string>.Recycle(ref this.editorStackTraceFile);
            if (this.editorStackTraceLineNumber.arr != null) PoolArray<int>.Recycle(ref this.editorStackTraceLineNumber);
            #endif

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsForEntity(in Entity entity) {

            ref var previousArchetype = ref this.world.currentState.storage.archetypes.GetPrevious(in entity);
            if (previousArchetype.ContainsAll(in this.archetypeContains) == true && previousArchetype.NotContains(in this.archetypeNotContains) == true) return true;
            
            ref var currentArchetype = ref this.world.currentState.storage.archetypes.Get(in entity);
            if (currentArchetype.ContainsAll(in this.archetypeContains) == true && currentArchetype.NotContains(in this.archetypeNotContains) == true) return true;

            return false;

        }

        public int GetNodesCount() {

            return this.nodesCount;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Archetype GetArchetypeContains() {

            return this.archetypeContains;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Archetype GetArchetypeNotContains() {

            return this.archetypeNotContains;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetForEachMode(bool state) {

            this.forEachMode = state;
            if (state == false) {

                if (this.world.currentSystemContext == null) {

                    this.ApplyAllRequests();

                }
                
            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void ApplyAllRequests() {

            var internalFilter = this;
            {
                
                var requests = internalFilter.GetRequests();
                for (int i = 0; i < requests.Length; ++i) {
                    
                    internalFilter.OnUpdate(requests[i]);

                }
                //this.requests = requests.Dispose();
                System.Array.Clear(requests.arr, 0, requests.Length);

            }

            {
                
                var requests = internalFilter.GetRequestsRemoveEntity();
                for (int i = 0; i < requests.Length; ++i) {
                    
                    internalFilter.Remove_INTERNAL(requests[i]);

                }
                //this.requestsRemoveEntity = requests.Dispose();
                System.Array.Clear(requests.arr, 0, requests.Length);
                
            }

        }

        public void CopyFrom(Filter other) {

            this.id = other.id;
            this.min = other.min;
            this.max = other.max;
            this.dataCount = other.dataCount;
            ArrayUtils.Copy(in other.aliases, ref this.aliases);
            this.nodesCount = other.nodesCount;

            this.archetypeContains = other.archetypeContains;
            this.archetypeNotContains = other.archetypeNotContains;
            
            ArrayUtils.Copy(in other.nodes, ref this.nodes);

            ArrayUtils.Copy(in other.data, ref this.data);
            ArrayUtils.Copy(in other.dataContains, ref this.dataContains);
            
            #if UNITY_EDITOR
            ArrayUtils.Copy(in other.editorTypes, ref this.editorTypes);
            this.editorStackTraceFile = other.editorStackTraceFile;
            this.editorStackTraceLineNumber = other.editorStackTraceLineNumber;
            #endif
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public BufferArray<Entity> GetData() {

            return this.data;

        }

        public BufferArray<Entity> GetRequests() {

            return this.requests;

        }

        public BufferArray<Entity> GetRequestsRemoveEntity() {

            return this.requestsRemoveEntity;

        }

        public int Count {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.dataCount;
            }
        }

        /*
        public ref Entity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.data;
            }
        }*/

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Contains(Entity entity) {

            return this.world.GetFilter(this.id).Contains_INTERNAL(entity.id);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool Contains_INTERNAL(in int entityId) {

            if (entityId >= this.dataContains.Length) return false;
            return this.dataContains[entityId];

        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() {

            //return ((IEnumerable<Entity>)this.data).GetEnumerator();
            throw new AllocationException();

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {

            //return ((System.Collections.IEnumerable)this.data).GetEnumerator();
            throw new AllocationException();

        }

        public FilterEnumerator GetEnumerator() {

            return new FilterEnumerator(this);

        }

        public bool OnUpdate(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        public bool OnAddComponent(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        public bool OnRemoveComponent(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool OnUpdate_INTERNAL(in Entity entity) {

            if (entity.version == 0) return false;
            
            if (this.world.currentSystemContext != null) {
                
                this.world.GetCurrentSystemContextFiltersUsed()[this.id] = true;
                //this.requests.TryAdd(entity.version, entity);
                this.requests[entity.id] = entity;
                return false;

            }

            if (this.forEachMode == true) {

                //this.requests.TryAdd(entity.version, entity);
                this.requests[entity.id] = entity;
                return false;

            }

            var isExists = this.Contains_INTERNAL(in entity.id);
            if (isExists == true) {

                return this.CheckRemove(in entity);

            } else {

                return this.CheckAdd(in entity);

            }

        }

        public bool OnRemoveEntity(in Entity entity) {

            if (entity.version == 0) return false;

            if (this.world.currentSystemContext != null) {

                this.world.GetCurrentSystemContextFiltersUsed()[this.id] = true;
                //this.requestsRemoveEntity.TryAdd(entity.version, entity);
                this.requestsRemoveEntity[entity.id] = entity;
                return false;

            }
            
            if (this.forEachMode == true) {

                //this.requestsRemoveEntity.TryAdd(entity.version, entity);
                this.requestsRemoveEntity[entity.id] = entity;
                return false;

            }

            return this.Remove_INTERNAL(entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal void Add_INTERNAL(in Entity entity) {

            var idx = entity.id;
            //ArrayUtils.Resize(entity.id, ref this.dataContains);
            ref var res = ref this.dataContains[idx];
            if (res == false) {

                res = true;
                //this.data.Add(entity.id, entity);
                this.data[idx] = entity;
                ++this.dataCount;
                this.UpdateMinMaxAdd(idx);

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal bool Remove_INTERNAL(in Entity entity) {

            var idx = entity.id;
            //if (idx < 0 || idx >= this.dataContains.Length) return false;
            ref var res = ref this.dataContains[idx];
            if (res == true) {

                res = false;
                //this.data.Remove(entity.id);
                this.data[idx] = default;
                --this.dataCount;
                this.UpdateMinMaxRemove(idx);
                return true;

            }

            return false;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateMinMaxAdd(int idx) {

            if (idx < this.min) this.min = idx;
            if (idx > this.max) this.max = idx;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateMinMaxRemove(int idx) {

            if (idx == this.min && idx == this.max) {

                this.min = int.MaxValue;
                this.max = int.MinValue;
                return;

            }
            
            if (idx == this.min) {

                // Update new min (find next index)
                for (int i = idx; i < this.data.Length; ++i) {

                    if (this.dataContains[i] == true) {

                        this.min = i;
                        break;

                    }

                }

            } else if (idx == this.max) {

                // Update new max (find prev index)
                for (int i = idx; i >= 0; --i) {

                    if (this.dataContains[i] == true) {

                        this.max = i;
                        break;

                    }

                }

            }

        }

        /*[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool ContainsAllNodes(Entity entity) {
            
            for (int i = 0; i < this.nodesCount; ++i) {

                if (this.nodes[i].Execute(entity) == false) {

                    return false;

                }

            }

            return true;

        }*/

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool CheckAdd(in Entity entity) {

            // If entity doesn't exist in cache - try to add if entity's archetype fit with contains & notContains
            ref var entArchetype = ref this.world.currentState.storage.archetypes.Get(in entity);
            /*if (System.Array.IndexOf(this.aliases.arr, "Filter-UnitSpawnSystem") >= 0) {

                UnityEngine.Debug.Log(entArchetype);
                UnityEngine.Debug.Log(this.archetypeContains);
                UnityEngine.Debug.Log(this.archetypeNotContains);
                UnityEngine.Debug.Log(entArchetype.ContainsAll(this.archetypeContains));
                UnityEngine.Debug.Log(entArchetype.NotContains(this.archetypeNotContains));

            }*/
            if (entArchetype.ContainsAll(in this.archetypeContains) == false) return false;
            if (entArchetype.NotContains(in this.archetypeNotContains) == false) return false;

            if (this.nodesCount > 0) {

                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(entity) == false) {

                        return false;

                    }

                }

            }

            this.Add_INTERNAL(in entity);

            return true;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool CheckRemove(in Entity entity) {

            // If entity already exists in cache - try to remove if entity's archetype doesn't fit with contains & notContains
            ref var entArchetype = ref this.world.currentState.storage.archetypes.Get(in entity);
            var allContains = entArchetype.ContainsAll(in this.archetypeContains);
            var allNotContains = entArchetype.NotContains(in this.archetypeNotContains);
            
            if (this.nodesCount > 0) {

                var isFail = false;
                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(entity) == false) {

                        isFail = true;
                        break;

                    }

                }

                if (isFail == false) return false;

            }
            
            if (allContains == true && allNotContains == true) return false;
            
            return this.Remove_INTERNAL(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsEquals(Filter filter) {

            if (this.GetArchetypeContains() == filter.GetArchetypeContains() &&
                this.GetArchetypeNotContains() == filter.GetArchetypeNotContains() &&
                this.GetType() == filter.GetType() &&
                this.GetNodesCount() == filter.GetNodesCount()) {

                return true;

            }

            return false;

        }

        public override int GetHashCode() {

            var hashCode = this.GetType().GetHashCode() ^ this.archetypeContains.GetHashCode() ^ this.archetypeNotContains.GetHashCode();
            for (int i = 0; i < this.nodesCount; ++i) {

                hashCode ^= this.nodes[i].GetType().GetHashCode();

            }
            
            return hashCode;

        }

        public Filter Push() {

            Filter filter = null;
            return this.Push(ref filter);

        }

        public Filter Push(ref Filter filter) {

            var world = Worlds.currentWorld;
            var worldInt = Worlds.currentWorld;
            var nextId = world.currentState.filters.GetNextId();
            if (worldInt.HasFilter(nextId) == false) {

                this.tempNodes.AddRange(this.tempNodesCustom);
                var arr = this.tempNodes.OrderBy(x => x.GetType().GetHashCode()).ToArray();
                if (this.nodes.arr != null) PoolArray<IFilterNode>.Recycle(ref this.nodes);
                this.nodes = new BufferArray<IFilterNode>(arr, arr.Length);
                this.nodesCount = this.nodes.Length;
                this.tempNodes.Clear();
                this.tempNodesCustom.Clear();
                
                var existsFilter = worldInt.GetFilterEquals(this);
                if (existsFilter != null) {

                    filter = existsFilter;
                    filter.AddAlias(this.name);
                    #if UNITY_EDITOR
                    filter.OnEditorFilterAddStackTrace(this.editorStackTraceFile[0], this.editorStackTraceLineNumber[0]);
                    #endif
                    this.Recycle();
                    return existsFilter;

                } else {

                    this.id = world.currentState.filters.AllocateNextId();

                    filter = this;
                    this.world = worldInt;
                    world.currentState.filters.RegisterInAllArchetype(in this.archetypeContains);
                    world.currentState.filters.RegisterInAllArchetype(in this.archetypeNotContains);
                    worldInt.Register(this);
                    
                }

            } else {

                UnityEngine.Debug.LogWarning(string.Format("World #{0} already has filter {1}!", worldInt.id, this));

            }

            return this;

        }

        public Filter Custom(IFilterNode filter) {

            this.tempNodesCustom.Add(filter);
            return this;

        }

        public Filter Custom<TFilter>() where TFilter : class, IFilterNode, new() {

            var filter = new TFilter();
            this.tempNodesCustom.Add(filter);
            return this;

        }

        public Filter WithComponent<TComponent>() where TComponent : class, IComponent {

            //var node = new ComponentExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWith<TComponent>();
            #endif
            return this;

        }
        
        public Filter WithoutComponent<TComponent>() where TComponent : class, IComponent {

            //var node = new ComponentNotExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeNotContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWithout<TComponent>();
            #endif
            return this;

        }

        public Filter WithStructComponent<TComponent>() where TComponent : struct, IStructComponent {

            //var node = new ComponentStructExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWith<TComponent>();
            #endif
            return this;

        }

        public Filter WithoutStructComponent<TComponent>() where TComponent : struct, IStructComponent {

            //var node = new ComponentStructNotExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeNotContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWithout<TComponent>();
            #endif
            return this;

        }

        public static Filter Create(string customName = null) {

            var f = PoolFilters.Spawn<Filter>();
            f.AddAlias(customName);
            f.tempNodes = new List<IFilterNode>();
            f.tempNodesCustom = new List<IFilterNode>();
            #if UNITY_EDITOR
            f.OnEditorFilterCreate();
            #endif
            return f;

        }

        public override string ToString() {

            return "Name: " + string.Join("/", this.aliases.arr, 0, this.aliases.Length) + " (#" + this.id.ToString() + ")";

        }

    }

}