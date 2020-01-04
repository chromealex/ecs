using NUnit.Framework;

namespace ME.ECS.Tests {

    public static class TestsHelper {

        public static World<State> CreateWorld() {
            
            World<State> world = null;
            WorldUtilities.CreateWorld(ref world, 0.05f);
            world.SetState(world.CreateState());

            return world;

        }

        public static void ReleaseWorld(ref World<State> world) {

            var state = world.GetState();
            WorldUtilities.ReleaseWorld(ref world);
            TestsHelper.TestStateIsClean(state);

        }

        public static T GetValue<T>(object instance, string field) {
            
            var fieldObj = instance.GetType().GetField(field, System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldObj == null) return default(T);
            return (T)fieldObj.GetValue(instance);

        }

        public static void TestStateIsClean(State state) {
            
            Assert.IsTrue(state.entityId == 0);
            Assert.IsTrue(state.points == null);
            Assert.IsTrue(state.units == null);
            Assert.IsTrue(state.pointComponents == null);
            Assert.IsTrue(state.unitComponents == null);
            
        }

    }

}
