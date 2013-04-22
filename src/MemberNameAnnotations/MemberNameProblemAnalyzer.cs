using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;

using MemberName.MemberNameAnnotations.QuickFixes;

namespace MemberName.MemberNameAnnotations
{
	public static class MemberNameProblemAnalyzer
	{
		[ElementProblemAnalyzer(
			new[] { typeof(IRegularParameterDeclaration) }, 
			HighlightingTypes = new[] { typeof(WrongParameterTypeError)})]
		private class ParameterAnalyzer : ElementProblemAnalyzer<IRegularParameterDeclaration>
		{
			protected override void Run(IRegularParameterDeclaration parameter, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
			{
				var predefinedType = parameter.GetPsiModule().GetPredefinedType();
				var typeConversionRule = parameter.GetTypeConversionRule();
				foreach (var attribute in parameter.Attributes)
				{
					var typeReference = attribute.TypeReference;
					if (typeReference != null)
					{
						var typeElement = typeReference.Resolve().DeclaredElement as ITypeElement;
						if (typeElement != null)
						{
							var clrName = typeElement.GetClrName();
							var type = parameter.Type;
							if (Equals(clrName, PredefinedType.CALLER_MEMBER_NAME_ATTRIBUTE_FQN) && 
								!type.IsImplicitlyConvertibleTo(predefinedType.String, typeConversionRule))
								consumer.AddHighlighting(new WrongParameterTypeError(parameter, predefinedType.String)); 
							//TODO make own class for not resolvable [DataSource] type
							// TODO wrong parameter name warning
						}
					}
				}
			}
		}

		[ElementProblemAnalyzer(
			new[] { typeof(ICSharpArgument) }, 
			HighlightingTypes = new[] { typeof(ExplicitCallerInfoArgumentWarning) })]
		private class ArgumentAnalyzer : ElementProblemAnalyzer<ICSharpArgument>
		{
			protected override void Run(ICSharpArgument argument, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
			{
				var matchingParameter = argument.MatchingParameter;
				if (matchingParameter == null)
					return;
				var element = matchingParameter.Element;
				if (!element.HasAttributeInstance(PredefinedType.CALLER_MEMBER_NAME_ATTRIBUTE_FQN, false))
					return;
				if (CheckStringRedundancy(argument))
					consumer.AddHighlighting(new RedundantArgumentValueWarning(element, argument));
				else
				{
					var method = matchingParameter.Element.ContainingParametersOwner as IMethod;
					if (method != null &&
					    element.Module.GetPsiServices().GetCodeAnnotationsCache().IsNotifyPropertyChangedInvocator(method) != null)
						return;
					consumer.AddHighlighting(new ExplicitCallerInfoArgumentWarning(argument));
				}
			}

			private static bool CheckStringRedundancy([NotNull] ICSharpArgument argument)
			{
				//TODO überarbeiten
				var literalExpression = argument.Expression as ICSharpLiteralExpression;
				if (literalExpression == null || 
					!literalExpression.ConstantValue.IsString())
					return false;
				var str = (string)null;
				var memberDeclaration = argument.GetContainingTypeMemberDeclaration();
				if (memberDeclaration is IMethodDeclaration || 
					memberDeclaration is IPropertyDeclaration || 
					memberDeclaration is IEventDeclaration)
					str = memberDeclaration.DeclaredName;
				else if (memberDeclaration is IIndexerDeclaration && 
					memberDeclaration.DeclaredElement != null)
					str = memberDeclaration.DeclaredElement.ShortName;
				return str == literalExpression.ConstantValue.Value as string;
			}
		}
	}
}
