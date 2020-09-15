
namespace ME.ECS {
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    #if MESSAGE_PACK_SUPPORT
    [MessagePack.MessagePackObjectAttribute()]
    #endif
    public struct FPVector2 : System.IEquatable<FPVector2> {
        
        public static readonly FPVector2 zero = new FPVector2(0, 0);
        public static readonly FPVector2 one = new FPVector2(1, 1);

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(0)]
        #endif
        public pfloat x;
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(1)]
        #endif
        public pfloat y;

        public bool Equals(FPVector2 other) {

            return (this == other);

        }

        public override int GetHashCode() {
            
            return this.x.GetHashCode() ^ this.y.GetHashCode();
            
        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }
        
        public static FPVector2 Rotate(FPVector2 v, float degrees) {
            
            float radians = degrees * FPMath.Deg2Rad;
            float sin = FPMath.Sin(radians);
            float cos = FPMath.Cos(radians);
            
            float tx = v.x;
            float ty = v.y;
            
            return new FPVector2(cos * tx - sin * ty, sin * tx + cos * ty);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public FPVector2(pfloat x, pfloat y) {

            this.x = x;
            this.y = y;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 Normalize(FPVector2 v) {
            
            v.Normalize();
            return v;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Normalize() {
            
            var num = this.magnitude;
            if ((double) num > 9.999999747378752E-06)
                this = this / num;
            else
                this = FPVector2.zero;
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 MoveTowards(
            FPVector2 current,
            FPVector2 target,
            pfloat maxDistanceDelta) {
            
            pfloat num1 = target.x - current.x;
            pfloat num2 = target.y - current.y;
            pfloat num4 = (num1 * num1 + num2 * num2);
            if (num4 == 0f || maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta) return target;
            
            var num5 = FPMath.Sqrt(num4);
            return new FPVector2(current.x + num1 / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta);
            
        }
        
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public pfloat sqrMagnitude {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return (pfloat)((double)this.x * (double)this.x + (double)this.y * (double)this.y);
            }
        }

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public pfloat magnitude {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return FPMath.Sqrt(this.sqrMagnitude);
            }
        }

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public FPVector2 normalized {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return FPVector2.Normalize(this);
            }
        }

        public static FPVector2 Lerp(FPVector2 a, FPVector2 b, pfloat t) {
            
            t = FPMath.Clamp01(t);
            return new FPVector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
            
        }

        public static implicit operator FPVector2(UnityEngine.Vector2 v) {
            
            return new FPVector2(v.x, v.y);
            
        }

        public static implicit operator UnityEngine.Vector2(FPVector2 v) {
            
            return new UnityEngine.Vector2((float)v.x, (float)v.y);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FPVector2 v1, in FPVector2 v2) {

            if (v1.x == v2.x && v1.y == v2.y) return true;
            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FPVector2 v1, in FPVector2 v2) {

            return !(v1 == v2);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 operator +(FPVector2 v1, in FPVector2 v2) {

            v1.x += v2.x;
            v1.y += v2.y;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 operator -(FPVector2 v1, in FPVector2 v2) {

            v1.x -= v2.x;
            v1.y -= v2.y;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 operator *(FPVector2 v1, in FPVector2 v2) {

            v1.x *= v2.x;
            v1.y *= v2.y;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 operator /(FPVector2 v1, in FPVector2 v2) {

            v1.x /= v2.x;
            v1.y /= v2.y;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector2 operator /(FPVector2 v1, in pfloat v2) {

            v1.x /= v2;
            v1.y /= v2;
            return v1;

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    #if MESSAGE_PACK_SUPPORT
    [MessagePack.MessagePackObjectAttribute()]
    #endif
    public struct FPVector3 : System.IEquatable<FPVector3> {
        
        public static readonly FPVector3 zero = new FPVector3(0, 0, 0);
        public static readonly FPVector3 one = new FPVector3(1, 1, 1);

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(0)]
        #endif
        public pfloat x;
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(1)]
        #endif
        public pfloat y;
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(2)]
        #endif
        public pfloat z;

        public bool Equals(FPVector3 other) {

            return (this == other);

        }

        public override int GetHashCode() {
            
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode();
            
        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }
        
        public static FPVector3 Lerp(FPVector3 a, FPVector3 b, pfloat t) {
            
            t = FPMath.Clamp01(t);
            return new FPVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
            
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public FPVector3(pfloat x, pfloat y, pfloat z) {

            this.x = x;
            this.y = y;
            this.z = z;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 Normalize(FPVector3 v) {
            
            v.Normalize();
            return v;

        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Normalize() {
            
            var num = this.magnitude;
            if ((double) num > 9.999999747378752E-06)
                this = this / num;
            else
                this = FPVector3.zero;
        }
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 MoveTowards(
            FPVector3 current,
            FPVector3 target,
            pfloat maxDistanceDelta) {
            
            pfloat num1 = target.x - current.x;
            pfloat num2 = target.y - current.y;
            pfloat num3 = target.z - current.z;
            pfloat num4 = (num1 * num1 + num2 * num2 + num3 * num3);
            if (num4 == 0f || maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta) return target;
            
            var num5 = FPMath.Sqrt(num4);
            return new FPVector3(current.x + num1 / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
            
        }
        
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public pfloat sqrMagnitude {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return (pfloat)((double)this.x * (double)this.x + (double)this.y * (double)this.y + (double)this.z * (double)this.z);
            }
        }

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public pfloat magnitude {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return FPMath.Sqrt(this.sqrMagnitude);
            }
        }

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public FPVector3 normalized {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return FPVector3.Normalize(this);
            }
        }

        public static implicit operator FPVector3(UnityEngine.Vector3 v) {
            
            return new FPVector3(v.x, v.y, v.z);
            
        }

        public static implicit operator UnityEngine.Vector3(FPVector3 v) {
            
            return new UnityEngine.Vector3((float)v.x, (float)v.y, (float)v.z);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FPVector3 v1, in FPVector3 v2) {

            if (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z) return true;
            return false;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FPVector3 v1, in FPVector3 v2) {

            return !(v1 == v2);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 operator +(FPVector3 v1, in FPVector3 v2) {

            v1.x += v2.x;
            v1.y += v2.y;
            v1.z += v2.z;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 operator -(FPVector3 v1, in FPVector3 v2) {

            v1.x -= v2.x;
            v1.y -= v2.y;
            v1.z -= v2.z;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 operator *(FPVector3 v1, in FPVector3 v2) {

            v1.x *= v2.x;
            v1.y *= v2.y;
            v1.z *= v2.z;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 operator /(FPVector3 v1, in FPVector3 v2) {

            v1.x /= v2.x;
            v1.y /= v2.y;
            v1.z /= v2.z;
            return v1;

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static FPVector3 operator /(FPVector3 v1, in pfloat v2) {

            v1.x /= v2;
            v1.y /= v2;
            v1.z /= v2;
            return v1;

        }

    }

}