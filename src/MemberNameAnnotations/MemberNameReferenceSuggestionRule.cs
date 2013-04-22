using System.Collections.Generic;
using System.Linq;

using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;

namespace MemberName.MemberNameAnnotations
{
	[Language(typeof (CSharpLanguage))]
	public class MemberNameReferenceSuggestionRule : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
	{
		protected override AutocompletionBehaviour GetAutocompletionBehaviour(CSharpCodeCompletionContext specificContext)
		{
			return AutocompletionBehaviour.AutocompleteWithReplace;
		}

		protected override bool IsAvailable(CSharpCodeCompletionContext context)
		{
			return context.TerminatedContext.Reference is MemberNameReference;
		}

		protected override void TransformItems(CSharpCodeCompletionContext context, GroupedItemsCollector collector)
		{
			if (!IsAvailable(context))
				return;
			List<ILookupItem> list = collector.Items.ToList();
			collector.Clear();
			foreach (ILookupItem lookupItem in list)
			{
				var methodsLookupItem = lookupItem as MethodsLookupItem;
				if (methodsLookupItem == null)
					collector.AddAtDefaultPlace(lookupItem);
				else
				{
					var textLookupItem = new TextLookupItem(methodsLookupItem.Text, methodsLookupItem.Image);
					textLookupItem.InitializeRanges(methodsLookupItem.Ranges, context.BasicContext);
					collector.AddAtDefaultPlace(textLookupItem);
				}
			}
		}
	}
}