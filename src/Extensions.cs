using System;

using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Impl.PsiManagerImpl;
using JetBrains.Util.DataStructures;

using MemberName.MemberNameAnnotations;

namespace MemberName
{
	public static class Extensions
	{
		#region Monads

		/// <summary>
		/// Evaluates the specified function if the <paramref name="input" /> is not <c>null</c>.
		/// </summary>
		/// <typeparam name="TInput">The type of the input.</typeparam>
		/// <typeparam name="TResult">The type of the <paramref name="evaluator" />result.</typeparam>
		/// <param name="input">The input.</param>
		/// <param name="evaluator">The evaluator function.</param>
		/// <param name="failureValue">The optional failure value, returned if .</param>
		/// <returns>
		/// The result of <paramref name="evaluator" /> function if the <paramref name="input" /> is not <c>null</c>;
		/// otherwise returns <c>null</c>.
		/// </returns>
		/// <example>
		///   <code>
		/// this.With(x =&gt; GetSelectedElement&lt;IX&gt;(true))
		/// .With(x =&gt; x.Something)
		/// .With(x =&gt; x.DoY() as Z)
		/// .Do(x =&gt; { /* something */ });
		///   </code>
		///   </example>
		[CanBeNull]
		public static TResult With<TInput, TResult>(this TInput input, Func<TInput, TResult> evaluator, TResult failureValue = default(TResult))
			where TInput : class
		{
			return input != null ? evaluator(input) : failureValue;
		}

		/// <summary>
		/// Returns the <paramref name="input"/> value if the <paramref name="condition"/> is <c>true</c>;
		/// otherwise returns <c>null</c>.
		/// </summary>
		/// <typeparam name="TInput">The type of the input.</typeparam>
		/// <param name="input">The input.</param>
		/// <param name="condition">The condition evaluator function.</param>
		/// <returns>
		/// The <paramref name="input"/> value if it is not <c>null</c> and the <paramref name="condition"/> is <c>true</c>; 
		/// otherwise returns <c>null</c>.
		/// </returns>
		[CanBeNull]
		public static TInput If<TInput>(this TInput input, Func<TInput, bool> condition)
		  where TInput : class
		{
			if (input != null && condition(input)) return input;
			return null;
		}

		/// <summary>
		/// Returns the <paramref name="input"/> value if the condition is <c>false</c>.
		/// </summary>
		/// <typeparam name="TInput">The type of the input.</typeparam>
		/// <param name="input">The input.</param>
		/// <param name="condition">The condition evaluator function.</param>
		/// <returns>
		/// The <paramref name="input"/> value if it is not <c>null</c> and the <paramref name="condition"/> is <c>false</c>; 
		/// otherwise returns <c>null</c>.
		/// </returns>
		[CanBeNull]
		public static TInput Unless<TInput>(this TInput input, Func<TInput, bool> condition)
		  where TInput : class
		{
			if (input != null && !condition(input)) return input;
			return null;
		}

		/// <summary>
		/// Returns the result of <paramref name="evaluator"/> function; 
		/// or the <paramref name="failureValue"/> if <paramref name="input"/> is <c>null</c>.
		/// </summary>
		/// <typeparam name="TInput">The type of the input.</typeparam>
		/// <typeparam name="TResult">The type of the <paramref name="evaluator"/> result.</typeparam>
		/// <param name="input">The input.</param>
		/// <param name="evaluator">The evaluator.</param>
		/// <param name="failureValue">The failure value.</param>
		/// <returns>
		/// Returns the result of <paramref name="evaluator"/> function; 
		/// or the <paramref name="failureValue"/> if <paramref name="input"/> is <c>null</c>.
		/// </returns>
		[CanBeNull]
		public static TResult Return<TInput, TResult>(this TInput input, Func<TInput, TResult> evaluator, TResult failureValue = default (TResult))
			where TInput : class
		{
			return input.With(evaluator, failureValue);
		}

		/// <summary>
		/// Executes the specified <paramref name="action"/> if <paramref name="input"/> is not <c>null</c>.
		/// </summary>
		/// <typeparam name="TInput">The type of the input.</typeparam>
		/// <param name="input">The input.</param>
		/// <param name="action">The action.</param>
		/// <returns>If the <paramref name="input"/> value is not <c>null</c> returns it after the <paramref name="action"/> is executed;
		/// otherwise returns <c>null</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="action" /> is <c>null</c>.</exception>
		[CanBeNull]
		public static TInput Do<TInput>(this TInput input, Action<TInput> action)
		  where TInput : class
		{
			if (action == null)
				throw new ArgumentNullException("action");

			if (input != null)
			{
				action(input);
				return input;
			}
			return null;
		}

		#endregion

		[NotNull]
		public static MemberNameAnnotationsCache GetMemberNameAnnotationsCache(this IPsiServices psiServices)
		{
			return ((PsiManagerImpl)psiServices.PsiManager).GetPsiCache<MemberNameAnnotationsCache>();
		}

		[NotNull]
		public static CompactMap<TKey, TValue> Copy<TKey, TValue>([NotNull] this CompactMap<TKey, TValue> map)
		{
			var copy = new CompactMap<TKey, TValue>();
			foreach (var pair in map)
				copy[pair.Key] = pair.Value;
			return copy;
		}
	}
}
