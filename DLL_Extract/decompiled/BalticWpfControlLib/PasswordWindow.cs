using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Syncfusion.UI.Xaml.TextInputLayout;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class PasswordWindow : Window, IComponentConnector
{
	public class PasswordViewModel : NotificationObject
	{
		private bool _isError;

		private string _errorNotifyText = string.Empty;

		private string _id = GenerateID();

		public bool IsError
		{
			get
			{
				return _isError;
			}
			set
			{
				_isError = value;
				RaisePropertyChanged("IsError");
			}
		}

		public string ErrorNotifyText
		{
			get
			{
				return _errorNotifyText;
			}
			set
			{
				_errorNotifyText = value;
				RaisePropertyChanged("ErrorNotifyText");
			}
		}

		public string ID
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
				RaisePropertyChanged("ID");
			}
		}

		private static string GenerateID()
		{
			char[] array = new char[8];
			Random random = new Random();
			for (int i = 0; i < array.Length; i++)
			{
				if (i == 0 || i == array.Length - 1)
				{
					array[i] = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz"[random.Next("ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz".Length)];
				}
				else
				{
					array[i] = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz123456789"[random.Next("ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz123456789".Length)];
				}
			}
			return new string(array);
		}
	}

	internal SfTextInputLayout idInputLayout;

	internal SfTextInputLayout pwdInputLayout;

	internal PasswordBox pswdBox;

	internal Button btnOK;

	private bool _contentLoaded;

	public PasswordViewModel Vm { get; }

	public PasswordWindow()
	{
		InitializeComponent();
		Vm = new PasswordViewModel();
		base.DataContext = Vm;
	}

	private void btnOK_Click(object sender, RoutedEventArgs e)
	{
		PasswordBox passwordBox = pswdBox;
		bool flag = true;
		string text = string.Concat(Vm.ID.Where((char c) => !char.IsDigit(c)));
		text = char.ToUpper(text[0]) + text.Substring(1, text.Length - 2).ToLower() + char.ToUpper(text[text.Length - 1]);
		if (passwordBox.Password != text)
		{
			flag = false;
		}
		if (!flag)
		{
			Vm.IsError = true;
			Vm.ErrorNotifyText = "invalid password";
		}
		else
		{
			Vm.IsError = false;
			base.DialogResult = true;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/passwordwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			idInputLayout = (SfTextInputLayout)target;
			break;
		case 2:
			pwdInputLayout = (SfTextInputLayout)target;
			break;
		case 3:
			pswdBox = (PasswordBox)target;
			break;
		case 4:
			btnOK = (Button)target;
			btnOK.Click += btnOK_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
