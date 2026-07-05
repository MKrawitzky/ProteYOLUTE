using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000043 RID: 67
	public abstract class BindableBase : INotifyPropertyChanged
	{
		// Token: 0x14000042 RID: 66
		// (add) Token: 0x060003DF RID: 991 RVA: 0x00018AC0 File Offset: 0x00016CC0
		// (remove) Token: 0x060003E0 RID: 992 RVA: 0x00018AF8 File Offset: 0x00016CF8
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060003E1 RID: 993 RVA: 0x00018B2D File Offset: 0x00016D2D
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00018B46 File Offset: 0x00016D46
		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}
			field = value;
			this.OnPropertyChanged(propertyName);
			return true;
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x00018B6C File Offset: 0x00016D6C
		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			return this.SetField<T>(ref storage, value, propertyName);
		}
	}
}
