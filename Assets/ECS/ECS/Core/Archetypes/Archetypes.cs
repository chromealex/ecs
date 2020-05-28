using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class ComponentTypeCounter {

        public static int counter = 0;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class ComponentType<T> {

        public static int index = -1;

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class ArchetypeEntities : IPoolableRecycle {

        private Archetype[] prevTypes;
        private Archetype[] types;
        
        public void OnRecycle() {

            if (this.prevTypes != null) PoolArray<Archetype>.Recycle(ref this.prevTypes);
            if (this.types != null) PoolArray<Archetype>.Recycle(ref this.types);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Validate(in Entity entity) {

            var id = entity.id;
            ArrayUtils.Resize(id, ref this.types);
            ArrayUtils.Resize(id, ref this.prevTypes);
            
        }

        public void CopyFrom(ArchetypeEntities other) {
            
            ArrayUtils.Copy(other.prevTypes, ref this.prevTypes);
            ArrayUtils.Copy(other.types, ref this.types);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref Archetype GetPrevious(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.prevTypes);
            return ref this.prevTypes[id];

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ref Archetype Get(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.types);
            return ref this.types[id];

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Has<T>(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.types);
            this.types[id].Has<T>();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Set<T>(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.types);
            //ArrayUtils.Resize(id, ref this.prevTypes);
            this.prevTypes[id] = this.Get(entity);
            this.types[id].Add<T>();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Remove<T>(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.types);
            //ArrayUtils.Resize(id, ref this.prevTypes);
            this.prevTypes[id] = this.Get(entity);
            this.types[id].Subtract<T>();

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveAll<T>(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.types);
            //ArrayUtils.Resize(id, ref this.prevTypes);
            this.prevTypes[id].Subtract<T>(); 
            this.types[id].Subtract<T>();

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveAll<T>() {

            ArrayUtils.Copy(this.types, ref this.prevTypes);
            for (int i = 0; i < this.types.Length; ++i) {

                this.types[i].Subtract<T>();

            }

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RemoveAll(in Entity entity) {

            var id = entity.id;
            //ArrayUtils.Resize(id, ref this.types);
            //ArrayUtils.Resize(id, ref this.prevTypes);
            this.prevTypes[id] = this.Get(entity);
            this.types[id].Clear();

        }

    }

    public struct Archetype : System.IEquatable<Archetype> {

        private BitMasks.BitMask value;
        
        public Archetype(BitMasks.BitMask value) {

            this.value = value;
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {

            return this.value == BitMasks.BitMask.None;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool ContainsAll(Archetype archetype) {

            return this.value.Has(archetype.value);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool NotContains(Archetype archetype) {

            return this.value.HasNot(archetype.value);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Has<T>() {

            if (ComponentType<T>.index == -1) ComponentType<T>.index = ++ComponentTypeCounter.counter;
            return this.value.HasBit(in ComponentType<T>.index);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Archetype Add<T>() {

            if (ComponentType<T>.index == -1) ComponentType<T>.index = ++ComponentTypeCounter.counter;
            this.value.AddBit(in ComponentType<T>.index);
            
            return this;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Archetype Subtract<T>() {

            if (ComponentType<T>.index == -1) ComponentType<T>.index = ++ComponentTypeCounter.counter;
            this.value.SubtractBit(in ComponentType<T>.index);

            return this;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public Archetype Clear() {

            this.value = new BitMasks.BitMask();

            return this;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Archetype e1, Archetype e2) {

            return e1.value == e2.value;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Archetype e1, Archetype e2) {

            return !(e1 == e2);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool Equals(Archetype other) {

            return this == other;

        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            
            return this.value.GetHashCode();
            
        }

        public override string ToString() {
            
            return this.value.ToString();
            
        }

    }

}
