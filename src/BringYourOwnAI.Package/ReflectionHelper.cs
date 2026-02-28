using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Extensibility;

namespace BringYourOwnAI.Package
{
    public static class ReflectionHelper
    {
        public static void Dump(VisualStudioExtensibility ex)
        {
            var methods = ex.Documents().GetType().GetMethods();
            foreach(var m in methods)
            {
                Console.WriteLine(m.Name);
            }
        }
    }
}
