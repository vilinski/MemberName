using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace MemberName.MemberNameAnnotations.QuickFixes
{
	[ConfigurableSeverityHighlighting("MemberName.RedundantArgumentValue", "CSHARP", AttributeId = "ReSharper Dead Code",
		OverlapResolve = OverlapResolveKind.DEADCODE, ToolTipFormatString = "The parameter {0} has the same default value")]
	public class RedundantArgumentValueWarning : CSharpHighlightingBase, IHighlightingWithRange
	{
		public RedundantArgumentValueWarning(IParameter parameter, ICSharpArgument argument)
		{
			Parameter = parameter;
			Argument = argument;
			ToolTip = string.Format("Redundant value of parameter {0}", Parameter.ShortName);
		}

		public IParameter Parameter { get; private set; }

		public ICSharpArgument Argument { get; private set; }

		public string ToolTip { get; private set; }

		public string ErrorStripeToolTip
		{
			get { return ToolTip; }
		}

		public int NavigationOffsetPatch
		{
			get { return 0; }
		}

		DocumentRange IHighlightingWithRange.CalculateRange()
		{
			return Argument.GetHighlightingRange();
		}

		public override bool IsValid()
		{
			if (Parameter != null && !Parameter.IsValid())
				return false;
			return Argument == null || Argument.IsValid();
		}
	}
}