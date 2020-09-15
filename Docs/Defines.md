# Defines [![](Logo-Tiny.png)](/../../#glossary)

| Define | Description |
| ------ | ------ |
| GAMEOBJECT_VIEWS_MODULE_SUPPORT | Turns on GameObject View Provider support. |
| PARTICLES_VIEWS_MODULE_SUPPORT | Turns on Particles View Provider support. |
| DRAWMESH_VIEWS_MODULE_SUPPORT | Turns on Graphics View Provider support. |
| UNITY_MATHEMATICS | Turns on Unity Mathematics Package use to generate random numbers. If it is turned off, UnityEngine.Random would be used instead. |
| WORLD_STATE_CHECK | If turned on, ME.ECS will check that all write data methods are in right state. If you turn off this check, you'll be able to write data in any state, but it could cause out of sync state. |
| WORLD_THREAD_CHECK | If turned on, ME.ECS will check random number usage from non-world thread. If you don't want to synchronize the game, you could turn this check off. |
| FPS_MODULE_SUPPORT | Turns on FPS Module. See [Default Modules](DefaultModules.md) section for more info. |
| ECS_COMPILE_IL2CPP_OPTIONS | If turned on, ME.ECS will use IL2CPP options for the faster runtime, this flag removed unnecessary null-checks and bounds array checks. |
| ECS_COMPILE_IL2CPP_OPTIONS_FILE_INCLUDE | Turn off this option if you provide your own Il2CppSetOptionAttribute. Works with ECS_COMPILE_IL2CPP_OPTIONS. |
| MESSAGE_PACK_SUPPORT | Turn on this option to enable MessagePack support (https://github.com/neuecc/MessagePack-CSharp). |

[![](Footer.png)](/../../#glossary)
