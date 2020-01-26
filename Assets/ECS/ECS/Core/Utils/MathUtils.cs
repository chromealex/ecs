namespace ME.ECS {

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

    }

}