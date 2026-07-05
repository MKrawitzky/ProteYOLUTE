using System;
using System.Diagnostics;
using System.Windows.Input;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000053 RID: 83
	public class RelayCommand : ICommand
	{
		// Token: 0x06000477 RID: 1143 RVA: 0x0001A2CE File Offset: 0x000184CE
		public RelayCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x0001A2D8 File Offset: 0x000184D8
		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}
			this._execute = execute;
			this._canExecute = canExecute;
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x0001A2FD File Offset: 0x000184FD
		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			Predicate<object> canExecute = this._canExecute;
			return canExecute == null || canExecute(parameter);
		}

		// Token: 0x14000043 RID: 67
		// (add) Token: 0x0600047A RID: 1146 RVA: 0x0001A311 File Offset: 0x00018511
		// (remove) Token: 0x0600047B RID: 1147 RVA: 0x0001A319 File Offset: 0x00018519
		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x0001A321 File Offset: 0x00018521
		public void Execute(object parameter)
		{
			this._execute(parameter);
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x0001A32F File Offset: 0x0001852F
		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}

		// Token: 0x04000284 RID: 644
		private readonly Action<object> _execute;

		// Token: 0x04000285 RID: 645
		private readonly Predicate<object> _canExecute;
	}
}
