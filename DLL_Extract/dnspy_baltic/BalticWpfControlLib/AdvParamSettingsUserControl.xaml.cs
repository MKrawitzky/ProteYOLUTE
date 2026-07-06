// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
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
	// Token: 0x02000018 RID: 24
	public partial class AdvParamSettingsUserControl : UserControl
	{
		// Token: 0x1400000B RID: 11
		// (add) Token: 0x060000A3 RID: 163 RVA: 0x000043DC File Offset: 0x000025DC
		// (remove) Token: 0x060000A4 RID: 164 RVA: 0x00004414 File Offset: 0x00002614
		public event AdvParamSettingsUserControl.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060000A5 RID: 165 RVA: 0x0000444C File Offset: 0x0000264C
		// (remove) Token: 0x060000A6 RID: 166 RVA: 0x00004484 File Offset: 0x00002684
		public event AdvParamSettingsUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x000044B9 File Offset: 0x000026B9
		public string Header { get; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000A8 RID: 168 RVA: 0x000044C1 File Offset: 0x000026C1
		public SolidColorBrush HeaderFgColor { get; }

		// Token: 0x060000A9 RID: 169 RVA: 0x000044CC File Offset: 0x000026CC
		public AdvParamSettingsUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment, bool isService)
		{
			this.InitializeComponent();
			this._method = method;
			this._instrument = instrument;
			this._experiment = experiment;
			this._advParamUserControl = new AdvancedParameterControl(method, instrument, experiment, isService);
			this._advParamUserControl.ValidationUpdateEvent += this.AdvParamUserControlValidationUpdateEvent;
			this._advParamUserControl.ModificationUpdateEvent += this.AdvParamUserControlModificationUpdateEvent;
			this.advScroll.Content = this._advParamUserControl;
			this.advScroll.DataContext = this._advParamUserControl;
			this._advParamUserControl.ArgumentValueUpdated += this.AdvParameterControl_ArgumentValueUpdated;
			this._pInfo = this._instrument.GetElutionProcedure(method.ElutionName);
			using (List<BindableBalticMethod.AdvancedSett.AdvancedParameter>.Enumerator enumerator = method.AdvancedSettings.Parameters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BindableBalticMethod.AdvancedSett.AdvancedParameter item2 = enumerator.Current;
					ProcedureParameter info = this._pInfo.AdvParameters.FirstOrDefault((ProcedureParameter x) => x.Name == item2.Name);
					if (info != null)
					{
						this._advProcParams.Add(new ProcedureParameter(item2.Name, "", item2.Value.GetType(), item2.Value, item2.Unit, info.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
						this._presets.Add(new ProcedureArgument(item2.Name, item2.DefaultValue, item2.Unit, info.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
						this._procArgs.Add(new ProcedureArgument(item2.Name, item2.Value, item2.Unit, info.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
					}
					else
					{
						this._advProcParams.Add(new ProcedureParameter(item2.Name, "", item2.Value.GetType(), item2.Value, item2.Unit, false, false, "", "", 0, "", 0));
						this._presets.Add(new ProcedureArgument(item2.Name, item2.DefaultValue, item2.Unit, false, false, "", "", 0, "", 0));
						this._procArgs.Add(new ProcedureArgument(item2.Name, item2.Value, item2.Unit, false, false, "", "", 0, "", 0));
					}
				}
			}
			using (List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter>.Enumerator enumerator2 = method.AdvancedSettings.ChildParameters.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					BindableBalticMethod.AdvancedSett.AdvancedChildParameter item = enumerator2.Current;
					if (this._pInfo.AdvChildParameters.FirstOrDefault((ChildProcedureParameter x) => x.Header == item.Header && x.Name == item.Name) != null)
					{
						this._advChildProcParams.Add(new ChildProcedureParameter(item.Header, item.Name, "", item.Value.GetType(), item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
						this._childPresets.Add(new ChildProcedureArgument(item.Header, item.Name, item.DefaultValue, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
						this._childProcArgs.Add(new ChildProcedureArgument(item.Header, item.Name, item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
					}
					else
					{
						this._advChildProcParams.Add(new ChildProcedureParameter(item.Header, item.Name, "", item.Value.GetType(), item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, "", 0));
						this._childPresets.Add(new ChildProcedureArgument(item.Header, item.Name, item.DefaultValue, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
						this._childProcArgs.Add(new ChildProcedureArgument(item.Header, item.Name, item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService, "", "", 0, ""));
					}
				}
			}
			this._advParamUserControl.SetParameters(this._advProcParams, this._procArgs, this._advChildProcParams, this._childProcArgs, this._presets, this._childPresets);
			this.Header = method.AdvancedSettings.Header;
			this.HeaderFgColor = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)method.AdvancedSettings.HeaderFgColor[0], (byte)method.AdvancedSettings.HeaderFgColor[1], (byte)method.AdvancedSettings.HeaderFgColor[2]));
			base.DataContext = this;
			this.ValidateParameters();
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00004BB8 File Offset: 0x00002DB8
		private void AdvParamUserControlModificationUpdateEvent(bool isModified)
		{
			AdvParamSettingsUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent != null)
			{
				modificationUpdateEvent(isModified);
			}
			if (!isModified)
			{
				this.CheckModified();
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00004BD5 File Offset: 0x00002DD5
		private void AdvParamUserControlValidationUpdateEvent(bool isValid)
		{
			AdvParamSettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(isValid);
			}
			this.ValidateParameters();
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00004BEF File Offset: 0x00002DEF
		public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
		{
			this._experiment = experiment;
			this._method = method;
			this.CheckModified();
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004C08 File Offset: 0x00002E08
		private void CheckModified()
		{
			bool isModified = false;
			if (this.ModificationUpdateEvent != null)
			{
				foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter item in this._method.AdvancedSettings.Parameters)
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
				foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item2 in this._method.AdvancedSettings.ChildParameters)
				{
					object obj = item2.Value;
					if (obj is bool)
					{
						bool b2 = (bool)obj;
						if (b2 != (bool)item2.DefaultValue)
						{
							isModified = true;
							break;
						}
					}
					else
					{
						obj = item2.Value;
						if (obj is double)
						{
							double d2 = (double)obj;
							if ((int)(d2 * 1000.0) != (int)((double)item2.DefaultValue * 1000.0))
							{
								isModified = true;
								break;
							}
						}
						else
						{
							obj = item2.Value;
							if (obj is int)
							{
								int j = (int)obj;
								if (j != (int)item2.DefaultValue)
								{
									isModified = true;
									break;
								}
							}
							else
							{
								string s2 = item2.Value as string;
								if (s2 != null && s2 != (string)item2.DefaultValue)
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

		// Token: 0x060000AE RID: 174 RVA: 0x00004E68 File Offset: 0x00003068
		private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
		{
			this._method.AdvancedSettings.RevertToDefault();
			this._advParamUserControl.ResetParameterValues();
			this.ValidateParameters(this._advParamUserControl);
			this.CheckModified();
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004E98 File Offset: 0x00003098
		private void AdvParameterControl_ArgumentValueUpdated(object sender, EventArgs e)
		{
			AdvancedParameterControl ppc = sender as AdvancedParameterControl;
			if (ppc != null)
			{
				using (IEnumerator<ProcedureArgument> enumerator = ppc.CreateArguments().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ProcedureArgument item = enumerator.Current;
						BindableBalticMethod.AdvancedSett.AdvancedParameter advSetting = this._method.AdvancedSettings.Parameters.Find((BindableBalticMethod.AdvancedSett.AdvancedParameter x) => x.Name == item.Name);
						if (advSetting != null)
						{
							advSetting.Value = item.Value;
						}
					}
				}
			}
			this.ValidateParameters(ppc);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004F30 File Offset: 0x00003130
		public void ValidateParameters()
		{
			this.ValidateParameters(this._advParamUserControl);
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004F40 File Offset: 0x00003140
		private void ValidateParameters(AdvancedParameterControl ppc)
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

		// Token: 0x060000B2 RID: 178 RVA: 0x00004FF8 File Offset: 0x000031F8
		private void HandleReports(AdvancedParameterControl ppc, IEnumerable<ProcedureReportEventArgs> reports)
		{
			ppc.ClearErrors();
			ProcedureReportEventArgs[] procedureReportEventArgsEnumerable = (reports as ProcedureReportEventArgs[]) ?? reports.ToArray<ProcedureReportEventArgs>();
			foreach (ProcedureReportEventArgs report in procedureReportEventArgsEnumerable)
			{
				ppc.SetError(report.Header, report.Subject, report.Message);
			}
			this._isValidateError = procedureReportEventArgsEnumerable.Any((ProcedureReportEventArgs r) => !r.IsGradientSubject);
			AdvParamSettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidateError);
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x0000508C File Offset: 0x0000328C
		private IEnumerable<ProcedureReportEventArgs> RequestValidation(ProcedureArguments args, ChildProcedureArguments childArgs)
		{
			AdvValidationRequestEventArgs val = new AdvValidationRequestEventArgs(this._pInfo, args, childArgs);
			EventHandler<ProcedureReportEventArgs> handler = delegate(object _, ProcedureReportEventArgs a)
			{
				val.AddReport(a);
			};
			this._instrument.ValidationMessageReported += handler;
			this._instrument.ValidateMethodProcedureOffLine(this._experiment, val.ProcedureSourceInfo, val.ProcedureSourceArgs, val.ProcedureSourceChildArgs);
			this._instrument.ValidationMessageReported -= handler;
			if (val.ProcedureSourceArgs.Contains("calibrantTime"))
			{
				this._method.AdvancedSettings.CalibrantTime = (double)val.ProcedureSourceArgs["calibrantTime"].Value;
			}
			return val.Reports;
		}

		// Token: 0x0400004D RID: 77
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x0400004E RID: 78
		private bool _isValidateError;

		// Token: 0x0400004F RID: 79
		private readonly ProcedureInfo _pInfo;

		// Token: 0x04000052 RID: 82
		private BindableBalticMethod _method;

		// Token: 0x04000053 RID: 83
		private ExperimentInfo _experiment;

		// Token: 0x04000054 RID: 84
		private readonly AdvancedParameterControl _advParamUserControl;

		// Token: 0x04000055 RID: 85
		private readonly List<ProcedureParameter> _advProcParams = new List<ProcedureParameter>();

		// Token: 0x04000056 RID: 86
		private readonly List<ChildProcedureParameter> _advChildProcParams = new List<ChildProcedureParameter>();

		// Token: 0x04000057 RID: 87
		private readonly ProcedureArguments _presets = new ProcedureArguments();

		// Token: 0x04000058 RID: 88
		private readonly ProcedureArguments _procArgs = new ProcedureArguments();

		// Token: 0x04000059 RID: 89
		private readonly ChildProcedureArguments _childPresets = new ChildProcedureArguments();

		// Token: 0x0400005A RID: 90
		private readonly ChildProcedureArguments _childProcArgs = new ChildProcedureArguments();

		// Token: 0x0200009B RID: 155
		// (Invoke) Token: 0x06000698 RID: 1688
		public delegate void ModificationUpdate(bool isModified);

		// Token: 0x0200009C RID: 156
		// (Invoke) Token: 0x0600069C RID: 1692
		public delegate void ValidationUpdate(bool isValid);
	}
}
