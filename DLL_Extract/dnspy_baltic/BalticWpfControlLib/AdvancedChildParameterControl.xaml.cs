using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000007 RID: 7
	public partial class AdvancedChildParameterControl : UserControl
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000010 RID: 16 RVA: 0x00002310 File Offset: 0x00000510
		// (remove) Token: 0x06000011 RID: 17 RVA: 0x00002348 File Offset: 0x00000548
		public event EventHandler ArgumentValueUpdated;

		// Token: 0x06000012 RID: 18 RVA: 0x0000237D File Offset: 0x0000057D
		public AdvancedChildParameterControl(bool isAppServiceMode)
		{
			this.InitializeComponent();
			this._isAppServiceMode = isAppServiceMode;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000013 RID: 19 RVA: 0x000023A8 File Offset: 0x000005A8
		public bool HasError
		{
			get
			{
				return (bool)base.GetValue(AdvancedChildParameterControl.HasErrorProperty);
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000023BC File Offset: 0x000005BC
		public bool Exists(string name)
		{
			return this._childParameters.FirstOrDefault((AdvChildProcParam p) => p.Name == name) != null;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000023F0 File Offset: 0x000005F0
		public void SetParameters(IEnumerable<ChildProcedureParameter> childProcedureParameters, ChildProcedureArguments procArgs, ChildProcedureArguments valuePresets)
		{
			this._childParameters.Clear();
			this._valuePresets.Clear();
			foreach (ChildProcedureArgument item in valuePresets)
			{
				this._valuePresets.Add(new ChildProcedureArgument(item));
			}
			using (IEnumerator<ChildProcedureParameter> enumerator2 = childProcedureParameters.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ChildProcedureParameter param = enumerator2.Current;
					AdvChildProcParam cpp = null;
					if (valuePresets.Find((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name) != null)
					{
						ChildProcedureArgument arg = procArgs.Find((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name);
						if (arg != null)
						{
							cpp = new AdvChildProcParam(param, arg, this._isAppServiceMode);
						}
					}
					if (cpp == null)
					{
						cpp = new AdvChildProcParam(param, this._isAppServiceMode);
					}
					this._childParameters.Add(cpp);
				}
			}
			this.ParameterItems.ItemsSource = this._childParameters;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002518 File Offset: 0x00000718
		public void ResetParameterValues()
		{
			using (List<ChildProcedureArgument>.Enumerator enumerator = this._valuePresets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ChildProcedureArgument param = enumerator.Current;
					AdvChildProcParam p = this._childParameters.Find((AdvChildProcParam x) => x.Header == param.Header && x.Name == param.ProcArg.Name);
					if (p != null)
					{
						p.Value = param.ProcArg.Value;
					}
				}
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000025A0 File Offset: 0x000007A0
		public void ClearErrors()
		{
			foreach (AdvChildProcParam advChildProcParam in this._childParameters)
			{
				advChildProcParam.ErrorMessage = null;
			}
			base.SetValue(AdvancedChildParameterControl.HasErrorProperty, false);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002604 File Offset: 0x00000804
		public void SetError(string parameter, string error)
		{
			AdvChildProcParam param = this._childParameters.FirstOrDefault((AdvChildProcParam pp) => pp.Name.Equals(parameter));
			if (param != null)
			{
				param.ErrorMessage = error;
				base.SetValue(AdvancedChildParameterControl.HasErrorProperty, true);
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002654 File Offset: 0x00000854
		public ChildProcedureArguments CreateChildArguments()
		{
			ChildProcedureArguments arguments = new ChildProcedureArguments();
			foreach (AdvChildProcParam param in this._childParameters)
			{
				arguments.Add(param.ChildArgument);
			}
			return arguments;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000026B4 File Offset: 0x000008B4
		private void ParameterItems_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			EventHandler argumentValueUpdated = this.ArgumentValueUpdated;
			if (argumentValueUpdated == null)
			{
				return;
			}
			argumentValueUpdated(this, EventArgs.Empty);
		}

		// Token: 0x04000009 RID: 9
		public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(AdvancedChildParameterControl), new PropertyMetadata(false));

		// Token: 0x0400000A RID: 10
		private readonly List<AdvChildProcParam> _childParameters = new List<AdvChildProcParam>();

		// Token: 0x0400000B RID: 11
		private readonly ChildProcedureArguments _valuePresets = new ChildProcedureArguments();

		// Token: 0x0400000C RID: 12
		private readonly bool _isAppServiceMode;
	}
}
