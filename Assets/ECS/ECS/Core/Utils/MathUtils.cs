namespace ME.ECS {

    using UnityEngine;
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class VectorExt {

        public static FPVector2 Abs(FPVector2 v) {
            
            return new FPVector2(FPMath.Abs(v.x), FPMath.Abs(v.y));
            
        }

        public static FPVector3 XY(this FPVector2 v) {
            
            return new FPVector3(v.x, v.y, 0f);
            
        }
        
        public static FPVector3 XZ(this FPVector2 v) {
            
            return new FPVector3(v.x, 0f, v.y);
            
        }

        public static FPVector2 XY(this FPVector3 v) {
            
            return new FPVector2(v.x, v.y);
            
        }
        
        public static FPVector2 XZ(this FPVector3 v) {
            
            return new FPVector2(v.x, v.z);
            
        }

        
        
        public static Vector2 Abs(Vector2 v) {
            
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
            
        }

        public static Vector3 XY(this Vector2 v) {
            
            return new Vector3(v.x, v.y, 0f);
            
        }
        
        public static Vector3 XZ(this Vector2 v) {
            
            return new Vector3(v.x, 0f, v.y);
            
        }

        public static Vector2 XY(this Vector3 v) {
            
            return new Vector2(v.x, v.y);
            
        }
        
        public static Vector2 XZ(this Vector3 v) {
            
            return new Vector2(v.x, v.z);
            
        }

        public static Vector3Int XY(this Vector2Int v) {
            
            return new Vector3Int(v.x, v.y, 0);
            
        }
        
        public static Vector3Int XZ(this Vector2Int v) {
            
            return new Vector3Int(v.x, 0, v.y);
            
        }

        public static Vector2Int XY(this Vector3Int v) {
            
            return new Vector2Int(v.x, v.y);
            
        }
        
        public static Vector2Int XZ(this Vector3Int v) {
            
            return new Vector2Int(v.x, v.z);
            
        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class MathUtils {

        public static bool IsUnityMathematicsUsed() {
            
            #if UNITY_MATHEMATICS
            return true;
            #else
            return false;
            #endif

        }

        public static Vector2Int GetSpiralPointByIndex(Vector2Int center, int index) {

            if (index == 0) return center;
            
            // given n an index in the squared spiral
            // p the sum of point in inner square
            // a the position on the current square
            // n = p + a

            var pos = Vector2Int.zero;
            var n = index;
            var r = Mathf.FloorToInt((Mathf.Sqrt(n + 1) - 1) / 2) + 1;

            // compute radius : inverse arithmetic sum of 8+16+24+...=
            var p = (8 * r * (r - 1)) / 2;
            // compute total point on radius -1 : arithmetic sum of 8+16+24+...

            var en = r * 2;
            // points by face

            var a = (1 + n - p) % (r * 8);
            // compute de position and shift it so the first is (-r,-r) but (-r+1,-r)
            // so square can connect

            //var pos = [0, 0, r];
            switch (Mathf.FloorToInt(a / (r * 2f))) {
                // find the face : 0 top, 1 right, 2, bottom, 3 left
                case 0:
                {
                    pos[0] = a - r;
                    pos[1] = -r;
                }
                    break;
                case 1:
                {
                    pos[0] = r;
                    pos[1] = (a % en) - r;

                }
                    break;
                case 2:
                {
                    pos[0] = r - (a % en);
                    pos[1] = r;
                }
                    break;
                case 3:
                {
                    pos[0] = -r;
                    pos[1] = r - (a % en);
                }
                    break;
            }
            
            return center + pos;
            
        }

        public static string SecondsToString(double seconds) {
            
            var parts = new System.Collections.Generic.List<string>();
            System.Action<int, string, string> add = (val, unit, format) => { if (val > 0) parts.Add(val.ToString(format) + unit); };
            var t = System.TimeSpan.FromSeconds(seconds);

            add(t.Days, "d", "#");
            add(t.Hours, "h", "#");
            add(t.Minutes, "m", "#");
            add(t.Seconds, "s", "#");
            add(t.Milliseconds, "ms", "000");

            return string.Join(" ", parts);
            
        }

        public static string BytesCountToString(int count) {
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int order = 0;
            while (count >= 1024 && order < sizes.Length - 1) {
                
                ++order;
                count = count / 1024;
                
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return string.Format("{0:0.##} {1}", count, sizes[order]);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static long GetKey(int a1, int a2) {
            
            return (((long)a2 << 32) | (uint)a1);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong GetKey(uint a1, uint a2) {
            
            return (((ulong)a2 << 32) | (uint)a1);
            
        }

        public static int GetOrientation(Vector2 dir) {

            MathUtils.GetOrientation(out var d, dir);
            return d;

        }

        public static int GetOrientation(Vector2 from, Vector2 to) {

            MathUtils.GetOrientation(out var d, to - from);
            return d;

        }
        
        public static void GetOrientation(out int orientation, Vector2 dir) {

            const float step = 360f / 8f;
            const float stepHalf = step * 0.5f;

            var ang = System.Math.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + stepHalf;
            if (ang < 0f) ang = 360f + ang;
            if (ang > 360f) ang -= 360f;

            var sector = (int)(ang / step);

            switch (sector) {
			
                case 0:
                    orientation = 2;
                    break;
			
                case 1:
                    orientation = 1;
                    break;
			
                case 2:
                    orientation = 0;
                    break;
			
                case 3:
                    orientation = 7;
                    break;
			
                case 4:
                    orientation = 6;
                    break;
			
                case 5:
                    orientation = 5;
                    break;
			
                case 6:
                    orientation = 4;
                    break;
			
                case 7:
                    orientation = 3;
                    break;
			
                default:
                    orientation = 0;
                    break;
            }

        }

    }

}