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

        public static void Throw(string methodName, string description = null) {

            throw new WrongThreadException("Can't use " + methodName + " method from non-world thread" + (string.IsNullOrEmpty(description) == true ? string.Empty : ", " + description) + ".\nTurn off this check by disabling WORLD_THREAD_CHECK.");

        }
        
    }

    public class EmptyEntityException : System.Exception {

        private EmptyEntityException() : base("[ME.ECS] You are trying to change empty entity.") {}

        public static void Throw() {

            throw new EmptyEntityException();

        }

    }

    public class InStateException : System.Exception {

        public InStateException() : base("[ME.ECS] Could not perform action because current step is in state (" + Worlds.currentWorld.GetCurrentStep().ToString() + ").") {}

    }

    public class OutOfStateException : System.Exception {

        public OutOfStateException(string description = "") : base("[ME.ECS] Could not perform action because current step is out of state (" + Worlds.currentWorld.GetCurrentStep().ToString() + "). This could cause out of sync state. " + description) {}

        public static void ThrowWorldStateCheck() {

            throw new OutOfStateException("LogicTick state is required. You can disable this check by turning off WORLD_STATE_CHECK define.");

        }
        
    }

    public class AllocationException : System.Exception {

        public AllocationException() : base("Allocation not allowed!") {}

    }

    public class SystemGroupRegistryException : System.Exception {

        private SystemGroupRegistryException() {}
        private SystemGroupRegistryException(string caption) : base(caption) {}

        public static void Throw() {

            throw new SystemGroupRegistryException("SystemGroup was not registered in world. Be sure you use constructor with parameters (new SystemGroup(name)).");

        }

    }

}