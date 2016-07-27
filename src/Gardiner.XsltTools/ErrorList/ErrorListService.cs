using System.Linq;

namespace Gardiner.XsltTools.ErrorList
{
    internal class ErrorListService
    {
        public static void ProcessLintingResults(AccessibilityResult result)
        {
            TableDataSource.Instance.CleanErrors(result.Url);

/*            if (!VSPackage.Options.ShowWarnings)
            {
               result.Violations = result.Violations.Where(r => r.GetSeverity() != __VSERRORCATEGORY.EC_WARNING);
            }

            if (!VSPackage.Options.ShowMessages)
            {
                result.Violations = result.Violations.Where(r => r.GetSeverity() != __VSERRORCATEGORY.EC_MESSAGE);
            }*/

            if (result.Violations.Any())
            {
                TableDataSource.Instance.AddErrors(result);
            }
        }
    }
}