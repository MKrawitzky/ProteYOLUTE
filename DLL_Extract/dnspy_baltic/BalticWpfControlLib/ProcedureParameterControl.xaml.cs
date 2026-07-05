using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000030 RID: 48
	public partial class ProcedureParameterControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060002D6 RID: 726 RVA: 0x000138CB File Offset: 0x00011ACB
		// (set) Token: 0x060002D7 RID: 727 RVA: 0x000138D3 File Offset: 0x00011AD3
		public bool IsAppServiceMode { get; set; }

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x060002D8 RID: 728 RVA: 0x000138DC File Offset: 0x00011ADC
		// (remove) Token: 0x060002D9 RID: 729 RVA: 0x00013914 File Offset: 0x00011B14
		public event EventHandler ArgumentValueUpdated;

		// Token: 0x060002DA RID: 730 RVA: 0x00013949 File Offset: 0x00011B49
		public ProcedureParameterControl()
			: this(false)
		{
		}

		// Token: 0x060002DB RID: 731 RVA: 0x00013952 File Offset: 0x00011B52
		public ProcedureParameterControl(bool isAppServiceMode)
		{
			this.InitializeComponent();
			this.IsAppServiceMode = isAppServiceMode;
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060002DC RID: 732 RVA: 0x00013972 File Offset: 0x00011B72
		// (set) Token: 0x060002DD RID: 733 RVA: 0x0001397A File Offset: 0x00011B7A
		public bool IsValid
		{
			get
			{
				return this._hasError;
			}
			set
			{
				this.SetField<bool>(ref this._hasError, value, "IsValid");
			}
		}

		// Token: 0x060002DE RID: 734 RVA: 0x00013990 File Offset: 0x00011B90
		public void SetParameters(IEnumerable<ProcedureParameter> procedureParameters, IEnumerable<ChildProcedureParameter> childProcedureParameters, string imagePath, ProcedureArguments valuePresets = null, ChildProcedureArguments childValuePresets = null)
		{
			this._parameters.Clear();
			ChildProcedureParameter[] childProcedureParametersArray = childProcedureParameters.ToArray<ChildProcedureParameter>();
			using (IEnumerator<ProcedureParameter> enumerator = procedureParameters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ProcedureParameter param = enumerator.Current;
					ObservableCollection<ChildProcParam> childProcParams = new ObservableCollection<ChildProcParam>();
					foreach (ChildProcedureParameter item in childProcedureParametersArray.Where((ChildProcedureParameter x) => x.Header == param.Name))
					{
						ProcedureArgument arg = item.CreateArgument();
						ChildProcParam cpp = new ChildProcParam(item.Header, new ProcedureParameter(item.Name, item.ControlType, item.Type, arg.Value, item.Unit, item.IsService, this.IsAppServiceMode, item.ToolTipText, item.ToolTipImageName, item.Indent, item.Group, 0), item.ToolTipImageName, this.IsAppServiceMode);
						childProcParams.Add(cpp);
					}
					ProcParam pp = ((valuePresets != null && valuePresets.Contains(param.Name)) ? new ProcParam(param, valuePresets[param.Name], imagePath, this.IsAppServiceMode, childProcParams) : new ProcParam(param, imagePath, this.IsAppServiceMode, childProcParams));
					this._parameters.Add(pp);
				}
			}
			this.ParameterItems.ItemsSource = this._parameters;
		}

		// Token: 0x060002DF RID: 735 RVA: 0x00013B50 File Offset: 0x00011D50
		public ValueTuple<ProcedureArguments, ChildProcedureArguments> GetParameters()
		{
			ProcedureArguments theArgs = new ProcedureArguments();
			ChildProcedureArguments theChildArgs = new ChildProcedureArguments();
			List<ProcParam> procParams = this.ParameterItems.ItemsSource as List<ProcParam>;
			if (procParams != null)
			{
				foreach (ProcParam param in procParams)
				{
					theArgs.Add(new ProcedureArgument(param.Name, param.Value, param.Unit, param.IsService, this.IsAppServiceMode, "", "", 0, "", 0));
					foreach (ChildProcParam childParam in param.ChildProcParams)
					{
						theChildArgs.Add(new ChildProcedureArgument(childParam.Header, childParam.Name, childParam.Value, childParam.Unit, childParam.IsService, this.IsAppServiceMode, "", "", 0, ""));
					}
				}
			}
			return new ValueTuple<ProcedureArguments, ChildProcedureArguments>(theArgs, theChildArgs);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x00013C88 File Offset: 0x00011E88
		public void ClearErrors()
		{
			foreach (ProcParam procParam in this._parameters)
			{
				procParam.ErrorMessage = null;
			}
			this.IsValid = true;
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x00013CE0 File Offset: 0x00011EE0
		public void SetError(string parameter, string error)
		{
			ProcParam param = this._parameters.FirstOrDefault((ProcParam pp) => pp.Name.Equals(parameter));
			if (param != null)
			{
				param.ErrorMessage = error;
				this.IsValid = false;
			}
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x00013D24 File Offset: 0x00011F24
		public ProcedureArguments CreateArguments()
		{
			ProcedureArguments arguments = new ProcedureArguments();
			foreach (ProcParam param in this._parameters)
			{
				arguments.Add(param.Argument);
			}
			return arguments;
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x00013D84 File Offset: 0x00011F84
		public static ChildProcedureArguments CreateChildArguments()
		{
			return new ChildProcedureArguments();
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x00013D8B File Offset: 0x00011F8B
		private void ParameterItems_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			EventHandler argumentValueUpdated = this.ArgumentValueUpdated;
			if (argumentValueUpdated == null)
			{
				return;
			}
			argumentValueUpdated(this, EventArgs.Empty);
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x00013DA4 File Offset: 0x00011FA4
		private void RadioButton_Loaded(object sender, RoutedEventArgs e)
		{
			RadioButton rButton = sender as RadioButton;
			if (rButton != null)
			{
				ProcParam p = this._parameters.FirstOrDefault((ProcParam x) => x.Name == (string)rButton.Content);
				if (p != null)
				{
					rButton.GroupName = p.Group;
				}
			}
		}

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x060002E6 RID: 742 RVA: 0x00013DF8 File Offset: 0x00011FF8
		// (remove) Token: 0x060002E7 RID: 743 RVA: 0x00013E30 File Offset: 0x00012030
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060002E8 RID: 744 RVA: 0x00013E65 File Offset: 0x00012065
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x00013E7E File Offset: 0x0001207E
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

		// Token: 0x040001C5 RID: 453
		private readonly List<ProcParam> _parameters = new List<ProcParam>();

		// Token: 0x040001C8 RID: 456
		private bool _hasError;
	}
}
