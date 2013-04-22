using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.Util.DataStructures;

namespace MemberName.MemberNameAnnotations
{
	[PsiComponent]
	public class MemberNameAnnotationsCache : InvalidatingPsiCache
	{
		public static readonly string MemberNameAttributeShortName = typeof (MemberNameAttribute).Name;

		private static readonly Func<MemberNameAnnotationsCache, ICSharpArgument, ITypeElement>
			CalcTargetTypeElement = (cache, argument) => cache.GetTargetTypeNotCached(argument);
		private static readonly Func<MemberNameAnnotationsCache, ICSharpArgument, IAttributeInstance>
			CalcMemberNameAttribute = (cache, argument) => cache.GetMemberNameAttribute(argument);

		private readonly Cache<ITypeElement> myTargetTypeCache;
		private readonly Cache<IAttributeInstance> myMemberNameCache;
		private readonly ISettingsStore mySettingsStore;
		private readonly ISolution mySolution;

		static MemberNameAnnotationsCache()
		{
		}

		public MemberNameAnnotationsCache(Lifetime lifetime, ISolution solution, ISettingsStore settingsStore,
		                                  IEnumerable<ICustomCodeAnnotationProvider> customProviders)
		{
			mySolution = solution;
			mySettingsStore = settingsStore;
			myTargetTypeCache = new Cache<ITypeElement>(this);
			myMemberNameCache = new Cache<IAttributeInstance>(this);

			mySettingsStore.AdviseChange(
				lifetime,
				mySettingsStore.Schema.GetKey<CodeAnnotationsSettings>(),
				UpdateAnnotationNamespaces);
			UpdateAnnotationNamespaces();
		}

		private IList<string> AnnotationNamespaces { get; set; }
		private string DefaultNamespace { get; set; }

		public void UpdateAnnotationNamespaces()
		{
			//TODO mySettingsStore.BindToContextLive().GetValue(); fix obsolete method
			AnnotationNamespaces =
				mySettingsStore.EnumerateIndexedEntry(mySolution.ToDataContext(), CodeAnnotationsSettingsAccessor.Namespaces)
				//mySettingsStore.BindToContextLive(myLifeTime, ContextRange.ApplicationWide).GetValues(CodeAnnotationsSettingsAccessor.Namespaces);
				               .Where(pair => pair.Second)
				               .Select(pair => pair.First)
				               .ToList();
			// TODO av fix obsolete method
			DefaultNamespace = mySettingsStore.GetValue(mySolution.ToDataContext(), CodeAnnotationsSettingsAccessor.DefaultNamespace);
		}

		public bool IsAnnotationAttribute([NotNull]IAttributeInstance instance, string shortName)
		{
			var clrName = instance.GetClrName();
			return IsAnnotationType(clrName, shortName);
		}

		public bool IsAnnotationType(IClrTypeName clrName, string shortName)
		{
			return
				clrName.ShortName == shortName &&
				AnnotationNamespaces.Contains(clrName.GetNamespaceName());
		}

		protected override void InvalidateOnPhysicalChange()
		{
			myTargetTypeCache.Clear();
			myMemberNameCache.Clear();
		}

		private IAttributeInstance GetMemberNameAnnotation(ICSharpArgument parameter)
		{
			return myMemberNameCache.Get(parameter, CalcMemberNameAttribute);
		}

		private IAttributeInstance GetMemberNameAttribute(ICSharpArgument argument)
		{
			return argument
				.With(x => x.MatchingParameter)
				.With(x => x.Element)
				.If(x => x.ContainingParametersOwner is IMethod)
				.With(x => x
					.GetAttributeInstances(true)
					.FirstOrDefault(y => IsAnnotationAttribute(y, MemberNameAttributeShortName)));
		}

		public ITypeElement GetTargetType(ICSharpArgument argument)
		{
			return argument == null
				       ? null
				       : myTargetTypeCache.Get(argument, CalcTargetTypeElement);
		}

		private ITypeElement GetTargetTypeNotCached(ICSharpArgument argument)
		{
			if (argument != null)
			{
				//var parameterInstance = argument.MatchingParameter; // ArgumentsUtil.GetParameter(argument);
				//if (parameterInstance != null)

				//var parameter = parameterInstance.Element;
				IAttributeInstance memberNameAnnotation = GetMemberNameAnnotation(argument);
				if (memberNameAnnotation != null)
				{
					var positionParameterCount = memberNameAnnotation.PositionParameterCount;
					if (positionParameterCount == 1)
					{
						var memberNameAnnotationParameter = memberNameAnnotation.PositionParameter(0);
						if (memberNameAnnotationParameter.IsConstant)
						{
							var otherArgName = memberNameAnnotationParameter.ConstantValue.Value as string;
							if (!string.IsNullOrWhiteSpace(otherArgName))
							{
								// get other argument type
								ICSharpArgument targetArgument = null;
								foreach (var otherArgument in argument.ContainingArgumentList.Arguments)
								{
									var otherParameter = otherArgument.MatchingParameter;
									if (otherParameter != null)
									{
										if (otherParameter.Element.ShortName == otherArgName)
										{
											targetArgument = otherArgument;
											break;
										}
									}
								}
								if (targetArgument != null)
								{
									var argType = targetArgument.Value.GetExpressionType() as IDeclaredType;
									if (argType != null && argType.IsResolved)
										return argType.GetTypeElement();
								}
							}
						}
						else if (memberNameAnnotationParameter.IsType)
						{
							var type = memberNameAnnotationParameter.TypeValue as IDeclaredType;
							if (type != null)
								return type.GetTypeElement();
						}
					}
					
					var ownerTypeDecl = argument.GetContainingTypeDeclaration();
					if (ownerTypeDecl != null)
						return ownerTypeDecl.DeclaredElement;
					// own type
					//var type = expression.GetExpressionType().ToIType();
				}
			}
			return null;
		}

		private sealed class Cache<T>
		{
			private readonly MemberNameAnnotationsCache myOwner;
			private CompactMap<ICSharpArgument, T> myMembersCache = new CompactMap<ICSharpArgument, T>();

			public Cache(MemberNameAnnotationsCache owner)
			{
				myOwner = owner;
			}

			public T Get(ICSharpArgument attributesOwner, Func<MemberNameAnnotationsCache, ICSharpArgument, T> calculator)
			{
				T t;
				if (myMembersCache.TryGetValue(attributesOwner, out t))
					return t;

				T obj = calculator(myOwner, attributesOwner);

				CompactMap<ICSharpArgument, T> snapshot, newCache;
				do
				{
					snapshot = myMembersCache;
					newCache = myMembersCache.Copy();
					newCache[attributesOwner] = obj;
				} while (!ReferenceEquals(Interlocked.CompareExchange(ref myMembersCache, newCache, snapshot), snapshot));
				return obj;
			}

			public void Clear()
			{
				CompactMap<ICSharpArgument, T> snapshot, newCache;
				do
				{
					snapshot = myMembersCache;
					newCache = new CompactMap<ICSharpArgument, T>();
				} while (!ReferenceEquals(Interlocked.CompareExchange(ref myMembersCache, newCache, snapshot), snapshot));
			}
		}
	}
}