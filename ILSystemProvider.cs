using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public interface ILSystemProvider
    {
        ILSystem Compile();
    }

    public interface ILSystemProvider<ModuleType>: ILSystemProvider
        where ModuleType : IModule
    {
        new ILSystem<ModuleType> Compile();
    }
}
