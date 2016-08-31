using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Gardiner.XsltTools.Weavers
{
    public static class CecilExtensions
    {
        public static string DisplayName(this TypeReference typeReference)
        {
            var genericInstanceType = typeReference as GenericInstanceType;
            if (genericInstanceType != null && genericInstanceType.HasGenericArguments)
            {
                return typeReference.Name.Split('`').First() + "<" + string.Join(", ", genericInstanceType.GenericArguments.Select(c => c.DisplayName())) + ">";
            }
            return typeReference.Name;
        }

        public static void InsertBefore(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                processor.InsertBefore(target, instruction);
            }
        }


        public static bool IsBasicLogCall(this Instruction instruction)
        {
            var previous = instruction.Previous;
            if (previous.OpCode != OpCodes.Newarr || ((TypeReference)previous.Operand).FullName != "System.Object")
            {
                return false;
            }

            previous = previous.Previous;
            if (previous.OpCode != OpCodes.Ldc_I4)
            {
                return false;
            }

            previous = previous.Previous;
            if (previous.OpCode != OpCodes.Ldstr)
            {
                return false;
            }

            return true;
        }
        
        public static bool ContainsAttribute(this Collection<CustomAttribute> attributes, string attributeName)
        {
            var containsAttribute = attributes.FirstOrDefault(x => x.AttributeType.FullName == attributeName);
            if (containsAttribute != null)
            {
                attributes.Remove(containsAttribute);
            }
            return containsAttribute != null;
        }

        public static MethodDefinition FindMethod(this TypeDefinition typeDefinition, string method, params string[] paramTypes)
        {
            var firstOrDefault = typeDefinition.Methods
                .FirstOrDefault(x =>
                    !x.HasGenericParameters &&
                    x.Name == method &&
                    x.IsMatch(paramTypes));
            if (firstOrDefault == null)
            {
                var parameterNames = string.Join(", ", paramTypes);
                throw new WeavingException(string.Format("Expected to find method '{0}({1})' on type '{2}'.", method, parameterNames, typeDefinition.FullName));
            }
            return firstOrDefault;
        }

        public static bool IsMatch(this MethodReference methodReference, params string[] paramTypes)
        {
            if (methodReference.Parameters.Count != paramTypes.Length)
            {
                return false;
            }
            for (var index = 0; index < methodReference.Parameters.Count; index++)
            {
                var parameterDefinition = methodReference.Parameters[index];
                var paramType = paramTypes[index];
                if (parameterDefinition.ParameterType.Name != paramType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}