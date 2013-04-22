using System.Collections.Generic;
using System.Linq;

using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Util;

namespace MemberName.MemberNameAnnotations
{
	public class MemberNameApplicableTypeMemberFilter : ISymbolFilter
	{
		private readonly ICSharpDeclaration myOwnerDeclaration;

		public ResolveErrorType ErrorType
		{
			get { return ResolveErrorType.IGNORABLE; }
		}

		public FilterRunType RunType
		{
			get { return FilterRunType.REGULAR; }
		}

		public MemberNameApplicableTypeMemberFilter(ICSharpDeclaration ownerDeclaration)
		{
			myOwnerDeclaration = ownerDeclaration;
		}

		public IList<ISymbolInfo> FilterArray(IList<ISymbolInfo> data)
		{
			var list = data.ToList();
			foreach (var current in data)
			{
				var typeMember = current.GetDeclaredElement() as ITypeMember;
				if (typeMember != null)
				{
					PredefinedType predefinedType = typeMember.Module.GetPredefinedType();
					var method = typeMember as IMethod;
					if (method != null)
					{
						if (method is IAccessor || 
							method.Parameters.Count > 0 || 
							!HasCorrectType(predefinedType, method.ReturnType))
							list.Remove(current);
					}
					else
					{
						var property = typeMember as IProperty;
						if (property != null)
						{
							if (!HasCorrectType(predefinedType, property.Type))
								list.Remove(current);
						}
						else
						{
							var field = typeMember as IField;
							if (field != null)
							{
								if (!HasCorrectType(predefinedType, field.Type))
									list.Remove(current);
							}
							else
								list.Remove(current);
						}
					}
				}
			}
			return list;
		}

		private bool HasCorrectType(PredefinedType predefinedType, IType type)
		{
			var typeConversionRule = myOwnerDeclaration.GetTypeConversionRule();
			if (type.IsString() || !type.IsSubtypeOf(predefinedType.IEnumerable))
				return false;
			var parameterDeclaration = myOwnerDeclaration as IRegularParameterDeclaration;
			if (parameterDeclaration != null)
				return CheckSingleType(predefinedType, myOwnerDeclaration.GetPsiModule(), type, typeConversionRule, parameterDeclaration.Type);
			var methodDeclaration = myOwnerDeclaration as IMethodDeclaration;
			if (methodDeclaration != null && methodDeclaration.ParameterDeclarations.Count == 1)
				return CheckSingleType(predefinedType, myOwnerDeclaration.GetPsiModule(), type, typeConversionRule, methodDeclaration.ParameterDeclarations[0].Type);
			return true;
		}

		private static bool CheckSingleType(PredefinedType predefinedType, IPsiModule psiModule, IType type, ITypeConversionRule conversionRule, IType typeInDeclaration)
		{
			if (!type.IsGenericOrNonIEnumerable())
			{
				if (!type.IsSubtypeOf(predefinedType.Array))
					return true;
				IDeclaredType scalarType = type.GetScalarType();
				return scalarType != null && scalarType.IsImplicitlyConvertibleTo(typeInDeclaration, conversionRule);
			}
			IDeclaredType ienumerableOf = CollectionTypeUtil.CreateIEnumerableOf(psiModule, typeInDeclaration);
			return ienumerableOf == null || type.IsImplicitlyConvertibleTo(ienumerableOf, conversionRule);
		}
	}
}