#if FPS_MODULE_SUPPORT
using System.Collections.Generic;
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    public interface IFPSModuleBase {}

    public interface IFPSModule<TState> : IFPSModuleBase, IModule<TState> where TState : class, IState<TState> {

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class FPSModule<TState> : IFPSModule<TState> where TState : class, IState<TState> {

        private float timeElapsed;
        private int framesElapsed;
        private int fps;
        private int minFps;
        private int maxFps;
        private int targetFps;
        
        public IWorld<TState> world { get; set; }

        void IModule<TState>.OnConstruct() {

            UnityEngine.Application.targetFrameRate = 120;
            this.minFps = int.MaxValue;
            this.maxFps = int.MinValue;
            this.fps = 0;
            this.targetFps = UnityEngine.Application.targetFrameRate;

        }

        void IModule<TState>.OnDeconstruct() {

        }

        void IModule<TState>.AdvanceTick(TState state, float deltaTime) {}

        void IModule<TState>.Update(TState state, float deltaTime) {

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

        public override string ToString() {

            return "<b>FPS:</b> " + this.fps.ToString() + ", <b>Max FPS:</b> " + this.maxFps.ToString() + ", <b>Min FPS:</b> " + this.minFps.ToString() + ", <b>Target FPS:</b> " + this.targetFps.ToString();

        }

    }

}
#endif