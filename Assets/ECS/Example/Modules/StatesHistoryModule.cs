/// <summary>
/// We need to implement our own StatesHistorySystem class without any logic just to catch our State type into ECS.StatesHistory
/// You can use some overrides to setup history config for your project 
/// </summary>
public class StatesHistoryModule : ME.ECS.StatesHistory.StatesHistoryModule<State> {

    protected override uint GetQueueCapacity() {

        return 20u;

    }

    protected override uint GetTicksPerState() {

        return 100u;

    }

}