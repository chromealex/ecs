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
            world.AddSystem<PointsSystem>();
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            world.AddComponent<Point, PointIncreaseUnits>(entity);
            // Save reset state
            world.SaveResetState();

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
            world.AddSystem<PointsSystem>();
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            world.AddComponent<Point, PointIncreaseUnits>(entity);
            // Save reset state
            world.SaveResetState();

            var state = world.GetState();
            Assert.IsTrue(state.points.Count == 1);
            Assert.IsTrue(state.pointComponents.Count == 1);
            
            world.RemoveEntity<Point>(entity);
            Assert.IsTrue(state.points.Count == 0);
            Assert.IsTrue(state.pointComponents.Count == 0);
            
            TestsHelper.ReleaseWorld(ref world);
            
        }

        private IWorld<State> worldTemp;
        [Test(Description = "StatesHistory::StoreState() consistency check")]
        public void StatesHistory() {
            
            // Initialize
            var world = TestsHelper.CreateWorld(1f);
            this.worldTemp = world;
            world.AddModule<StatesHistoryModule>();
            world.AddModule<NetworkModule>();
            var history = world.GetModule<StatesHistoryModule>();

            var network = world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            var testCallId = network.RegisterRPC(new System.Action(this.TestCall_RPC).Method);
            network.RegisterObject(this, 1);
            
            // Adding default items
            world.AddSystem<PointsSystem>();
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            world.AddComponent<Point, PointIncreaseUnitsOnce>(entity);
            // Save reset state
            world.SaveResetState();

            { // Test body
                
                // Do regular update
                world.Update(100f);

                Point data;
                if (world.GetEntityData(entity.id, out data) == true) {

                    Assert.IsTrue(data.unitsCount == 2f);

                } else {
                    
                    Assert.Fail();
                    
                }
                
                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() {
                    tick = 0L, order = 0, localOrder = 1,
                    objId = 1,
                    groupId = 0,
                    rpcId = testCallId,
                });

                if (world.GetEntityData(entity.id, out data) == true) {

                    Assert.IsTrue(data.unitsCount == 3f, "UnitsCount: " + data.unitsCount.ToString() + ", Tick: " + world.GetTick());

                } else {
                    
                    Assert.Fail();
                    
                }

                // Do regular update
                world.Update(100f);

                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() {
                    tick = 100L, order = 0, localOrder = 1,
                    objId = 1,
                    groupId = 0,
                    rpcId = testCallId,
                });

                if (world.GetEntityData(entity.id, out data) == true) {

                    Assert.IsTrue(data.unitsCount == 4f, "UnitsCount: " + data.unitsCount.ToString() + ", Tick: " + world.GetTick());

                } else {
                    
                    Assert.Fail();
                    
                }

                // Do regular update
                world.Update(100f);

                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() {
                    tick = 200L, order = 0, localOrder = 1,
                    objId = 1,
                    groupId = 0,
                    rpcId = testCallId,
                });

                if (world.GetEntityData(entity.id, out data) == true) {

                    Assert.IsTrue(data.unitsCount == 5f, "UnitsCount: " + data.unitsCount.ToString() + ", Tick: " + world.GetTick());

                } else {
                    
                    Assert.Fail();
                    
                }
                
                Assert.IsTrue(world.GetTick() == 300f);

                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() {
                    tick = 500L, order = 0, localOrder = 1,
                    objId = 1,
                    groupId = 0,
                    rpcId = testCallId,
                });

                if (world.GetEntityData(entity.id, out data) == true) {

                    Assert.IsTrue(data.unitsCount == 5f, "UnitsCount: " + data.unitsCount.ToString() + ", Tick: " + world.GetTick());

                } else {
                    
                    Assert.Fail();
                    
                }

                // Do regular update
                world.Update(300f);

                if (world.GetEntityData(entity.id, out data) == true) {

                    Assert.IsTrue(data.unitsCount == 6f, "UnitsCount: " + data.unitsCount.ToString() + ", Tick: " + world.GetTick());

                } else {
                    
                    Assert.Fail();
                    
                }

            }

            TestsHelper.ReleaseWorld(ref world);

        }

        public void TestCall_RPC() {

            this.worldTemp.AddComponent<Point, PointIncreaseUnitsOnce>(Entity.Create<Point>(1));

        }

        [Test]
        public void AddEvent() {

            // Initialize
            var world = TestsHelper.CreateWorld();
            world.AddModule<StatesHistoryModule>();

            // Adding default items
            world.AddSystem<PointsSystem>();
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            var entityToRemove = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            world.AddComponent<Point, PointIncreaseUnits>(entity);
            world.AddComponent<Point, PointIncreaseUnits>(entityToRemove);
            // Save reset state
            world.SaveResetState();

            // Rewind to 100th tick
            ((IWorldBase)world).Simulate(100);

            // Add events
            var history = world.GetModule<StatesHistoryModule>();
            history.BeginAddEvents();
            {
                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 0L, order = 0, localOrder = 1 });
                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 0L, order = 0, localOrder = 2 });

                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 10L, order = 0, localOrder = 3 });
                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 10L, order = 0, localOrder = 4 });

                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 20L, order = 0, localOrder = 5 });
                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 20L, order = 0, localOrder = 6 });
                history.AddEvent(new ME.ECS.StatesHistory.HistoryEvent() { tick = 20L, order = 0, localOrder = 7 });
            }
            history.EndAddEvents();

            // Do regular update
            world.Update(0.01f);
            
            TestsHelper.ReleaseWorld(ref world);

        }

        [Test]
        public void CopyState() {
            
            // Initialize
            var world = TestsHelper.CreateWorld();
            
            // Adding items
            world.AddSystem<PointsSystem>();
            var entity = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 99f, increaseRate = 1f });
            var entityToRemove = world.AddEntity(new Point() { position = Vector3.one, unitsCount = 1f, increaseRate = 1f });
            world.AddComponent<Point, PointIncreaseUnits>(entity);
            world.AddComponent<Point, PointIncreaseUnits>(entityToRemove);
            
            // Save state
            var state = world.GetState();
            Assert.IsTrue(state.points.Count == 2);
            Assert.IsTrue(state.units.Count == 0);
            Assert.IsTrue(state.pointComponents.Count == 2);
            
            world.RemoveEntity<Point>(entityToRemove);
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
            ((IWorldBase)world).Simulate(savedState.tick);

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