using System;
using System.Reflection;

namespace Neko.SimTest
{
    internal static class Program
    {
        private static int Main(string[] args) =>
            new NUnitLite.AutoRun(Assembly.GetExecutingAssembly()).Execute(args);
    }
}
