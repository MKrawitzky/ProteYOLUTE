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

namespace BalticWpfControlLib
{
	// Token: 0x0200002D RID: 45
	public partial class PasswordWindow : Window
	{
		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600029F RID: 671 RVA: 0x00012BAA File Offset: 0x00010DAA
		public PasswordWindow.PasswordViewModel Vm { get; }

		// Token: 0x060002A0 RID: 672 RVA: 0x00012BB2 File Offset: 0x00010DB2
		public PasswordWindow()
		{
			this.InitializeComponent();
			this.Vm = new PasswordWindow.PasswordViewModel();
			base.DataContext = this.Vm;
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x00012BD8 File Offset: 0x00010DD8
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			PasswordBox passwordBox = this.pswdBox;
			bool isValid = true;
			string generatedPw = string.Concat<char>(this.Vm.ID.Where((char c) => !char.IsDigit(c)));
			generatedPw = char.ToUpper(generatedPw[0]).ToString() + generatedPw.Substring(1, generatedPw.Length - 2).ToLower() + char.ToUpper(generatedPw[generatedPw.Length - 1]).ToString();
			if (passwordBox.Password != generatedPw)
			{
				isValid = false;
			}
			if (!isValid)
			{
				this.Vm.IsError = true;
				this.Vm.ErrorNotifyText = "invalid password";
				return;
			}
			this.Vm.IsError = false;
			base.DialogResult = new bool?(true);
		}

		// Token: 0x020000F5 RID: 245
		public class PasswordViewModel : NotificationObject
		{
			// Token: 0x17000173 RID: 371
			// (get) Token: 0x06000793 RID: 1939 RVA: 0x0003D83E File Offset: 0x0003BA3E
			// (set) Token: 0x06000794 RID: 1940 RVA: 0x0003D846 File Offset: 0x0003BA46
			public bool IsError
			{
				get
				{
					return this._isError;
				}
				set
				{
					this._isError = value;
					this.RaisePropertyChanged("IsError");
				}
			}

			// Token: 0x17000174 RID: 372
			// (get) Token: 0x06000795 RID: 1941 RVA: 0x0003D85A File Offset: 0x0003BA5A
			// (set) Token: 0x06000796 RID: 1942 RVA: 0x0003D862 File Offset: 0x0003BA62
			public string ErrorNotifyText
			{
				get
				{
					return this._errorNotifyText;
				}
				set
				{
					this._errorNotifyText = value;
					this.RaisePropertyChanged("ErrorNotifyText");
				}
			}

			// Token: 0x17000175 RID: 373
			// (get) Token: 0x06000797 RID: 1943 RVA: 0x0003D876 File Offset: 0x0003BA76
			// (set) Token: 0x06000798 RID: 1944 RVA: 0x0003D87E File Offset: 0x0003BA7E
			public string ID
			{
				get
				{
					return this._id;
				}
				set
				{
					this._id = value;
					this.RaisePropertyChanged("ID");
				}
			}

			// Token: 0x06000799 RID: 1945 RVA: 0x0003D894 File Offset: 0x0003BA94
			private static string GenerateID()
			{
				char[] stringChars = new char[8];
				Random random = new Random();
				for (int i = 0; i < stringChars.Length; i++)
				{
					if (i == 0 || i == stringChars.Length - 1)
					{
						stringChars[i] = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz"[random.Next("ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz".Length)];
					}
					else
					{
						stringChars[i] = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz123456789"[random.Next("ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz123456789".Length)];
					}
				}
				return new string(stringChars);
			}

			// Token: 0x0400040B RID: 1035
			private bool _isError;

			// Token: 0x0400040C RID: 1036
			private string _errorNotifyText = string.Empty;

			// Token: 0x0400040D RID: 1037
			private string _id = PasswordWindow.PasswordViewModel.GenerateID();
		}
	}
}
