namespace ME.Example.Game.Modules {

    /// <summary>
    /// We need to implement our own StatesHistoryModule class without any logic just to catch our State type into ECS.StatesHistory
    /// You can use some overrides to setup history config for your project 
    /// </summary>
    public class StatesHistoryModule : ME.ECS.StatesHistory.StatesHistoryModule<State> {

        protected override uint GetQueueCapacity() {

            return 10u;

        }

        protected override uint GetTicksPerState() {

            return 20u;

        }

    }

}