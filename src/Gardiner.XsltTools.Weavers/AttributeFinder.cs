using Mono.Cecil;

namespace Gardiner.XsltTools.Weavers
{
    public class AttributeFinder
    {
        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            if (customAttributes.ContainsAttribute("LogExceptionsAttribute"))
            {
                LogException = true;
            }
        }

        public bool LogException;
    }
}