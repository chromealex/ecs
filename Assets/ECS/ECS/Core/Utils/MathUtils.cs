namespace ME.ECS {

    public static class MathUtils {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static long GetKey(int a1, int a2) {
            
            return (((long)a2 << 32) | (uint)a1);
            
        }

    }

}