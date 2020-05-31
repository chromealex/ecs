namespace ME.ECS {

    public class OutOfBoundsException : System.Exception {

        public OutOfBoundsException() : base("ME.ECS Exception") { }
        public OutOfBoundsException(string message) : base(message) { }

    }
    
    public class StateNotFoundException : System.Exception {

        public StateNotFoundException() : base("ME.ECS Exception") { }
        public StateNotFoundException(string message) : base(message) { }

    }

    public class WrongThreadException : System.Exception {

        public WrongThreadException() : base("ME.ECS Exception") { }
        public WrongThreadException(string message) : base(message) { }

    }

    public class InStateException : System.Exception {

        public InStateException() : base("[ME.ECS] Could not perform action because current step is in state (" + Worlds.currentWorld.GetCurrentStep().ToString() + ").") {}

    }

    public class OutOfStateException : System.Exception {

        public OutOfStateException(string description = "") : base("[ME.ECS] Could not perform action because current step is out of state (" + Worlds.currentWorld.GetCurrentStep().ToString() + "). This could cause out of sync state. " + description) {}

        public static void ThrowWorldState() {

            throw new OutOfStateException("LogicTick state is required. You can disable this check by turning off WORLD_STATE_CHECK define.");

        }
        
    }

    public class AllocationException : System.Exception {

        public AllocationException() : base("Allocation not allowed!") {}

    }

}