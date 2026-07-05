using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000016 RID: 22
	public partial class AdvChildParamSettingsUserControl : UserControl
	{
		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000090 RID: 144 RVA: 0x00003A08 File Offset: 0x00001C08
		// (remove) Token: 0x06000091 RID: 145 RVA: 0x00003A40 File Offset: 0x00001C40
		public event AdvChildParamSettingsUserControl.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06000092 RID: 146 RVA: 0x00003A78 File Offset: 0x00001C78
		// (remove) Token: 0x06000093 RID: 147 RVA: 0x00003AB0 File Offset: 0x00001CB0
		public event AdvChildParamSettingsUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000094 RID: 148 RVA: 0x00003AE5 File Offset: 0x00001CE5
		public string Header { get; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00003AED File Offset: 0x00001CED
		public SolidColorBrush HeaderFgColor { get; }

		// Token: 0x06000096 RID: 150 RVA: 0x00003AF8 File Offset: 0x00001CF8
		public AdvChildParamSettingsUserControl(string header, BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
		{
			this.InitializeComponent();
			this._method = method;
			this.Header = header;
			this._instrument = instrument;
			this._experiment = experiment;
			this._advChildParameterControl = new AdvancedChildParameterControl(BalticInstrumentFacade.IsService);
			this.svContent.Content = this._advChildParameterControl;
			this.svContent.DataContext = this._advChildParameterControl;
			this._advChildParameterControl.ArgumentValueUpdated += this.AdvParameterControl_ArgumentValueUpdated;
			this._pInfo = this._instrument.GetElutionProcedure(method.ElutionName);
			using (List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter>.Enumerator enumerator = method.AdvancedSettings.ChildParameters.FindAll((BindableBalticMethod.AdvancedSett.AdvancedChildParameter x) => x.Header == this.Header).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BindableBalticMethod.AdvancedSett.AdvancedChildParameter item = enumerator.Current;
					ChildProcedureParameter info = this._pInfo.AdvChildParameters.FirstOrDefault((ChildProcedureParameter x) => x.Header == item.Header && x.Name == item.Name);
					if (info != null)
					{
						this._advChildProcParams.Add(new ChildProcedureParameter(item.Header, item.Name, "", item.Value.GetType(), item.Value, item.Unit, info.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
						this._presets.Add(new ChildProcedureArgument(item.Header, item.Name, item.DefaultValue, item.Unit, info.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
						this._childProcArgs.Add(new ChildProcedureArgument(item.Header, item.Name, item.Value, item.Unit, info.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
					}
					else
					{
						this._advChildProcParams.Add(new ChildProcedureParameter(item.Header, item.Name, "", item.Value.GetType(), item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
						this._presets.Add(new ChildProcedureArgument(item.Header, item.Name, item.DefaultValue, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
						this._childProcArgs.Add(new ChildProcedureArgument(item.Header, item.Name, item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
					}
				}
			}
			this._advChildParameterControl.SetParameters(this._advChildProcParams, this._childProcArgs, this._presets);
			this.HeaderFgColor = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)method.AdvancedSettings.HeaderFgColor[0], (byte)method.AdvancedSettings.HeaderFgColor[1], (byte)method.AdvancedSettings.HeaderFgColor[2]));
			base.DataContext = this;
			this.ValidateParameters(this._advChildParameterControl);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003F00 File Offset: 0x00002100
		private void CheckModified()
		{
			bool isModified = false;
			if (this.ModificationUpdateEvent != null)
			{
				foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item in this._method.AdvancedSettings.ChildParameters)
				{
					object obj = item.Value;
					if (obj is bool)
					{
						bool b = (bool)obj;
						if (b != (bool)item.DefaultValue)
						{
							isModified = true;
							break;
						}
					}
					else
					{
						obj = item.Value;
						if (obj is double)
						{
							double d = (double)obj;
							if ((int)(d * 1000.0) != (int)((double)item.DefaultValue * 1000.0))
							{
								isModified = true;
								break;
							}
						}
						else
						{
							obj = item.Value;
							if (obj is int)
							{
								int i = (int)obj;
								if (i != (int)item.DefaultValue)
								{
									isModified = true;
									break;
								}
							}
							else
							{
								string s = item.Value as string;
								if (s != null && s != (string)item.DefaultValue)
								{
									isModified = true;
									break;
								}
							}
						}
					}
				}
				this.ModificationUpdateEvent(isModified);
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00004040 File Offset: 0x00002240
		private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
		{
			this._method.AdvancedSettings.RevertChildrenToDefault();
			this._advChildParameterControl.ResetParameterValues();
			this.ValidateParameters(this._advChildParameterControl);
			this.CheckModified();
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00004070 File Offset: 0x00002270
		private void AdvParameterControl_ArgumentValueUpdated(object sender, EventArgs e)
		{
			AdvancedChildParameterControl ppc = sender as AdvancedChildParameterControl;
			if (ppc != null)
			{
				using (List<ChildProcedureArgument>.Enumerator enumerator = ppc.CreateChildArguments().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ChildProcedureArgument item = enumerator.Current;
						BindableBalticMethod.AdvancedSett.AdvancedChildParameter advSetting = this._method.AdvancedSettings.ChildParameters.Find((BindableBalticMethod.AdvancedSett.AdvancedChildParameter x) => x.Header == item.Header && x.Name == item.ProcArg.Name);
						if (advSetting != null)
						{
							advSetting.Value = item.ProcArg.Value;
						}
					}
				}
			}
			this.ValidateParameters(ppc);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00004114 File Offset: 0x00002314
		private void ValidateParameters(AdvancedChildParameterControl ppc)
		{
			BalticMethod method = this._method.ToBalticMethod(null);
			ProcedureInfo elutionProcedure = this._instrument.GetElutionProcedure(method.ElutionName);
			ProcedureArguments arguments = elutionProcedure.CreateArguments();
			ProcedureArguments advArguments = elutionProcedure.CreateAdvancedArguments();
			ChildProcedureArguments advChildArguments = elutionProcedure.CreateAdvancedChildArguments();
			ElutionMethodUtil.PopulateMethodArguments(this._instrument.IsColumnOvenConnected, method, arguments, advArguments, advChildArguments, null, null);
			foreach (ProcedureArgument item in advArguments)
			{
				arguments.Add(new ProcedureArgument(item));
			}
			IEnumerable<ProcedureReportEventArgs> reports = this.RequestValidation(arguments, advChildArguments);
			this.HandleReports(ppc, reports);
			this.CheckModified();
		}

		// Token: 0x0600009B RID: 155 RVA: 0x000041CC File Offset: 0x000023CC
		private void HandleReports(AdvancedChildParameterControl ppc, IEnumerable<ProcedureReportEventArgs> reports)
		{
			ppc.ClearErrors();
			ProcedureReportEventArgs[] procedureReportEventArgsEnumerable = (reports as ProcedureReportEventArgs[]) ?? reports.ToArray<ProcedureReportEventArgs>();
			foreach (ProcedureReportEventArgs report in procedureReportEventArgsEnumerable)
			{
				ppc.SetError(report.Subject, report.Message);
			}
			this._isValidateError = false;
			if (procedureReportEventArgsEnumerable.Length != 0)
			{
				foreach (ProcedureReportEventArgs report2 in procedureReportEventArgsEnumerable)
				{
					if (report2.Header != "" && ppc.Exists(report2.Subject))
					{
						this._isValidateError = true;
						break;
					}
				}
			}
			AdvChildParamSettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidateError);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000427C File Offset: 0x0000247C
		private IEnumerable<ProcedureReportEventArgs> RequestValidation(ProcedureArguments args, ChildProcedureArguments childArgs)
		{
			AdvChildValidationRequestEventArgs val = new AdvChildValidationRequestEventArgs(this._pInfo, args, childArgs);
			EventHandler<ProcedureReportEventArgs> handler = delegate(object _, ProcedureReportEventArgs a)
			{
				val.AddReport(a);
			};
			this._instrument.ValidationMessageReported += handler;
			this._instrument.ValidateMethodProcedureOffLine(this._experiment, val.ProcedureSourceInfo, val.ProcedureSourceArgs, val.ProcedureSourceChildArgs);
			this._instrument.ValidationMessageReported -= handler;
			return val.Reports;
		}

		// Token: 0x0400003B RID: 59
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x0400003C RID: 60
		private bool _isValidateError;

		// Token: 0x0400003D RID: 61
		private readonly ProcedureInfo _pInfo;

		// Token: 0x04000040 RID: 64
		private readonly BindableBalticMethod _method;

		// Token: 0x04000041 RID: 65
		private readonly ExperimentInfo _experiment;

		// Token: 0x04000042 RID: 66
		private readonly List<ChildProcedureParameter> _advChildProcParams = new List<ChildProcedureParameter>();

		// Token: 0x04000043 RID: 67
		private readonly ChildProcedureArguments _presets = new ChildProcedureArguments();

		// Token: 0x04000044 RID: 68
		private readonly ChildProcedureArguments _childProcArgs = new ChildProcedureArguments();

		// Token: 0x04000045 RID: 69
		private readonly AdvancedChildParameterControl _advChildParameterControl;

		// Token: 0x02000096 RID: 150
		// (Invoke) Token: 0x0600068A RID: 1674
		public delegate void ModificationUpdate(bool isModified);

		// Token: 0x02000097 RID: 151
		// (Invoke) Token: 0x0600068E RID: 1678
		public delegate void ValidationUpdate(bool isValid);
	}
}
