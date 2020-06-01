using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    using ME.ECS.Collections;

    public interface IFilterInternal : IFilter {

        bool OnUpdate(in Entity entity);
        //bool CheckAdd(in Entity entity);
        //bool CheckRemove(in Entity entity);
        bool OnAddComponent(in Entity entity);
        bool OnRemoveComponent(in Entity entity);
        bool OnRemoveEntity(in Entity entity);

        SortedList<int, Entity> GetRequests();
        SortedList<int, Entity> GetRequestsRemoveEntity();
        bool forEachMode { get; set; }
        void Add_INTERNAL(in Entity entity);
        bool Remove_INTERNAL(in Entity entity);

    }

    public interface IFilterBase : IPoolableSpawn, IPoolableRecycle {

        int id { get; set; }
        string name { get; }
        int Count { get; }

        string[] GetAllNames();
        
        void Recycle();
        IFilterBase Clone();
        void CopyFrom(IFilterBase other);
        void Update();

        Archetype GetArchetypeContains();
        Archetype GetArchetypeNotContains();
        int GetNodesCount();

        void SetForEachMode(bool state);
        
        SortedList<int, Entity> GetData();

        bool IsEquals(IFilterBase other);

        #if UNITY_EDITOR
        string ToEditorTypesString();
        string GetEditorStackTraceFilename(int index);
        int GetEditorStackTraceLineNumber(int index);
        void OnEditorFilterAddStackTrace(string file, int lineNumber);
        #endif

    }

    public interface IFilterNode {

        bool Execute(Entity entity);

    }

    public interface IFilter : IFilterBase, IEnumerable<Entity> {

        void AddAlias(string name);

        /*ref Entity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get;
        }*/
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool Contains(Entity entity);
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool IsForEntity(in Entity entity);

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void ApplyAllRequests();
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        Entity[] GetArray();
        IFilter Custom(IFilterNode filter);
        IFilter Custom<TFilter>() where TFilter : class, IFilterNode, new();
        IFilter WithComponent<TComponent>() where TComponent : class, IComponent;
        IFilter WithoutComponent<TComponent>() where TComponent : class, IComponent;
        IFilter WithStructComponent<TComponent>() where TComponent : struct, IStructComponent;
        /*IFilter WithOneOfStructComponent<T1, T2>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent;
        IFilter WithOneOfStructComponent<T1, T2, T3>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent
            where T3 : struct, IStructComponent;
        IFilter WithOneOfStructComponent<T1, T2, T3, T4>()
            where T1 : struct, IStructComponent
            where T2 : struct, IStructComponent
            where T3 : struct, IStructComponent
            where T4 : struct, IStructComponent;*/
        IFilter WithoutStructComponent<TComponent>() where TComponent : struct, IStructComponent;

        IFilter Push();
        IFilter Push(ref IFilter filter);
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        new FilterEnumerator GetEnumerator();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class FiltersStorage : IPoolableRecycle {

        internal IFilterBase[] filters;
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

        public void RegisterInAllArchetype(Archetype archetype) {

            this.allFiltersArchetype.Add(archetype);

        }
        
        void IPoolableRecycle.OnRecycle() {

            this.nextId = default;
            this.freeze = default;
            
            if (this.filters != null) {

                for (int i = 0, count = this.filters.Length; i < count; ++i) {
                    
                    if (this.filters[i] == null) continue;
                    this.filters[i].Recycle();

                }

                PoolArray<IFilterBase>.Recycle(ref this.filters);
                
            }
            
        }

        public void Initialize(int capacity) {

            this.filters = PoolArray<IFilterBase>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public IFilterBase[] GetData() {

            return this.filters;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref IFilterBase Get(int id) {

            return ref this.filters[id - 1];

        }

        public IFilterBase GetByHashCode(int hashCode) {

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

        public IFilterBase GetFilterEquals(IFilterBase other) {
            
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

        public void Register(IFilterBase filter) {

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
                if (this.filters == null) {

                    this.filters = PoolArray<IFilterBase>.Spawn(other.filters.Length);

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
            
        private readonly IFilterInternal set;
        private SortedList<int, Entity>.Enumerator setEnumerator;
            
        internal FilterEnumerator(IFilterInternal set) {
                
            this.set = set;
            this.setEnumerator = this.set.GetData().GetEnumerator();
            this.set.SetForEachMode(true);

        }
 
        public void Dispose() {

            this.set.SetForEachMode(false);

        }
 
        public bool MoveNext() {
            
            return this.setEnumerator.MoveNext();
                
        }
 
        public Entity Current {
            get {
                return this.setEnumerator.Current.Value;
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
    public class Filter : IFilterInternal, IFilter {

        private const int REQUESTS_CAPACITY = 4;
        private const int NODES_CAPACITY = 4;
        private const int ENTITIES_CAPACITY = 100;

        public int id { get; set; }

        public string name {
            get {
                return this.aliases[0];
            }
        }

        public IWorldBase world { get; private set; }
        private IFilterNode[] nodes;
        private Archetype archetypeContains;
        private Archetype archetypeNotContains;
        private int nodesCount;
        private bool[] dataContains;
        private SortedList<int, Entity> data;
        bool IFilterInternal.forEachMode { get; set; }
        private SortedList<int, Entity> requests;
        private SortedList<int, Entity> requestsRemoveEntity;
        
        private List<IFilterNode> tempNodes;
        private List<IFilterNode> tempNodesCustom;
        private string[] aliases;

        #if UNITY_EDITOR
        private string[] editorTypes;
        private string[] editorStackTraceFile;
        private int[] editorStackTraceLineNumber;
        #endif

        public Filter() {}

        internal Filter(string name) {

            this.AddAlias(name);

        }

        public void Recycle() {
            
            PoolFilters.Recycle(this);
            
        }

        public void Update() {

            if (this.world.ForEachEntity(out RefList<Entity> list) == true) {

                for (int i = list.FromIndex; i < list.SizeCount; ++i) {

                    var entity = list[i];
                    if (list.IsFree(i) == true) continue;
                    
                    ((IFilterInternal)this).OnUpdate(entity);

                }

            }

        }

        public IFilterBase Clone() {

            var instance = PoolFilters.Spawn<Filter>();
            instance.CopyFrom(this);
            return instance;

        }

        public string[] GetAllNames() {

            return this.aliases;

        }
        
        public void AddAlias(string name) {

            if (string.IsNullOrEmpty(name) == true) return;
            
            var idx = (this.aliases != null ? this.aliases.Length : 0);
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

            return string.Join(", ", this.editorTypes);

        }

        public void AddTypeToEditorWith<TComponent>() {
            
            var idx = (this.editorTypes != null ? this.editorTypes.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorTypes, resizeWithOffset: false);
            this.editorTypes[idx] = "W<" + typeof(TComponent).Name + ">";
            
        }

        public void AddTypeToEditorWithout<TComponent>() {

            var idx = (this.editorTypes != null ? this.editorTypes.Length : 0);
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
            
            var idx = (this.editorStackTraceFile != null ? this.editorStackTraceFile.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorStackTraceFile, resizeWithOffset: false);
            this.editorStackTraceFile[idx] = file;
            
            idx = (this.editorStackTraceLineNumber != null ? this.editorStackTraceLineNumber.Length : 0);
            ArrayUtils.Resize(idx, ref this.editorStackTraceLineNumber, resizeWithOffset: false);
            this.editorStackTraceLineNumber[idx] = lineNumber;

        }
        #endif
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Entity[] GetArray() {

            var arr = PoolArray<Entity>.Spawn(this.data.Count);
            this.data.CopyTo(arr, 0);
            return arr;

        }

        void IPoolableSpawn.OnSpawn() {

            this.requests = PoolSortedList<int, Entity>.Spawn(Filter.REQUESTS_CAPACITY);
            this.requestsRemoveEntity = PoolSortedList<int, Entity>.Spawn(Filter.REQUESTS_CAPACITY);
            this.nodes = PoolArray<IFilterNode>.Spawn(Filter.NODES_CAPACITY);
            this.data = PoolSortedList<int, Entity>.Spawn(Filter.ENTITIES_CAPACITY);
            this.dataContains = PoolArray<bool>.Spawn(Filter.ENTITIES_CAPACITY);

            this.id = default;
            if (this.aliases != null) PoolArray<string>.Recycle(ref this.aliases);
            this.nodesCount = default;
            this.archetypeContains = default;
            this.archetypeNotContains = default;

            #if UNITY_EDITOR
            if (this.editorTypes != null) PoolArray<string>.Recycle(ref this.editorTypes);
            #endif
            
        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<bool>.Recycle(ref this.dataContains);
            PoolSortedList<int, Entity>.Recycle(ref this.data);
            PoolArray<IFilterNode>.Recycle(ref this.nodes);
            PoolSortedList<int, Entity>.Recycle(ref this.requestsRemoveEntity);
            PoolSortedList<int, Entity>.Recycle(ref this.requests);
            
            if (this.aliases != null) PoolArray<string>.Recycle(ref this.aliases);
            
            #if UNITY_EDITOR
            if (this.editorTypes != null) PoolArray<string>.Recycle(ref this.editorTypes);
            if (this.editorStackTraceFile != null) PoolArray<string>.Recycle(ref this.editorStackTraceFile);
            if (this.editorStackTraceLineNumber != null) PoolArray<int>.Recycle(ref this.editorStackTraceLineNumber);
            #endif

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsForEntity(in Entity entity) {

            ref var previousArchetype = ref ((WorldBase)this.world).storagesCache.archetypes.GetPrevious(in entity);
            ref var currentArchetype = ref ((WorldBase)this.world).storagesCache.archetypes.Get(in entity);

            if (previousArchetype.ContainsAll(this.archetypeContains) == true) return true;
            if (previousArchetype.NotContains(this.archetypeNotContains) == true) return true;

            if (currentArchetype.ContainsAll(this.archetypeContains) == true) return true;
            if (currentArchetype.NotContains(this.archetypeNotContains) == true) return true;

            return false;

        }

        public int GetNodesCount() {

            return this.nodesCount;

        }

        public Archetype GetArchetypeContains() {

            return this.archetypeContains;

        }

        public Archetype GetArchetypeNotContains() {

            return this.archetypeNotContains;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SetForEachMode(bool state) {

            lock (this) {

                var internalFilter = ((IFilterInternal)this);
                internalFilter.forEachMode = state;
                if (state == false) {

                    if (Worlds.currentWorld.currentSystemContext == null) {

                        this.ApplyAllRequests();

                    }
                    
                }

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void ApplyAllRequests() {

            var internalFilter = ((IFilterInternal)this);
            {
                
                var requests = internalFilter.GetRequests();
                foreach (var entity in requests) {

                    internalFilter.OnUpdate(entity.Value);

                }

                requests.Clear();

            }

            {
                
                var requests = internalFilter.GetRequestsRemoveEntity();
                foreach (var entity in requests) {

                    internalFilter.Remove_INTERNAL(entity.Value);

                }

                requests.Clear();

            }

        }

        public void CopyFrom(IFilterBase other) {
            
            this.CopyFrom(other as Filter);
            
        }
        
        public void CopyFrom(Filter other) {

            lock (this) {

                this.id = other.id;
                ArrayUtils.Copy(other.aliases, ref this.aliases);
                this.nodesCount = other.nodesCount;

                this.archetypeContains = other.archetypeContains;
                this.archetypeNotContains = other.archetypeNotContains;
                
                ArrayUtils.Copy(other.nodes, ref this.nodes);

                if (this.data != null) PoolSortedList<int, Entity>.Recycle(ref this.data);
                this.data = PoolSortedList<int, Entity>.Spawn(other.data.Count);
                this.data.CopyFrom(other.data);
                //this.data.CopyFrom(other.data);
                //this.data = new SortedSetCopyable<Entity>(other.data);

                ArrayUtils.Copy(other.dataContains, ref this.dataContains);
                
                #if UNITY_EDITOR
                ArrayUtils.Copy(other.editorTypes, ref this.editorTypes);
                this.editorStackTraceFile = other.editorStackTraceFile;
                this.editorStackTraceLineNumber = other.editorStackTraceLineNumber;
                #endif

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public SortedList<int, Entity> GetData() {

            return this.data;

        }
        
        SortedList<int, Entity> IFilterInternal.GetRequests() {

            return this.requests;

        }

        SortedList<int, Entity> IFilterInternal.GetRequestsRemoveEntity() {

            return this.requestsRemoveEntity;

        }

        public int Count {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.data.Count;
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

            var filter = (Filter)this.world.GetFilter(this.id);
            return filter.Contains_INTERNAL(entity.id);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool Contains_INTERNAL(int entityId) {

            if (entityId < 0 || entityId >= this.dataContains.Length) return false;
            return this.dataContains[entityId];

        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() {

            return ((IEnumerable<Entity>)this.data).GetEnumerator();

        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {

            return ((System.Collections.IEnumerable)this.data).GetEnumerator();

        }

        public FilterEnumerator GetEnumerator() {

            return new FilterEnumerator(this);

        }

        bool IFilterInternal.OnUpdate(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        bool IFilterInternal.OnAddComponent(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        bool IFilterInternal.OnRemoveComponent(in Entity entity) {

            return this.OnUpdate_INTERNAL(in entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private bool OnUpdate_INTERNAL(in Entity entity) {

            lock (this) {

                if (this.world.currentSystemContext != null) {

                    lock (this.world.GetCurrentSystemContextFiltersUsed()) {

                        if (this.world.GetCurrentSystemContextFiltersUsed().ContainsKey(this.id) == false) {

                            this.world.GetCurrentSystemContextFiltersUsed().Add(this.id, this);

                        }

                    }

                    if (this.requests.ContainsKey(entity.version) == false) this.requests.Add(entity.version, entity);
                    return false;

                }

                var cast = (IFilterInternal)this;
                if (cast.forEachMode == true) {

                    if (this.requests.ContainsKey(entity.version) == false) this.requests.Add(entity.version, entity);
                    return false;

                }

                var isExists = this.Contains_INTERNAL(entity.id);
                if (isExists == true) {

                    return this.CheckRemove(in entity);

                } else {

                    return this.CheckAdd(in entity);

                }

            }

        }

        bool IFilterInternal.OnRemoveEntity(in Entity entity) {

            lock (this) {

                if (this.world.currentSystemContext != null) {

                    lock (this.world.GetCurrentSystemContextFiltersUsed()) {

                        if (this.world.GetCurrentSystemContextFiltersUsed().ContainsKey(this.id) == false) {

                            this.world.GetCurrentSystemContextFiltersUsed().Add(this.id, this);

                        }

                    }

                    if (this.requestsRemoveEntity.ContainsKey(entity.version) == false) this.requestsRemoveEntity.Add(entity.version, entity);
                    return false;

                }
                
                var cast = (IFilterInternal)this;
                if (cast.forEachMode == true) {

                    if (this.requestsRemoveEntity.ContainsKey(entity.version) == false) this.requestsRemoveEntity.Add(entity.version, entity);
                    return false;

                }

                return ((IFilterInternal)this).Remove_INTERNAL(entity);

            }
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        void IFilterInternal.Add_INTERNAL(in Entity entity) {

            ArrayUtils.Resize(entity.id, ref this.dataContains);
            this.dataContains[entity.id] = true;
            this.data.Add(entity.id, entity);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        bool IFilterInternal.Remove_INTERNAL(in Entity entity) {

            var idx = entity.id;
            if (idx < 0 || idx >= this.dataContains.Length) return false;
            
            ref var res = ref this.dataContains[idx];
            if (res == true) {

                res = false;
                this.data.Remove(entity.id);
                return true;

            }

            return false;

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

        private bool CheckAdd(in Entity entity) {

            // If entity doesn't exist in cache - try to add if entity's archetype fit with contains & notContains
            ref var entArchetype = ref ((WorldBase)this.world).storagesCache.archetypes.Get(in entity);
            if (entArchetype.ContainsAll(this.archetypeContains) == false) return false;
            if (entArchetype.NotContains(this.archetypeNotContains) == false) return false;

            if (this.nodesCount > 0) {

                for (int i = 0; i < this.nodesCount; ++i) {

                    if (this.nodes[i].Execute(entity) == false) {

                        return false;

                    }

                }

            }

            var cast = (IFilterInternal)this;
            cast.Add_INTERNAL(entity);

            return true;

        }

        private bool CheckRemove(in Entity entity) {

            // If entity already exists in cache - try to remove if entity's archetype doesn't fit with contains & notContains
            ref var entArchetype = ref ((WorldBase)this.world).storagesCache.archetypes.Get(in entity);
            var allContains = entArchetype.ContainsAll(this.archetypeContains);
            var allNotContains = entArchetype.NotContains(this.archetypeNotContains);
            
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

            return ((IFilterInternal)this).Remove_INTERNAL(entity);

        }

        public bool IsEquals(IFilterBase filter) {

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

        public IFilter Push() {

            IFilter filter = null;
            return this.Push(ref filter);

        }

        public IFilter Push(ref IFilter filter) {

            var world = (WorldBase)Worlds.currentWorld;
            var worldInt = Worlds.currentWorld;
            var nextId = world.filtersStorage.GetNextId();
            if (worldInt.HasFilter(nextId) == false) {

                this.tempNodes.AddRange(this.tempNodesCustom);
                this.nodes = this.tempNodes.OrderBy(x => x.GetType().GetHashCode()).ToArray();
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

                    this.id = world.filtersStorage.AllocateNextId();

                    filter = this;
                    this.world = worldInt;
                    world.filtersStorage.RegisterInAllArchetype(this.archetypeContains);
                    world.filtersStorage.RegisterInAllArchetype(this.archetypeNotContains);
                    worldInt.Register(this);
                    
                }

            } else {

                UnityEngine.Debug.LogWarning(string.Format("World #{0} already has filter {1}!", worldInt.id, this));

            }

            return this;

        }

        public IFilter Custom(IFilterNode filter) {

            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter Custom<TFilter>() where TFilter : class, IFilterNode, new() {

            var filter = new TFilter();
            this.tempNodesCustom.Add(filter);
            return this;

        }

        public IFilter WithComponent<TComponent>() where TComponent : class, IComponent {

            //var node = new ComponentExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWith<TComponent>();
            #endif
            return this;

        }
        
        public IFilter WithoutComponent<TComponent>() where TComponent : class, IComponent {

            //var node = new ComponentNotExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeNotContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWithout<TComponent>();
            #endif
            return this;

        }

        public IFilter WithStructComponent<TComponent>() where TComponent : struct, IStructComponent {

            //var node = new ComponentStructExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWith<TComponent>();
            #endif
            return this;

        }

        public IFilter WithoutStructComponent<TComponent>() where TComponent : struct, IStructComponent {

            //var node = new ComponentStructNotExistsFilterNode<TComponent>();
            //this.tempNodes.Add(node);
            this.archetypeNotContains.Add<TComponent>();
            #if UNITY_EDITOR
            this.AddTypeToEditorWithout<TComponent>();
            #endif
            return this;

        }

        public static IFilter Create(string customName = null) {

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

            return "Name: " + this.name + " (" + this.id.ToString() + ")";

        }

    }

}