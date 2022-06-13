# Creating Entities [![](Logo-Tiny.png)](/../../#glossary)

Entity – is an abstract idea of something that exists in a world. In context of ECS – an entity is something to represent a data container with no data attached. Basically, entity is an ID, and you can specify components (data) to be associated with that ID.

In ME.ECS – Entities are structs, that have a name (relevant in editor only), a generation and a version. They do not have anything by themselves, and it is your job to do something with them. Entities could be created by in two ways

```csharp

  var entity = this.world.AddEntity(); //this would create an empty entity and add it to the world
  var entity = new Entity(someName); // this would alse create an empty entity and add it to the world, but with a given name

```
> *NOTE: If you are using “new Entity()” instead of “this.world.AddEntity()” you **must** provide a name and/or a flag.*

Entities can have certain flags associated with them, controlling their lifetime. By default – if you do not provide a flag yourself, or choose None flag – entity's lifetime would be infinite, as in – you would have to manually remove entity from the world (entity would be returned to the pool upun Desstoy() call).

You can have a OneShot entity, that would be (as a OneShotComponent) removed and returned to the pool at the end of a logic tick it was created in.

You can have a DestroyWithoutComponents entity, that would be automatically removed and returned to the pool if it has no components associated with it.
```csharp

    var entity1 = this.world.AddEntity(EntityFlag.None); // adds an entity with no name and a default flag
    var entity2 = this.world.AddEntity("someEntity", EntityFlag.OneShot); // adds an entity with a name and a OneShot flag
    var entity3 = new Entity("someEntity"); // adds an entity with a name and a default flag
    var entity4 = new Entity(EntityFlag.DestroyWithoutComponents); // adds an entity with no name and a flag

```

You can remove an entity at any given time during AdvanceTick() or in any RPC, by calling entity.Destroy().

> *NOTE: this should be done at the end of the logic call, or the loop should be continued or broken, to exclude any errors*
```csharp

void ISystemFilter.AdvanceTick(in Entity entity, in float deltaTime) {

            if (someCondition) {
            
                entity.Destroy();
            }
        }
 
```
# What are the generation and version for?
Entities in ME.ECS are pooled for better performance, and every entity is generation 0 while it is dormant, but as soon as you create it from the pool – it’s generation goes up, indicating, that this entity has already been created, processed and recycled.
Versions are almost the same, but they indicate entity’s changes throughout it’s lifetime (within one generation). Entity’s version is changed every time a component is added, removed, or component’s data is changed.

You can access different entities properties
```csharp

	var id = entity.id; //returns you entity’s id
	var generation = entity.generation; // returns entity’s generation
	var version = entity.GetVersion(); // returns current version of the entity
  
```
You can also check if the entity is alive or if the entity was created or referenced at all
```csharp

	var alive = entity.IsAlive(); // returns whether the entity is in the world or was returned to the pool
	var exists = entity.IsEmpty(); // returns whether the entity was created at all
	var existsWithinBounds = entity.IsAliveWithBoundsCheck(); // returns whether the entity is alive with some checks.
  
```
> *NOTE: IsAliveWithBoundsCheck() should be called from UI scripts as they might reference a sate that has been changed.*

Also, there are several methods to help you debug the entity

```csharp

	var noVersion = entity.ToStringNoVersion() ; // returns a formatted string with Id and  Generation
	var full = entity.ToString(); // returns a formatted string, with an Id, generation and version
	var small = entity.ToSmallString(); // same as above, but formatted shorter

```
[![](Footer.png)](/../../#glossary)
