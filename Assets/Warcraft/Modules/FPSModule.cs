namespace Warcraft.Modules {
    
    using TState = WarcraftState;
    
    /// <summary>
    /// We need to implement our own FPSModule class without any logic just to catch your State type into ECS.FPSModule
    /// You can use some overrides to setup FPS config for your project
    /// </summary>
    public class FPSModule : ME.ECS.FPSModule<TState> {
        
    }
    
}