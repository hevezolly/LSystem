using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public interface ILSystem
    {
        void Init();
        IEnumerable<IModule> CurrentState { get; }
        void Step();
    }

    public interface ILSystem<ModuleType>: ILSystem
        where ModuleType: IModule
    {
        new IEnumerable<ModuleType> CurrentState { get; }
    }
}
