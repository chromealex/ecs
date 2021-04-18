# Data Configs [![](Logo-Tiny.png)](/../../#glossary)
ME.ECS has Data Config files to store your components instead of custom scriptable objects for storing data.
To add Data Config object to your project, choose **Create/ME.ECS/Data Config** menu and create your config file.
In config file you can add struct components, reference components and "remove" struct components.

All configs should be applied on exist entity by the following code:
```csharp
config.Apply(in entity, [overrideIfExist]);
```
After this code works, on your entity you'll get all components you'd choosed in your config file (except [IComponentStatic](https://github.com/chromealex/ecs/blob/master/Docs/Manual-CreatingComponents.md#static-components)).
For [shared components](https://github.com/chromealex/ecs/blob/master/Docs/Manual-CreatingComponents.md#shared-components) each DataConfig stores groupId (you could edit it at the top of data config in inspector).

You can check has config some component or not:
```csharp
if (config.Has<Component>() == true) ...
```

or get component data without appling config on entity:
```csharp
var data = config.Get<Component>();
```

### Data Config Templates

When you create feature, you have one or several components working with your logic. You can create **Data Config Template** file to store components like in regular data config, but templates are using just in editor not at runtime.
To apply config template you need to use "Manage Templates" button on your **Data Config**.
Note that data config templates useful just in editor mode.

[![](Footer.png)](/../../#glossary)
