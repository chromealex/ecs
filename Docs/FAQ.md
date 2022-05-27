### Is marker a one-frame?
<answer>Yes, marker's lifetime is roughly one frame. It is created immediately (during the frame) and removed at the end of the same frame.</answer>

### What is a “state” ?
<answer>State is a data storage. All things you want to sync with a clients must be in inside components.</answer>
 
### Can I change the game state (e.g. call entity.Get/entity.Set) from the view?
<answer>No! You can change the state only from AdvanceTick method or from RPC call.</answer>
 
### Can I use DOTween or any other coroutine-like plugin in AdvanceTick?
<answer>No! You should remember that any coroutine-like plugin (e.g. DOTween) run in Unity update (e.g. Update/LateUpdate/FixedUpdate), and you cannot modify game state from Unity update methods.</answer>
 
### I've got compilation error like Copyright (C) Microsoft Corporation. All rights reserved.
<answer>This behaviour can be caused by different things, you need to read the error text to determine what goes wrong. But in context of ME.ECS - you should check if this file “csc.rsp.gen” exists and it is located at the root of your “Assets” folder.</answer>
 
### I've got compilation error when removing or renaming component
<answer>If you remove or rename component manually (not using IDE) you might get a compilation error. In that case you should find and delete file “compiler.gen.structcomponents” located at “Assets\Project\Generator\gen”. Don’t worry, ME.ECS has code-generation tool and will regenerate this file. If not, you should check ME.ECS/Generators/Struct Components/Auto Compile checkmark, but otherwise – ME.ECS should handle that error on it’s own as you let Unity refresh assets and recompile.</answer>

### I've renamed a component via IDE, but get an Unknown managed type referenced ... ?
<answer>You can get this error if you use DataConfigs, this happens due to Unity's serialization methods. If you get this error - you can either remove that component from all configs that use it, or you can open a config SO in your IDE and manually rename invalid component to correct naming. Note that if you changed not only name of component, but also its fields or data type - you should correct them in scriptable object too.</answer>

### Can I store logic between-frames temp data in systems, views, etc?
<answer>No! You should not store any temporary between-frames data in systems and views. In general, if you need to store any data, that goes out of the scope of a single call of AdvanceTick(), you should create components and use them to hold that data. More to this - views are stateless, and you should not store any data in them, or you would need to update it when the “immediately” flag is true while “View::ApplyState” is called.</answer>
 
### Can I use Unity Physics, NavMesh or any other Update-dependent tool?
<answer>There is no support to Unity’s implementation of different systems out-of-the box. Those systems are stateless as they do not store it’s previous “state” to compare to and calculate from. Generally, you would need to rebuld those systems yourself. You would need, for example, to adapt physics system to manage scene colliders by itself in AdvanceTick. Or, if you want to use NavMesh, you need to rebuild NavMesh every tick (call graph rebuild in AdvanceTick). Sure, you can cache some results, listen to some changes, etc, but in general those systems would not work on their own.</answer>
 
### Can I store ReferenceValue objects in components?
<answer>It depends on the read/write access you want to have to the data. If you just read this object and never change it - you can place it in component, but if you want to modify this object - you should use Copyable Component (MyComponent : IStructCopyable<MyComponent>). Take a look into DataObject struct type, that allows you to modify data and store it in default IComponent. But you should implement CopyFrom() and Recycle() methods for your object in any case.</answer>
 
### Can I somehow notify view from logic?
<answer>No, you cannot. You should add or remove a component to or from your entity in the logic tick, to notify it’s view that something changed. Then in the view – you just check if entity has desired component or not, and update the view accordingly. </answer>
<answer>But, in case you want to notify UI that something happened, you can use GlobalEvent that allows to receive events AFTER the logic tick has been completed.</answer>
 
### Will entity be removed automatically if it has no components at the end of the tick?
<answer>No, entity will be alive forever until you call entity.Destroy().</answer>
 
### How can I remove entity by the time or at the end of the tick?
<answer>You can use Essentials Destroy feature addon to control entities lifetime.</answer>
 
### Is there any one-tick component?
<answer>Yes, you can use entity.Set(new Component(), ComponentLifetime.NotifyAllSystems) API to be able to notify all systems start from the next tick. For cases where you need to notify all systems below - you can use OneShotComponent type. It will be better choice because OneShot components have not stored in state and theirs lifetime over at the end of the tick.</answer>
 
### Can I call entity.InstantiateView for current player in AdvanceTick?
<answer>No! You can't do anything in AdvanceTick which will affect your local player only. InstantiateView will create a ViewComponent and place it onto your entity, so it will affect your game state.</answer>
 
### Can I send entity through RPC?
<answer>No! You can't. This is because entity could changed with rollback and RPC events will apply onto wrong entity.</answer>
 
### Can I store entity in components?
<answer>Yes, sure you can. You can store entity in IComponent or in copyable components.</answer>
 
### I've got a hash errors while playing in multiplayer mode
<answer>Hash errors may occur when you do not follow instructions in general. But in some cases you can get this error because of different processor archetectures on your clients.</answer>
<answer>If you test your multiplayer on the same computer - just check your code, the problem is in there 100%. Check all IStructCopyable components, be sure you Copy and Recycle these components properly.</answer>
<answer>If you test your multiplayer on different computers (for example mac and pc) - call world.GetIEEEFloat and world.GetIEEEFloatFixed. If you've got GetIEEEFloat result as different and GetIEEEFloatFixed the same - you need to use fpmath.</answer>
 
### I want to play sound (for example), how and where can I do this?
<answer>In ME.ECS you have events, which accumulated in AdvanceTick and fires only on VisualTick (regular Update). So you don't need to play sound inside your logic, you should play it after all rollbacks have done.</answer>
 
### I want to implement interpolation, how can I do this?
<answer>ME.ECS separates Logic and Presentation(View) layers. Logic can read and write state, while View can only read state. So when rollback fires, it will affect state data only (not views). That’s why you can run your game without any visual representation at all. You will receive two states in the certain view: before rollback and after rollback, so you can interpolate it on the view layer.</answer>
 
### How does server validate client events?
<answer>In ME.ECS events could be run in different ways: SendToServer and/or RunLocal. So you can choose the way you want. RunLocal runs your event locally immediately and SendToServer - send it to the other clients (or may be to your client too if you don't choose RunLocal option). If you want to validate your actions on the server - you can use SendToServer mode only. Btw ME.ECS has Cancel Event functionality, which allows you to cancel some event on the certain client (for example if client ping is too high for some reason and server receive the event too late).</answer>
