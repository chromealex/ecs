#if FPS_MODULE_SUPPORT

namespace ME.ECS {

    public interface IFPSModuleBase : IModuleBase {

        int fps { get; set; }
        int minFps { get; set; }
        int maxFps { get; set; }
        int targetFps { get; set; }

    }

    public interface IFPSModule<TState> : IFPSModuleBase, IModule where TState : State, new() {

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class FPSModule<TState> : IFPSModule<TState>, IUpdate where TState : State, new() {

        private float timeElapsed;
        private int framesElapsed;
        public int fps { get; set; }
        public int minFps { get; set; }
        public int maxFps { get; set; }
        public int targetFps { get; set; }
        
        public World world { get; set; }

        void IModuleBase.OnConstruct() {

            //UnityEngine.Application.targetFrameRate = 120;
            this.minFps = int.MaxValue;
            this.maxFps = int.MinValue;
            this.fps = 0;
            this.targetFps = UnityEngine.Application.targetFrameRate;

        }

        void IModuleBase.OnDeconstruct() {

        }

        public void Update(in float deltaTime) {

            const float checkTime = 1f;
            
            this.timeElapsed += deltaTime;
            ++this.framesElapsed;

            if (this.timeElapsed > checkTime) {

                this.fps = this.framesElapsed;
                if (this.fps < this.minFps) this.minFps = this.fps;
                if (this.fps > this.maxFps) this.maxFps = this.fps;
                
                this.framesElapsed = 0;
                this.timeElapsed -= checkTime;

            }

        }

    }

}
#endif