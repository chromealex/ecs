# Instantiating Views [![](Logo-Tiny.png)](/../../#glossary)
To instantiate view need to call thread-safe method **world.InstantiateView(viewId, entity)**. Short method is **entity.InstantiateView(viewId)**.
> You can attach any count of views on each entity.
>
> But here are some **limitations**: for now you couldn't attach one source twice, only different sources for one entity allowed.
```csharp
this.world.InstantiateView(this.viewSourceId, entity);
```

[![](Footer.png)](/../../#glossary)
