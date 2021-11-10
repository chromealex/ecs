# Instantiating Views [![](Logo-Tiny.png)](/../../#glossary)
To instantiate view need to call thread-safe method **world.InstantiateView(viewId, entity)**. Short method is **entity.InstantiateView(viewId)**.
> You can attach only one view on each entity at once. If you try to Instantiate second view on entity which already has view component, you'll got an exception.
> If you want to change view on entity - use **entity.DestroyAllViews();** before calling InstantiateView or call **entity.ReplaceView(viewId);**.
```csharp
entity.InstantiateView(this.viewSourceId);
```
or
```csharp
this.world.InstantiateView(this.viewSourceId, entity);
```

[![](Footer.png)](/../../#glossary)
