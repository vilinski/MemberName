using System.ComponentModel;

using DataSourceTestProjekt.Properties.Annotations;

namespace DataSourceTestProjekt
{
	public class Class1 : INotifyPropertyChanged
	{
		private string _name;
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		public virtual void OnPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null) 
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
					return;
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		public string Test0 { get; set; }

		public void BindToName()
		{
			new UnderTest().BindToUnderTest(new UnderTest(), "Test1", "Name");
			new UnderTest2().BindToUnderTest(new UnderTest(), "Test2", "Name");
		}
	}

	public class UnderTest
	{
		public void BindToUnderTest(
			UnderTest dataSrc,
			[MemberName(typeof (UnderTest))] string member1,
			[MemberName("dataSrc")] string member2)
		{
		}

		public void BindToUnderTest2(
		object dataSrc,
	[TypeName("DataSourceTestProjekt.UnderTest")] string member1,
	[MemberName("dataSrc")] string member2)
		{
		}

		public string Test1 { get; set; }
	}
	public class UnderTest2
	{
		public void BindToUnderTest(
			UnderTest dataSrc,
			[MemberName(typeof (UnderTest))] string member1,
			[MemberName("dataSrc")] string member2)
		{
		}

		public void BindToUnderTest2(
		object dataSrc,
	[TypeName("DataSourceTestProjekt.UnderTest")] string member1,
	[MemberName("dataSrc")] string member2)
		{
		}

		public string Test2 { get; set; }
	}
}
