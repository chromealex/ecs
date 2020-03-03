namespace ME.ECS {

    using UnityEngine;
    
    public static class VectorExt {

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

    public static class MathUtils {

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
        
        public static void GetOrientation(out int orientation, Vector2 from, Vector2 to) {

            const float step = 360f / 8f;
            const float stepHalf = step * 0.5f;

            var ang = System.Math.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg + stepHalf;
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