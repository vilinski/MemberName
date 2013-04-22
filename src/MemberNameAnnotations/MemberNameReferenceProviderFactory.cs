using System;
using System.Collections.Generic;
using System.Threading;

using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace MemberName.MemberNameAnnotations
{
	[ReferenceProviderFactory]
	internal class MemberNameReferenceProviderFactory : IReferenceProviderFactory
	{
		private readonly MemberNameAnnotationsCache myCache;
		private readonly MemberNameReferenceProvider myProvider;
		private Action myOnChanged;

		public MemberNameReferenceProviderFactory(MemberNameAnnotationsCache memberNameAnnotationsCache)
		{
			myCache = memberNameAnnotationsCache;
			myProvider = new MemberNameReferenceProvider(this);
		}

		public event Action OnChanged
		{
			add
			{
				Action action = myOnChanged;
				Action comparand;
				do
				{
					comparand = action;
					action = Interlocked.CompareExchange(ref myOnChanged, comparand + value, comparand);
				} while (action != comparand);
			}
			remove
			{
				Action action = myOnChanged;
				Action comparand;
				do
				{
					comparand = action;
					action = Interlocked.CompareExchange(ref myOnChanged, comparand - value, comparand);
				} while (action != comparand);
			}
		}

		public IReferenceFactory CreateFactory(IPsiSourceFile sourceFile, IFile file)
		{
			if (file is ICSharpFile)
				return myProvider;
			return null;
		}

		private sealed class MemberNameReferenceProvider : IReferenceFactory
		{
			private readonly MemberNameReferenceProviderFactory myFactory;

			public MemberNameReferenceProvider(MemberNameReferenceProviderFactory factory)
			{
				myFactory = factory;
			}

			public IReference[] GetReferences(ITreeNode element, IReference[] oldReferences)
			{
				var argument = element as ICSharpArgument;
				if (argument != null)
				{
					var typeElement = myFactory.myCache.GetTargetType(argument);
					if (typeElement != null)
						return new[] {TryReuseOld<MemberNameReference>(oldReferences) ?? new MemberNameReference(argument, typeElement)};
				}
				return EmptyArray<IReference>.Instance;
			}

			public bool HasReference(ITreeNode element, ICollection<string> names)
			{
				ICSharpLiteralExpression literalExpression;
				var csharpArgument = element as ICSharpArgument;
				if (csharpArgument != null)
					literalExpression = csharpArgument.Value as ICSharpLiteralExpression;
				else
				{
					var assignmentExpression = element as IAssignmentExpression;
					if (assignmentExpression == null)
						return false;
					literalExpression = assignmentExpression.Source as ICSharpLiteralExpression;
				}
				return HasReference(names, literalExpression);
			}

			private static bool HasReference(ICollection<string> names, IConstantValueOwner literalExpression)
			{
				if (literalExpression == null)
					return false;
				var constantValue = literalExpression.ConstantValue;
				return constantValue.IsString() && names.Contains((string) constantValue.Value);
			}

			private static IReference TryReuseOld<TReference>(IList<IReference> oldReferences)
				where TReference : class, IReference
			{
				return
					oldReferences != null &&
					oldReferences.Count == 1 &&
					oldReferences[0] is TReference
						? oldReferences[0]
						: null;
			}

			private static IParameter GetStringLiteralParameter(ICSharpArgument argument)
			{
				var expression = argument.Value;
				var literalExpression = expression as ICSharpLiteralExpression;
				if (literalExpression == null)
					return null;
				ITokenNode literal = literalExpression.Literal;
				if (literal == null)
					return null;
				if (literal.GetTokenType() != CSharpTokenType.STRING_LITERAL)
					return null;
				var parameter = ArgumentsUtil.GetParameter(argument);
				return parameter == null ? null : parameter.Element;
			}
		}
	}
}