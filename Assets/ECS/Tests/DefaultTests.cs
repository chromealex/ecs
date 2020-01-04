using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ME.ECS.Tests {

    public class DefaultTests {

        [Test]
        public void AddEntity() {
            
            // Initialize
            var world = TestsHelper.CreateWorld();
            
            // Adding items
            world.AddSystem(new PointsSystem());
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            world.AddComponent<Point, IncreaseUnits>(entity);

            var state = world.GetState();
            Assert.IsTrue(state.points.Count == 1);
            Assert.IsTrue(state.pointComponents.Count == 1);
            
            TestsHelper.ReleaseWorld(ref world);
            
        }

        [Test]
        public void RemoveEntity() {
            
            // Initialize
            var world = TestsHelper.CreateWorld();
            
            // Adding items
            world.AddSystem(new PointsSystem());
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            world.AddComponent<Point, IncreaseUnits>(entity);

            var state = world.GetState();
            Assert.IsTrue(state.points.Count == 1);
            Assert.IsTrue(state.pointComponents.Count == 1);
            
            world.RemoveEntity(entity);
            Assert.IsTrue(state.points.Count == 0);
            Assert.IsTrue(state.pointComponents.Count == 0);
            
            TestsHelper.ReleaseWorld(ref world);
            
        }

        [Test]
        public void CopyState() {
            
            // Initialize
            var world = TestsHelper.CreateWorld();
            
            // Adding items
            world.AddSystem(new PointsSystem());
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            var entityToRemove = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            world.AddComponent<Point, IncreaseUnits>(entity);
            world.AddComponent<Point, IncreaseUnits>(entityToRemove);
            
            // Save state
            var state = world.GetState();
            Assert.IsTrue(state.points.Count == 2);
            Assert.IsTrue(state.units.Count == 0);
            Assert.IsTrue(state.pointComponents.Count == 2);
            
            world.RemoveEntity(entityToRemove);
            Assert.IsTrue(state.points.Count == 1);
            Assert.IsTrue(state.pointComponents.Count == 1);
            
            var savedState = (IState<State>)world.CreateState();
            savedState.Initialize(world, freeze: true, restore: false);
            
            Assert.IsTrue(state.points.Count == 1);
            Assert.IsTrue(state.pointComponents.Count == 1);
            {
                var dic = TestsHelper.GetValue<Dictionary<int, IComponents>>(world, "componentsCache");
                Assert.IsTrue(dic.Count == 2);
            }
            
            savedState.CopyFrom(state);
            
            world.Update(1f);
            world.ReleaseState(ref state);
            
            // Restore state
            world.SetState((State)savedState);

            Assert.IsTrue(((State)savedState).points.Count == 1);
            Assert.IsTrue(((State)savedState).pointComponents.Count == 1);
            
            {
                var dic = TestsHelper.GetValue<Dictionary<int, IList>>(world, "entitiesCache");
                Assert.IsTrue(dic.Count == 1);
            }

            {
                var dic = TestsHelper.GetValue<Dictionary<int, IList>>(world, "filtersCache");
                Assert.IsTrue(dic.Count == 2);
            }

            {
                var dic = TestsHelper.GetValue<Dictionary<int, IComponents>>(world, "componentsCache");
                Assert.IsTrue(dic.Count == 2);
            }

            {
                Point data;
                world.GetEntityData(1, out data);
                Assert.IsTrue(data.unitsCount == 99f);
            }

            TestsHelper.ReleaseWorld(ref world);

        }

    }

}