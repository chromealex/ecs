# Data Configs [![](Logo-Tiny.png)](/../../#glossary)
ME.ECS has Data Config files to store your components instead of custom scriptable objects for storing data.
To add Data Config object to your project, choose **Create/ME.ECS/Data Config** menu and create your config file.
In config file you can add struct components, reference components and "remove" struct components.

All configs should be applied on exist entity by the following code:
```csharp
config.Apply(in entity);
```

After this code works, on your entity you'll get all components you'd choose in your config file.

[![](Footer.png)](/../../#glossary)
