using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace MemberName.MemberNameAnnotations.QuickFixes
{
	[StaticSeverityHighlighting(
		Severity.ERROR, 
		"CSharpErrors", 
		OverlapResolve = OverlapResolveKind.ERROR, 
		ToolTipFormatString = "Parameter type with caller info must be implicitly convertible to '{0}'")]
	public class WrongParameterTypeError : CSharpHighlightingBase, IHighlightingWithRange
	{
		private readonly IRegularParameterDeclaration myParameterDeclaration;
		private readonly IType myType;
		private readonly string myMessage;

		public IRegularParameterDeclaration ParameterDeclaration
		{
			get { return myParameterDeclaration; }
		}

		public IType Type
		{
			get { return myType; }
		}

		public string ToolTip
		{
			get { return myMessage; }
		}

		public string ErrorStripeToolTip
		{
			get { return ToolTip; }
		}

		public int NavigationOffsetPatch
		{
			get { return 0; }
		}

		public WrongParameterTypeError(IRegularParameterDeclaration parameterDeclaration, IType type)
		{
			myParameterDeclaration = parameterDeclaration;
			myType = type;
			myMessage = string.Format(
				"Member name parameter type must be implicitly convertible to '{0}'", 
				Type.GetPresentableName(CSharpLanguage.Instance));
		}

		DocumentRange IHighlightingWithRange.CalculateRange()
		{
			return ParameterDeclaration.TypeUsage.GetHighlightingRange();
		}

		public override bool IsValid()
		{
			if (myParameterDeclaration == null || myParameterDeclaration.IsValid())
				return myType == null || myType.IsValid();
			return false;
		}
	}
}
