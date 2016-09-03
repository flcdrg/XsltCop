using System.Linq;

namespace Gardiner.XsltTools.ErrorList
{
    internal static class ErrorListService
    {
        public static void ProcessLintingResults(AccessibilityResult result)
        {
            TableDataSource.Instance.CleanErrors(result.Url);

            if (result.Violations.Any())
            {
                TableDataSource.Instance.AddErrors(result);
            }
        }
    }
}