using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;

namespace MemberName
{
  [ActionHandler("MemberName.About")]
  public class AboutAction : IActionHandler
  {
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      // return true or false to enable/disable this action
      return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      MessageBox.Show(
        "MemberName\nAndreas Vilinski\n\nProvides an additional annotation attribute - [MemberName]",
        "About MemberName",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    }
  }
}
