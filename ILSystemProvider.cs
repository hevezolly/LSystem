using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    interface ILSystemProvider
    {
        ILSystem Compile();
    }

    interface ILSystemProvider<ModuleType>: ILSystemProvider
        where ModuleType : IModule
    {
        new ILSystem<ModuleType> Compile();
    }
}
