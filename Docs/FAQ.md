### Is marker a one-frame?
<answer>Yes, marker's lifetime is a one-frame. It's created immediately and removed at the end of the frame.</answer>

### Can I change the game state (e.g. call entity.Get/entity.Set) from the view?
<answer>No! You can change the state only from AdvanceTick method or from RPC call.</answer>

### Can I use DOTween or any other coroutine-like plugin in AdvanceTick?
<answer>No! You should understand that any coroutine-like plugin (e.g. DOTween) run in Unity update (e.g. Update/LateUpdate/FixedUpdate), so in Unity update methods you can't modify the game state.</answer>

### I've got compilation error like Copyright (C) Microsoft Corporation. All rights reserved.
<answer>This behaviour can be caused by different things, you need to read the error text to determine what goes wrong. But in ME.ECS context you should check if this file is exist: Assets/csc.rsp.gen.</answer>

### I've got compilation error when removing or renaming component
<answer>If you remove or rename component manually (not using IDE) you've got compilation error because ME.ECS has code-generation tool. Just find and remove file compiler.gen.structcomponents.cs, it will be created automatically. If not, you should check ME.ECS/Generators/Struct Components/Auto Compile checkmark.</answer>

### Can I store logic temp between-frames data in systems, views, etc?
<answer>No! You should not store any temp between-frames data in systems and views. If you need to store temp data - just create the component and store it there. Views are stateless, so you should not store temp data there or you need to update it when the immediately flag is true while View::ApplyState call.</answer>

### Can I use Unity Physics, NavMesh or any other Update-dependent tool?
<answer>In general, no, mean out-of-the-box. You can implement custom physics system, which should manage scene colliders by itself in AdvanceTick. For example, if you want to use NavMesh, all you need to do - it's to rebuild NavMesh every tick (call graph rebuild in AdvanceTick). Sure, here you can cache some results, listening some changes, etc, but in general it works like this.</answer>

### Can I store ReferenceValue objects in components?
<answer>In general, no. If you just read this object and never change it - you can place it in component, but if you want to modify this object you should use Copyable Component. Take a look into DataObject struct type, that allows you to modify data and store it in default IComponent. But in any case you should implement CopyFrom and Recycle methods for your object.</answer>

### Can I somehow notify view from logic?
<answer>No, you not able to notify views directly from your logic! You should add some component in logic tick onto your entity, when your effect over - just remove it from entity. In view just check does entity has component or not and update your view.</answer>
<answer>But if you want to notify UI, you can use GlobalEvent that allows receive events after logic tick has been complete.</answer>

### Will entity be removed automatically if it has no components at the end of the tick?
<answer>No, entity will be alive forever until you call entity.Destroy().</answer>

### How can I remove entity by the time or at the end of the tick?
<answer>You can use Essentials Destroy feature addon to control entities lifetime.</answer>

### Is there any one-tick component?
<answer>Yes, you can use entity.Set(new Component(), ComponentLifetime.NotifyAllSystems) API to be able to notify all systems start from the next tick. For cases where you need to notify all systems below - you can use OneShotComponent type. It will be better choice because OneShot components have not stored in state and theirs lifetime over at the end of the tick.</answer>

### Can I call entity.InstantiateView for current player in AdvanceTick?
<answer>No! You can't do anything in AdvanceTick which will affect your active player only. InstantiateView will create a ViewComponent and place it onto your entity, so it will affect your game state.</answer>

### Can I send entity through RPC?
<answer>No! You can't. This is because entity could changed with rollback and RPC events will apply onto wrong entity.</answer>

### Can I store entity in components?
<answer>Yes, sure you can. You can store entity in IComponent or in copyable components.</answer>

### I've got a hash errors while playing in multiplayer mode
<answer>Hash errors may occur when you doesn't lead instructions in general case. But in some cases you can got this error because of different processor archetectures on your clients.</answer>
<answer>If you test your multiplayer on the same computer - just check your code, the problem is there 100%. Check all IStructCopyable components, be sure you Copy and Recycle these components properly.</answer>
<answer>If you test your multiplayer on different computers (for example mac and pc) - call world.GetIEEEFloat and world.GetIEEEFloatFixed. If you've got GetIEEEFloat result as different and GetIEEEFloatFixed the same - you need to use fpmath.</answer>
