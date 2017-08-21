using System;

namespace Gluon
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var structureCreator = new ClassesStructureCreator();
            structureCreator.CreateStructure();
        }
    }
}
