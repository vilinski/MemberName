using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve.Filters;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace MemberName.MemberNameAnnotations
{
	sealed class MemberNameReference : StringLiteralArgumentReference
	{
		private static readonly ISymbolFilter OurInstanceFilter =
			new PredicateFilter(InstanceProperties, ResolveErrorType.STATIC_PROBLEM);

		private static readonly ISymbolFilter OurPropertyFilter = new DeclaredElementTypeFilter(
			ResolveErrorType.NOT_RESOLVED, new[] { CLRDeclaredElementType.PROPERTY });

		private readonly ITypeElement myTypeElement;

		static MemberNameReference()
		{
		}

		public MemberNameReference([NotNull]ICSharpArgument argument, [NotNull]ITypeElement typeElement)
			: base(argument)
		{
			myTypeElement = typeElement;
		}


		//#region other
		//private readonly ITypeElement myTypeElement;
		//private readonly ICSharpDeclaration myOwnerDeclaration;
		//private readonly ISymbolFilter myExactNameFilter;
		//private readonly bool myIsAbstract;
		//public MemberNameReference(ITypeElement typeElement, ILiteralExpression literal, ICSharpDeclaration ownerDeclaration)
		//    : base(literal)
		//{
		//    myTypeElement = typeElement;
		//    myOwnerDeclaration = ownerDeclaration;
		//    myIsAbstract = typeElement
		//        .With(x => x as IClass)
		//        .With(x => x.IsAbstract);
		//    myExactNameFilter = new ExactNameFilter((string)literal.ConstantValue.Value);
		//}
		//#endregion

		private static bool InstanceProperties([NotNull] ISymbolInfo symbol)
		{
			var property = symbol.GetDeclaredElement() as IProperty;
			return property != null && !property.IsStatic;
		}

		public override IReference BindTo(IDeclaredElement element)
		{
			CSharpElementFactory instance = CSharpElementFactory.GetInstance(myOwner.GetPsiModule());
			ICSharpExpression csharpExpression = myOwner.Value;
			var str = (string)null;
			if (csharpExpression is ILiteralExpression)
			{
				string text = csharpExpression.GetText();
				if (text.Length > 0 && text[0] == '@')
					str = "@\"" + StringLiteralConverter.EscapeToVerbatim(element.ShortName) + "\"";
			}
			string format = str ?? "\"" + StringLiteralConverter.EscapeToRegular(element.ShortName) + "\"";
			csharpExpression.ReplaceBy(instance.CreateExpression(format, new object[0]));
			return this;

			//IStringLiteralAlterer literalByExpression = StringLiteralAltererUtil.CreateStringLiteralByExpression(OwnerLiteral);
			//literalByExpression.Replace((string)OwnerLiteral.ConstantValue.Value, element.ShortName, myOwner.GetPsiModule());
			//ILiteralExpression expression = literalByExpression.Expression;
			//if (myOwner.Equals(expression))
			//    return this;
			//return expression.FindReference<MemberNameReference>() ?? this;
		}

		public override ISymbolTable GetReferenceSymbolTable(bool useReferenceName)
		{
			return ResolveUtil.GetSymbolTableByTypeElement(myTypeElement, SymbolTableMode.FULL, myOwner.GetPsiModule()).Distinct();
			//ICSharpTypeDeclaration containingTypeDeclaration = myOwner.GetContainingTypeDeclaration();
			//if (containingTypeDeclaration != null)
			//{
			//    ITypeElement declaredElement = containingTypeDeclaration.DeclaredElement;
			//    if (declaredElement != null)
			//        return ResolveUtil.GetSymbolTableByTypeElement(myTypeElement, SymbolTableMode.FULL, myOwner.GetPsiModule()).Distinct();
			//}
			//return EmptySymbolTable.INSTANCE;
			//ISymbolTable symbolTable = ResolveUtil.GetSymbolTableByTypeElement(myTypeElement, SymbolTableMode.FULL, myOwner.GetPsiModule());
			////ISolution solution = myTypeElement.GetSolution();
			////solution.GetPsiServices().Finder.FindInheritors(myTypeElement,
			////    SearchDomainFactory.Instance.CreateSearchDomain(solution, true),
			////    (IFindResultConsumer<ITypeElement>)inheritorsConsumer,
			////    NullProgressIndicator.Instance);
			////foreach (ITypeElement typeElement in inheritorsConsumer.FoundElements)
			////    symbolTable = symbolTable.Merge(ResolveUtil.GetSymbolTableByTypeElement(typeElement, SymbolTableMode.FULL, typeElement.Module));
			//ISymbolTable table = symbolTable.Distinct().Filter(new ISymbolFilter[] { MethodFieldPropertyFilter.Instance });
			//if (!useReferenceName)
			//    return table;
			//return table.Filter(GetName(), new[] { ExactMemberNameFilter });
		}

		public override ISymbolFilter[] GetSymbolFilters()
		{
			return new[]
					{
						OurPropertyFilter,
						OurInstanceFilter,
						IsPublicFilter.INSTANCE,
						OverriddenFilter.INSTANCE
					};
		}

		public override ISymbolTable GetCompletionSymbolTable()
		{
			return GetReferenceSymbolTable(false).Filter(GetSymbolFilters());
			//return GetReferenceSymbolTable(false).Filter(new ISymbolFilter[]
			//    {
			//        new MemberNameApplicableTypeMemberFilter(myOwnerDeclaration),
			//        CSharpAutoPropertyBackingFieldFilter.Instance
			//    });
		}

	}

	//public class MemberNameReference : TreeReferenceBase<ILiteralExpression>, ICompleteableReference
	//{
	//    private readonly ITypeElement myTypeElement;
	//    private readonly ICSharpDeclaration myOwnerDeclaration;
	//    private readonly ISymbolFilter myExactNameFilter;
	//    private readonly bool myIsAbstract;

	//    public MemberNameReference(ITypeElement typeElement, ILiteralExpression literal, ICSharpDeclaration ownerDeclaration)
	//        : base(literal)
	//    {
	//        myTypeElement = typeElement;
	//        myOwnerDeclaration = ownerDeclaration;
	//        myIsAbstract = typeElement
	//            .With(x => x as IClass)
	//            .With(x => x.IsAbstract);
	//        myExactNameFilter = new ExactNameFilter((string)literal.ConstantValue.Value);
	//    }

	//    public override ResolveResultWithInfo ResolveWithoutCache()
	//    {
	//        ResolveResultWithInfo resolveResult = GetReferenceSymbolTable(true).GetResolveResult(GetName());
	//        if (!myIsAbstract || !(resolveResult.Info.ResolveErrorType == ResolveErrorType.MULTIPLE_CANDIDATES))
	//            return resolveResult;
	//        return new ResolveResultWithInfo(resolveResult.Result, ResolveErrorType.OK);
	//    }

	//    public override string GetName()
	//    {
	//        return (string)myOwner.ConstantValue.Value;
	//    }

	//    public ISymbolTable GetCompletionSymbolTable()
	//    {
	//        return GetReferenceSymbolTable(false).Filter(new ISymbolFilter[]
	//            {
	//                new MemberNameApplicableTypeMemberFilter(myOwnerDeclaration),
	//                CSharpAutoPropertyBackingFieldFilter.Instance
	//            });
	//    }

	//    //TODO GetReferenceSymbolTable
	//    public override ISymbolTable GetReferenceSymbolTable(bool useReferenceName)
	//    {
	//        ISymbolTable symbolTable = ResolveUtil.GetSymbolTableByTypeElement(myTypeElement, SymbolTableMode.FULL, myTypeElement.Module);
	//        //ISolution solution = myTypeElement.GetSolution();
	//        //solution.GetPsiServices().Finder.FindInheritors(myTypeElement,
	//        //    SearchDomainFactory.Instance.CreateSearchDomain(solution, true),
	//        //    (IFindResultConsumer<ITypeElement>)inheritorsConsumer,
	//        //    NullProgressIndicator.Instance);
	//        //foreach (ITypeElement typeElement in inheritorsConsumer.FoundElements)
	//        //    symbolTable = symbolTable.Merge(ResolveUtil.GetSymbolTableByTypeElement(typeElement, SymbolTableMode.FULL, typeElement.Module));
	//        ISymbolTable table = symbolTable.Distinct().Filter(new ISymbolFilter[] { MethodFieldPropertyFilter.Instance });
	//        if (!useReferenceName)
	//            return table;
	//        return table.Filter(GetName(), new[] { myExactNameFilter });
	//    }

	//    public override TreeTextRange GetTreeTextRange()
	//    {
	//        TreeTextRange contentTreeRange = ((ICSharpLiteralExpression)myOwner).GetStringLiteralContentTreeRange();
	//        return contentTreeRange.Length != 0 ? contentTreeRange : myOwner.GetTreeTextRange();
	//    }

	//    public override IReference BindTo(IDeclaredElement element)
	//    {
	//        IStringLiteralAlterer literalByExpression = StringLiteralAltererUtil.CreateStringLiteralByExpression(myOwner);
	//        literalByExpression.Replace((string)myOwner.ConstantValue.Value, element.ShortName, myOwner.GetPsiModule());
	//        ILiteralExpression expression = literalByExpression.Expression;
	//        if (!myOwner.Equals(expression))
	//            return expression.FindReference<MemberNameReference>() ?? this;
	//        return 
	//            this;
	//    }

	//    public override IReference BindTo(IDeclaredElement element, ISubstitution substitution)
	//    {
	//        return BindTo(element);
	//    }

	//    public override IAccessContext GetAccessContext()
	//    {
	//        return new ElementAccessContext(myOwner);
	//    }

	//    public bool Equals(MemberNameReference other)
	//    {
	//        if (ReferenceEquals(null, other))
	//            return false;
	//        if (ReferenceEquals(this, other))
	//            return true;
	//        return 
	//            Equals(other.myTypeElement, myTypeElement) && 
	//            Equals(other.myOwner, myOwner);
	//    }

	//    public override bool Equals(object obj)
	//    {
	//        if (ReferenceEquals(null, obj))
	//            return false;
	//        if (ReferenceEquals(this, obj))
	//            return true;
	//        return obj.GetType() == typeof(MemberNameReference) && Equals((MemberNameReference)obj);
	//    }

	//    public override int GetHashCode()
	//    {
	//        if (myTypeElement == null)
	//            return 0;
	//        return myTypeElement.GetHashCode() * 29 + myOwner.GetHashCode();
	//    }
	//}
}
