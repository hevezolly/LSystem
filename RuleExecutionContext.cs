using System;
using System.Collections.Generic;
using System.Text;

namespace LSystem
{
    public class RuleExecutionContext
    {
        public readonly IModule Source;
        public readonly IModule LeftContext;
        public readonly IModule RightContext;

        public List<IModule> Successors { get; private set; } = new List<IModule>();

        public readonly float[] RandomValues;

        public readonly System.Random Random;

        public RuleExecutionContext(IModule source, IModule leftContext = null, IModule rightContext = null): 
            this(source, new Random().Next(0, int.MaxValue), leftContext, rightContext)
        { }

        public RuleExecutionContext(IModule source, int seed, IModule leftContext = null, IModule rightContext = null)
        {
            Source = source;
            LeftContext = leftContext;
            RightContext = rightContext;
            Random = new System.Random(seed);
            RandomValues = new float[10];
            for (var i = 0; i < 10; i++)
            {
                RandomValues[i] = (float)Random.NextDouble();
            }
        }

        public void SetSuccessors(IEnumerable<IModule> successors)
        {
            Successors = new List<IModule>(successors);
        }
    }
}
