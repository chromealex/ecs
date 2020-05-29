# Code Generators [![](Logo-Tiny.png)](/../../#glossary)
> Any of code generators works when Project Directory selected.
>
> To run all code generators at once, select your Project Directory and choose **ME.ECS/Generators/Compile All...**

#### AOT
You need some magic to work with IL2CPP, so before build your project you need to use **ME.ECS/Generators/AOT/Compile All...** top menu command to run code generator.

#### Struct Components
To be able to use struct components in jobs, you need some sort of magic to register all your struct components while project initialization.

#### Stack Array
If you want to support different sizes of StackArray, you can run StackArray compiler to re-generate them.
