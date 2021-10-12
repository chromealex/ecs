# Creating Entities [![](Logo-Tiny.png)](/../../#glossary)
For creating entities you need to run world.AddEntity().
Names are used for Editor debug information only, so they are optional.
> If you use new Entity() instead of this.world.AddEntity() you **must** to provide some name.
```csharp
var entity = this.world.AddEntity([name]); // The same as var entity = new Entity(name);
...
```

[![](Footer.png)](/../../#glossary)
