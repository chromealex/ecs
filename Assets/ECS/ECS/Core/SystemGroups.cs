namespace ME.ECS {

    using ME.ECS.Collections;
    using System.Collections.Generic;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct SystemGroup {

        public string name;
        internal World world;
        internal BufferArray<ISystemBase> systems;
        internal BufferArray<ModuleState> statesSystems;
        internal int length;
        internal int worldIndex;

        public SystemGroup(string name) : this(Worlds.currentWorld, name) {}

        public SystemGroup(World world, string name) {

            this.name = name;
            this.world = world;
            this.worldIndex = -1;
            this.systems = new BufferArray<ISystemBase>();
            this.statesSystems = new BufferArray<ModuleState>();
            this.length = 0;
            this.worldIndex = world.AddSystemGroup(ref this);

        }
        
        internal void Deconstruct() {
            
            for (int i = 0; i < this.systems.Count; ++i) {
                
                this.systems.arr[i].OnDeconstruct();
                if (this.systems.arr[i] is ISystemFilter systemFilter) {

                    systemFilter.filter = null;

                }
                PoolSystems.Recycle(this.systems.arr[i]);

            }
            PoolArray<ISystemBase>.Recycle(ref this.systems);
            PoolArray<ModuleState>.Recycle(ref this.statesSystems);

        }

        public bool SetSystemState(ISystemBase system, ModuleState state) {
            
            var index = this.systems.IndexOf(system);
            if (index >= 0) {

                this.statesSystems.arr[index] = state;
                return true;

            }

            return false;

        }

        public bool GetSystemState(ISystemBase system, out ModuleState state) {
            
            var index = this.systems.IndexOf(system);
            if (index >= 0) {

                state = this.statesSystems.arr[index];
                return true;

            }

            state = ModuleState.AllActive;
            return false;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsSystemActive(int index) {

            var step = this.world.currentStep;
            if ((step & WorldStep.LogicTick) != 0) {

                return (this.statesSystems.arr[index] & ModuleState.LogicInactive) == 0;

            } else if ((step & WorldStep.VisualTick) != 0) {

                return (this.statesSystems.arr[index] & ModuleState.VisualInactive) == 0;

            }

            return false;

        }

        /// <summary>
        /// Returns true if system with TSystem type exists
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public bool HasSystem<TSystem>() where TSystem : class, ISystemBase, new() {

            if (this.world == null) {
                
                SystemGroupRegistryException.Throw();
                
            }

            for (int i = 0, count = this.systems.Length; i < count; ++i) {

                if (this.systems.arr[i] is TSystem) return true;

            }

            return false;

        }

        /// <summary>
        /// Add system by type
        /// Retrieve system from pool, OnConstruct() call
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        public bool AddSystem<TSystem>() where TSystem : class, ISystemBase, new() {

            if (this.world == null) {
                
                SystemGroupRegistryException.Throw();
                
            }

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
        public bool AddSystem(ISystemBase instance) {

            if (this.world == null) {
                
                SystemGroupRegistryException.Throw();
                
            }

            WorldUtilities.SetWorld(this.world);
            
            instance.world = this.world;
            if (instance is ISystemValidation instanceValidate) {

                if (instanceValidate.CouldBeAdded() == false) {
                    
                    instance.world = null;
                    return false;
                    
                }

            }

            var k = this.length;
            ArrayUtils.Resize(in k, ref this.systems, resizeWithOffset: false);
            ArrayUtils.Resize(in k, ref this.statesSystems, resizeWithOffset: false);
            ++this.length;
            
            this.systems.arr[k] = instance;
            this.statesSystems.arr[k] = ModuleState.AllActive;
            instance.OnConstruct();

            if (instance is ISystemFilter systemFilter) {

                systemFilter.filter = systemFilter.CreateFilter();

            }

            this.world.UpdateGroup(this);
            
            return true;

        }

        /// <summary>
        /// Remove system manually
        /// Pool will not be used, OnDeconstruct() call
        /// </summary>
        /// <param name="instance"></param>
        public void RemoveSystem(ISystemBase instance) {

            if (this.world == null) {
                
                SystemGroupRegistryException.Throw();
                
            }

            var idx = this.systems.IndexOf(instance);
            if (idx >= 0) {
                
                if (instance is ISystemFilter systemFilter) {

                    systemFilter.filter = null;
                    
                }
                
                instance.world = null;
                this.systems = this.systems.RemoveAt(idx);
                this.statesSystems = this.statesSystems.RemoveAt(idx);
                instance.OnDeconstruct();
                
                this.world.UpdateGroup(this);

            }
            
        }

        /// <summary>
        /// Get first system by type
        /// </summary>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public TSystem GetSystem<TSystem>() where TSystem : class, ISystemBase {

            if (this.world == null) {
                
                SystemGroupRegistryException.Throw();
                
            }

            for (int i = 0, count = this.systems.Count; i < count; ++i) {

                var system = this.systems.arr[i];
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

            if (this.world == null) {
                
                SystemGroupRegistryException.Throw();
                
            }

            for (int i = 0, count = this.systems.Count; i < count; ++i) {

                var system = this.systems.arr[i];
                if (system is TSystem tSystem) {

                    PoolSystems.Recycle(tSystem);
                    this.systems = this.systems.RemoveAt(i);
                    this.statesSystems = this.statesSystems.RemoveAt(i);
                    system.OnDeconstruct();
                    --i;
                    --count;

                }

            }

            this.world.UpdateGroup(this);

        }
        
    }

}