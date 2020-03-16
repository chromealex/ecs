using ME.ECS;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ME.Example.Tests {

    using ME.Example.Game;
    using ME.Example.Game.Systems;
    using ME.Example.Game.Modules;
    using ME.Example.Game.Components;
    using ME.Example.Game.Entities;
    
    public class CollectionTests {

        [Test]
        public void RefList() {

            var list = new ME.ECS.Collections.RefList<Entity>();
            var idx0 = list.Add(new Entity());
            var idx1 = list.Add(new Entity());
            var idx2 = list.Add(new Entity());

            Assert.True(list.SizeCount == 3);
            Assert.True(list.UsedCount == 3);

            list.RemoveAt(idx1);
            
            Assert.True(list.UsedCount == 2);
            Assert.True(list.SizeCount == 3);

        }

    }

}