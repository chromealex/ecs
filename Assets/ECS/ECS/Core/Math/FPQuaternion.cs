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
    public struct FPQuaternion : System.IEquatable<FPQuaternion> {
        
        public static readonly FPQuaternion identity = new FPQuaternion(0, 0, 0, 1);
        public static UnityEngine.Quaternion q;

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
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(3)]
        #endif
        public pfloat w;

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public FPQuaternion(pfloat x, pfloat y, pfloat z, pfloat w) {

            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;

        }

        public override int GetHashCode() {
            
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode() ^ this.w.GetHashCode();
            
        }

        public bool Equals(FPQuaternion other) {

            return (this == other);

        }

        public override bool Equals(object obj) {
            
            throw new AllocationException();
            
        }

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public FPVector3 eulerAngles {
            get {

                FPVector3 angles;
                var sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
                var cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
                angles.x = FPMath.Atan2(sinr_cosp, cosr_cosp);

                // pitch (y-axis rotation)
                var sinp = 2 * (q.w * q.y - q.z * q.x);
                if (FPMath.Abs(sinp) >= 1)
                    angles.y = pfloat.Pi / 2 * FPMath.Sign(sinp); // use 90 degrees if out of range
                else
                    angles.y = FPMath.ASin(sinp);

                // yaw (z-axis rotation)
                var siny_cosp = 2 * (q.w * q.z + q.x * q.y);
                var cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
                angles.z = FPMath.Atan2(siny_cosp, cosy_cosp);
                
                return angles;
            }
        }

        public static FPQuaternion Euler(FPVector3 v) {

            return Euler(v.x, v.y, v.z);

        }

        public static FPQuaternion Euler(pfloat roll, pfloat pitch, pfloat yaw) {
            
            FPQuaternion quaternion;
            var num9 = roll * 0.5f;
            var num6 = FPMath.Sin(num9);
            var num5 = FPMath.Cos(num9);
            var num8 = pitch * 0.5f;
            var num4 = FPMath.Sin(num8);
            var num3 = FPMath.Cos(num8);
            var num7 = yaw * 0.5f;
            var num2 = FPMath.Sin(num7);
            var num = FPMath.Cos(num7);
            quaternion.x = ((num * num4) * num5) + ((num2 * num3) * num6);
            quaternion.y = ((num2 * num3) * num5) - ((num * num4) * num6);
            quaternion.z = ((num * num3) * num6) - ((num2 * num4) * num5);
            quaternion.w = ((num * num3) * num5) + ((num2 * num4) * num6);
            return quaternion;
            
        }

        public static FPQuaternion Slerp(FPQuaternion quaternion1, FPQuaternion quaternion2, pfloat amount) {
            
            pfloat num2;
            pfloat num3;
            FPQuaternion quaternion;
            pfloat num = amount;
            pfloat num4 = (((quaternion1.x * quaternion2.x) + (quaternion1.y * quaternion2.y)) + (quaternion1.z * quaternion2.z)) + (quaternion1.w * quaternion2.w);
            bool flag = false;
            if (num4 < 0f)
            {
                flag = true;
                num4 = -num4;
            }
            if (num4 > 0.999999f)
            {
                num3 = 1f - num;
                num2 = flag ? -num : num;
            }
            else
            {
                pfloat num5 = FPMath.ACos(num4);
                pfloat num6 = (1.0f / FPMath.Sin(num5));
                num3 = (FPMath.Sin(((1f - num) * num5))) * num6;
                num2 = flag ? ((-FPMath.Sin((num * num5))) * num6) : ((FPMath.Sin((num * num5))) * num6);
            }
            quaternion.x = (num3 * quaternion1.x) + (num2 * quaternion2.x);
            quaternion.y = (num3 * quaternion1.y) + (num2 * quaternion2.y);
            quaternion.z = (num3 * quaternion1.z) + (num2 * quaternion2.z);
            quaternion.w = (num3 * quaternion1.w) + (num2 * quaternion2.w);
            return quaternion;
            
        }

        public static FPQuaternion Inverse(FPQuaternion quaternion) {
            
            quaternion.Inverse();
            return quaternion;

        }
        
        public void Inverse() {
            
            var num2 = (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z)) + (this.w * this.w);
            var num = 1f / num2;
            this.x = -this.x * num;
            this.y = -this.y * num;
            this.z = -this.z * num;
            this.w = this.w * num;
            
        }

        public static implicit operator FPQuaternion(UnityEngine.Quaternion v) {
            
            return new FPQuaternion(v.x, v.y, v.z, v.w);
            
        }

        public static FPQuaternion operator *(FPQuaternion lhs, FPQuaternion rhs) => new FPQuaternion((float) ((double) lhs.w * (double) rhs.x + (double) lhs.x * (double) rhs.w + (double) lhs.y * (double) rhs.z - (double) lhs.z * (double) rhs.y), (float) ((double) lhs.w * (double) rhs.y + (double) lhs.y * (double) rhs.w + (double) lhs.z * (double) rhs.x - (double) lhs.x * (double) rhs.z), (float) ((double) lhs.w * (double) rhs.z + (double) lhs.z * (double) rhs.w + (double) lhs.x * (double) rhs.y - (double) lhs.y * (double) rhs.x), (float) ((double) lhs.w * (double) rhs.w - (double) lhs.x * (double) rhs.x - (double) lhs.y * (double) rhs.y - (double) lhs.z * (double) rhs.z));

        public static FPVector3 operator *(FPQuaternion rotation, FPVector3 point) {
            
            var num1 = rotation.x * 2f;
            var num2 = rotation.y * 2f;
            var num3 = rotation.z * 2f;
            var num4 = rotation.x * num1;
            var num5 = rotation.y * num2;
            var num6 = rotation.z * num3;
            var num7 = rotation.x * num2;
            var num8 = rotation.x * num3;
            var num9 = rotation.y * num3;
            var num10 = rotation.w * num1;
            var num11 = rotation.w * num2;
            var num12 = rotation.w * num3;
            FPVector3 vector3;
            vector3.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) point.x + ((double) num7 - (double) num12) * (double) point.y + ((double) num8 + (double) num11) * (double) point.z);
            vector3.y = (float) (((double) num7 + (double) num12) * (double) point.x + (1.0 - ((double) num4 + (double) num6)) * (double) point.y + ((double) num9 - (double) num10) * (double) point.z);
            vector3.z = (float) (((double) num8 - (double) num11) * (double) point.x + ((double) num9 + (double) num10) * (double) point.y + (1.0 - ((double) num4 + (double) num5)) * (double) point.z);
            return vector3;
            
        }

        public static FPVector2 operator *(FPQuaternion rotation, FPVector2 point) {
            
            var num1 = rotation.x * 2f;
            var num2 = rotation.y * 2f;
            var num3 = rotation.z * 2f;
            var num4 = rotation.x * num1;
            var num5 = rotation.y * num2;
            var num6 = rotation.z * num3;
            var num7 = rotation.x * num2;
            var num12 = rotation.w * num3;
            FPVector2 vector3;
            vector3.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) point.x + ((double) num7 - (double) num12) * (double) point.y);
            vector3.y = (float) (((double) num7 + (double) num12) * (double) point.x + (1.0 - ((double) num4 + (double) num6)) * (double) point.y);
            return vector3;
            
        }

        public static implicit operator UnityEngine.Quaternion(FPQuaternion q) {
            
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
            
        }

        public static bool operator ==(FPQuaternion q1, FPQuaternion q2) {
            
            return q1.x == q2.x &&
                   q1.y == q2.y &&
                   q1.z == q2.z &&
                   q1.w == q2.w;
            
        }

        public static bool operator !=(FPQuaternion q1, FPQuaternion q2) {
            
            return !(q1 == q2);

        }

    }

}