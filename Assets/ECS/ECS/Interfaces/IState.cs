#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace ME.ECS {

    public interface IStateBase {

        int entityId { get; set; }
        Tick tick { get; set; }
        RandomState randomState { get; set; }

        int GetHash();

    }

    public interface IState<T> : IStateBase, IPoolableRecycle where T : class, IState<T>, new() {

        void Initialize(IWorld<T> world, bool freeze, bool restore);
        void CopyFrom(T other);

    }

}