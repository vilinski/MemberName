using System;

namespace DataSourceTestProjekt.Properties.Annotations
{
	/// <summary>
	///     Marks the corresponding string literal as a member name of some declared type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	[UsedImplicitly(ImplicitUseTargetFlags.Default)]
	public class MemberNameAttribute : Attribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MemberNameAttribute" /> class.
		/// </summary>
		/// <param name="name">Method parameter name or class name to get the data source type from.</param>
		public MemberNameAttribute(string name = "")
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MemberNameAttribute" /> class.
		/// </summary>
		/// <param name="type">The data source type specified directly.</param>
		public MemberNameAttribute(Type type)
		{
		}
	}

	/// <summary>
	///     Marks the corresponding string literal as a name of some declared type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	[UsedImplicitly(ImplicitUseTargetFlags.Default)]
	public class TypeNameAttribute : Attribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TypeNameAttribute" /> class.
		/// </summary>
		/// <param name="name">The declared type name.</param>
		public TypeNameAttribute([TypeName]string name = "")
		{
		}
	}
}