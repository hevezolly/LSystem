using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LSystem
{
    public class Programm
    {
        public static void Main(string[] args)
        {
            var lSys = new XmlLSystemProvider().Load("D:\\UnityProjects\\TreeGenerator.v3\\Assets\\LSystems\\test\\testLSystem.xml").Compile();
            lSys.Init();
            Display(lSys.CurrentState);
            for (var i = 0; i < 5; i++)
            {
                lSys.Step();
                Display(lSys.CurrentState);
            }
        }

        private static void Display(IEnumerable<IModule> state)
        {
            foreach (var m in state)
            {
                if (m.parameters.Count() == 0)
                    Console.Write($"{m.Id}; ");
                else
                {
                    Console.Write($"{m.Id}({string.Join(", ", m.parameters)}); ");
                }
            }
            Console.WriteLine();
        }
    }
}
