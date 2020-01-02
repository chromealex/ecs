using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {

    public class CopyState {

        [Test]
        public void CopyStatePass() {
            
            // Initialize
            var world = new World<State>();
            world.SetState(new State());
            world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            world.AddSystem(new PointsSystem());
            world.AddComponent<IncreaseUnits, Point>(new Entity() { id = 1 });
            
            // Save state
            var state = world.GetState();
            var savedState = (IState<State>)new State();
            savedState.Initialize(world, freeze: true, restore: false);
            savedState.CopyFrom(state);
            
            world.Update(1f);

            // Restore state
            world.SetState((State)savedState);

            {
                var dic = this.GetValue<Dictionary<int, IList>>(world, "entitiesCache");
                Assert.IsTrue(dic.Count == 1);
            }

            {
                var dic = this.GetValue<Dictionary<int, IList>>(world, "filtersCache");
                Assert.IsTrue(dic.Count == 2);
            }

            {
                var dic = this.GetValue<Dictionary<int, IComponents>>(world, "componentsCache");
                Assert.IsTrue(dic.Count == 1);
            }

            {
                Point data;
                world.GetEntityData(1, out data);
                Assert.IsTrue(data.unitsCount == 99f);
            }

        }

        private T GetValue<T>(object instance, string field) {
            
            var fieldObj = instance.GetType().GetField(field, System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldObj == null) return default(T);
            return (T)fieldObj.GetValue(instance);

        }

    }

}