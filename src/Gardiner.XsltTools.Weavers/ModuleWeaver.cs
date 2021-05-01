using System;
using System.Linq;

using Mono.Cecil;

namespace Gardiner.XsltTools.Weavers
{
    public class ModuleWeaver
    {
        public Action<string> LogDebug { get; set; }
        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }

        public TypeReference ExceptionType;

        public ModuleWeaver()
        {
            LogWarning = s => { };
            LogInfo = s => { };
            LogDebug = s => { };
        }

        public void Execute()
        {
            var loggerTypeDefinition = ModuleDefinition.Types.First(x => x.Name == "Telemetry");
            LogMethod = ModuleDefinition.ImportReference(loggerTypeDefinition.FindMethod("Log", "Exception"));

            var mscorelibReference = new AssemblyNameReference("mscorlib", new Version(4, 0, 0, 0));
            var msCoreLibDefinition = AssemblyResolver.Resolve(mscorelibReference);
            ExceptionType = ModuleDefinition.ImportReference(msCoreLibDefinition.MainModule.Types.First(x => x.Name == "Exception"));
            foreach (var type in ModuleDefinition
                .GetTypes()
                .Where(x => (x.BaseType != null) && !x.IsEnum && !x.IsInterface))
            {
                ProcessType(type);
            }
        }

        public MethodReference LogMethod { get; set; }

        private void ProcessType(TypeDefinition type)
        {
            foreach (var method in type.Methods)
            {
                //skip for abstract and delegates
                if (!method.HasBody)
                {
                    continue;
                }

                var onExceptionProcessor = new OnExceptionProcessor
                {
                    Method = method,
                    ModuleWeaver = this
                };
                onExceptionProcessor.Process();
            }
        }
    }
}
