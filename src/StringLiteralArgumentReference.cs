using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Resolve;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace MemberName
{
	internal abstract class StringLiteralArgumentReference : CheckedReferenceBase<ICSharpArgument>, ICompleteableReference
	{
		protected readonly ExactNameFilter ExactMemberNameFilter;
		protected readonly ICSharpLiteralExpression OwnerLiteral;

		protected StringLiteralArgumentReference(ICSharpArgument argument)
			: base(argument)
		{
			OwnerLiteral = (ICSharpLiteralExpression) myOwner.Value;
			ExactMemberNameFilter = new ExactNameFilter((string) OwnerLiteral.ConstantValue.Value);
		}

		public override bool IsValid()
		{
			return base.IsValid() && myOwner.ContainsReference(this);
		}

		public override TreeTextRange GetTreeTextRange()
		{
			return OwnerLiteral.GetStringLiteralContentTreeRange();
			//TreeTextRange contentTreeRange = myOwnerLiteral.GetStringLiteralContentTreeRange();
			//return contentTreeRange.Length != 0 ? contentTreeRange : myOwner.GetTreeTextRange();
		}

		public override IAccessContext GetAccessContext()
		{
			return new DefaultAccessContext(myOwner);
		}

		public override IReference BindTo(IDeclaredElement element, ISubstitution substitution)
		{
			return BindTo(element);
		}

		public override string GetName()
		{
			if (OwnerLiteral == null || !OwnerLiteral.ConstantValue.IsString())
				return "???";
			return (string) OwnerLiteral.ConstantValue.Value;
		}

		public virtual ISymbolTable GetCompletionSymbolTable()
		{
			return GetReferenceSymbolTable(false);
		}

		public override ResolveResultWithInfo ResolveWithoutCache()
		{
			var resolveResultWithInfo = CheckedReferenceImplUtil
				.Resolve(this, GetReferenceSymbolTable(true)
					               .Filter(new ISymbolFilter[] {ExactMemberNameFilter}));
			return resolveResultWithInfo.Result.IsEmpty
				       ? new ResolveResultWithInfo(EmptyResolveResult.Instance, CSharpResolveErrorType.NOT_RESOLVED_TEXT_REFERENCE)
				       : resolveResultWithInfo;
			//ResolveResultWithInfo resolveResult = GetReferenceSymbolTable(true).GetResolveResult(GetName());
			//if (!myIsAbstract || !(resolveResult.Info.ResolveErrorType == ResolveErrorType.MULTIPLE_CANDIDATES))
			//    return resolveResult;
			//return new ResolveResultWithInfo(resolveResult.Result, ResolveErrorType.OK);
		}

		public override ISymbolFilter[] GetSymbolFilters()
		{
			return EmptyArray<ISymbolFilter>.Instance;
		}
	}
}