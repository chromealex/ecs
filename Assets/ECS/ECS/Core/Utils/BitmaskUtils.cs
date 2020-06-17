using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ME.ECS {

    using FieldType = UInt64;

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct BitMask : IEquatable<BitMask> {

        private const int FIELD_COUNT = 4;
        private const int BITS_PER_FIELD = 8 * sizeof(FieldType);
        public const int MAX_BIT_INDEX = BitMask.FIELD_COUNT * BitMask.BITS_PER_FIELD - 1;
        //public const int BitSize = BitMask.FIELD_COUNT * BitMask.BITS_PER_FIELD;
        
        private FieldType field0;
        private FieldType field1;
        private FieldType field2;
        private FieldType field3;

        public BitMask(in FieldType field0, in FieldType field1, in FieldType field2, in FieldType field3) {
            
            this.field0 = field0;
            this.field1 = field1;
            this.field2 = field2;
            this.field3 = field3;

        }

        public int Count {
            #if ECS_COMPILE_IL2CPP_OPTIONS
            [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
             Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
             Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
            #endif
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                var count = 0;
                for (int i = 0; i < BitMask.MAX_BIT_INDEX; ++i) {
                    if (this.HasBit(i) == true) ++count;
                }
                return count;
            }
        }

        public int BitsCount {
            #if ECS_COMPILE_IL2CPP_OPTIONS
            [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
             Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
             Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
            #endif
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return BitMask.MAX_BIT_INDEX;
            }
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool HasBit(in int bit) {
            
            if (bit < 0 || bit > BitMask.MAX_BIT_INDEX) {
                throw new Exception($"Attempted to set bit #{bit}, but the maximum is {BitMask.MAX_BIT_INDEX}");
            }

            var dataIndex = bit / BitMask.BITS_PER_FIELD;
            var bitIndex = bit % BitMask.BITS_PER_FIELD;
            var mask = (FieldType)1 << bitIndex;
            switch (dataIndex) {
                case 0: return (this.field0 & mask) != 0;
                case 1: return (this.field1 & mask) != 0;
                case 2: return (this.field2 & mask) != 0;
                case 3: return (this.field3 & mask) != 0;
                
                default:
                    throw new Exception($"Nonexistent field: {dataIndex}");
            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void AddBits(in BitMask bits) {

            this.field0 |= bits.field0;
            this.field1 |= bits.field1;
            this.field2 |= bits.field2;
            this.field3 |= bits.field3;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void AddBit(in int bit) {
            
            #if UNITY_EDITOR
            if (bit < 0 || bit > BitMask.MAX_BIT_INDEX) {
                throw new Exception($"Attempted to set bit #{bit}, but the maximum is {BitMask.MAX_BIT_INDEX}");
            }
            #endif

            var dataIndex = bit / BitMask.BITS_PER_FIELD;
            var bitIndex = bit % BitMask.BITS_PER_FIELD;
            var mask = (FieldType)1 << bitIndex;
            switch (dataIndex) {
                case 0:
                    this.field0 |= mask;
                    break;

                case 1:
                    this.field1 |= mask;
                    break;

                case 2:
                    this.field2 |= mask;
                    break;

                case 3:
                    this.field3 |= mask;
                    break;

                default:
                    throw new Exception($"Nonexistent field: {dataIndex}");
            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Clear() {

            this.field0 = 0;
            this.field1 = 0;
            this.field2 = 0;
            this.field3 = 0;

        }
        
        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SubtractBit(in int bit) {
            
            if (bit < 0 || bit > BitMask.MAX_BIT_INDEX) {
                throw new Exception($"Attempted to set bit #{bit}, but the maximum is {BitMask.MAX_BIT_INDEX}");
            }

            var dataIndex = bit / BitMask.BITS_PER_FIELD;
            var bitIndex = bit % BitMask.BITS_PER_FIELD;
            var mask = (FieldType)1 << bitIndex;
            switch (dataIndex) {
                case 0:
                    this.field0 &= ~mask;
                    break;

                case 1:
                    this.field1 &= ~mask;
                    break;

                case 2:
                    this.field2 &= ~mask;
                    break;

                case 3:
                    this.field3 &= ~mask;
                    break;
                
                default:
                    throw new Exception($"Nonexistent field: {dataIndex}");
            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public bool this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                if (index < 0 || index > BitMask.MAX_BIT_INDEX) {
                    throw new Exception($"Invalid bit index: {index}");
                }

                var dataIndex = index / BitMask.BITS_PER_FIELD;
                var bitIndex = index % BitMask.BITS_PER_FIELD;
                switch (dataIndex) {
                    case 0:
                        return (this.field0 & ((FieldType)1 << bitIndex)) != 0;

                    case 1:
                        return (this.field1 & ((FieldType)1 << bitIndex)) != 0;

                    case 2:
                        return (this.field2 & ((FieldType)1 << bitIndex)) != 0;

                    case 3:
                        return (this.field3 & ((FieldType)1 << bitIndex)) != 0;

                    default:
                        return false;
                }
            }
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            
            return (int)this.field0 ^ (int)this.field1 ^ (int)this.field2 ^ (int)this.field3;
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitMask other) {
            
            if (this.field0 != other.field0) {
                return false;
            }

            if (this.field1 != other.field1) {
                return false;
            }

            if (this.field2 != other.field2) {
                return false;
            }

            if (this.field3 != other.field3) {
                return false;
            }

            return true;
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public override bool Equals(object obj) {
            if (obj is BitMask) {
                return this.Equals((BitMask)obj);
            }

            return base.Equals(obj);
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BitMask mask1, BitMask mask2) {
            return mask1.Equals(mask2);
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BitMask mask1, BitMask mask2) {
            return !mask1.Equals(mask2);
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            
            return this.field0 == 0 && this.field1 == 0 && this.field2 == 0 && this.field3 == 0;
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitMask operator &(BitMask mask1, BitMask mask2) {

            BitMask newBitMask;
            newBitMask.field0 = mask1.field0 & mask2.field0;
            newBitMask.field1 = mask1.field1 & mask2.field1;
            newBitMask.field2 = mask1.field2 & mask2.field2;
            newBitMask.field3 = mask1.field3 & mask2.field3;
            return newBitMask;
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitMask operator |(BitMask mask1, BitMask mask2) {

            BitMask newBitMask;
            newBitMask.field0 = mask1.field0 | mask2.field0;
            newBitMask.field1 = mask1.field1 | mask2.field1;
            newBitMask.field2 = mask1.field2 | mask2.field2;
            newBitMask.field3 = mask1.field3 | mask2.field3;
            return newBitMask;
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitMask operator ~(BitMask mask) {
            
            BitMask newBitMask;
            newBitMask.field0 = ~mask.field0;
            newBitMask.field1 = ~mask.field1;
            newBitMask.field2 = ~mask.field2;
            newBitMask.field3 = ~mask.field3;
            return newBitMask;
            
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(in BitMask mask) {
            
            if ((this.field0 & mask.field0) != mask.field0 ||
                (this.field1 & mask.field1) != mask.field1 ||
                (this.field2 & mask.field2) != mask.field2 ||
                (this.field3 & mask.field3) != mask.field3) {
                return false;
            }

            return true;
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasNot(in BitMask mask) {
            
            if ((this.field0 & mask.field0) != 0 ||
                (this.field1 & mask.field1) != 0 ||
                (this.field2 & mask.field2) != 0 ||
                (this.field3 & mask.field3) != 0) {
                return false;
            }

            return true;
        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public override string ToString() {
            var builder = new StringBuilder();
            var fields = new FieldType[BitMask.FIELD_COUNT];
            fields[0] = this.field0;
            fields[1] = this.field1;
            fields[2] = this.field2;
            fields[3] = this.field3;
            for (var i = 0; i < BitMask.FIELD_COUNT; ++i) {
                var binaryString = Convert.ToString((long)fields[i], 2);
                builder.Append(binaryString.PadLeft(BitMask.BITS_PER_FIELD, '0'));
            }

            return builder.ToString();
        }

    }

}