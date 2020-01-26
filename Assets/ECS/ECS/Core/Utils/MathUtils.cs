namespace ME.ECS {

    public static class MathUtils {

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