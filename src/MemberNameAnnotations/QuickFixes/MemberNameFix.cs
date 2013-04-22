using System;
using System.Linq;

using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Intentions.CSharp.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.TextControl;
using JetBrains.Util;

namespace MemberName.MemberNameAnnotations.QuickFixes
{
	public class MemberNameFix : ChangeTextFixBase
	{
		private readonly ICompleteableReference myReference;

		public override string Text
		{
			get { return "Fix member name"; }
		}

		public MemberNameFix(NotResolvedInTextWarning error)
		{
			myReference = error.Reference as ICompleteableReference;
		}

		public override bool IsAvailable(IUserDataHolder cache)
		{
			return myReference
				.If(x => x.IsValid())
				.With(x => x.GetTreeNode() as ICSharpArgument)
				.If(x => x.Value is ICSharpLiteralExpression)
				.With(x => x.MatchingParameter)
				.With(x => x.Element)
				.With(x => myReference.GetReferenceSymbolTable(false).Names().Any());
		}

		protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
		{
			var csharpArgument = (ICSharpArgument) myReference.GetTreeNode();
			var properties = myReference
				.GetCompletionSymbolTable()
				.GetAllSymbolInfos()
				.Select(x => x.GetDeclaredElement())
				.OfType<IProperty>()
				.Select(x => x.ShortName)
				.ToList();
			if (properties.Count == 1)
			{
				var str = properties[0];
				var expression = CSharpElementFactory
					.GetInstance(csharpArgument.GetPsiModule())
					.CreateExpression("$0", new object[] { "\"" + str + "\"" });
				csharpArgument.Value.ReplaceBy(expression);
				return null;
			}
			if (properties.Count == 0)
				properties.Add((string) csharpArgument.Value.ConstantValue.Value);
			return textControl => ExecutePostReplaceSuggestion(textControl, solution, myReference, properties);
		}
	}

}
