// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib
{
	// Token: 0x0200001A RID: 26
	public partial class AnalyticalColEquilUserControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060000B9 RID: 185 RVA: 0x00005220 File Offset: 0x00003420
		// (remove) Token: 0x060000BA RID: 186 RVA: 0x00005258 File Offset: 0x00003458
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060000BB RID: 187 RVA: 0x0000528D File Offset: 0x0000348D
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000BC RID: 188 RVA: 0x000052A6 File Offset: 0x000034A6
		// (set) Token: 0x060000BD RID: 189 RVA: 0x000052B8 File Offset: 0x000034B8
		public double Scale
		{
			get
			{
				return this.Method.SeparationColumnEquil.Scale;
			}
			set
			{
				this.Method.SeparationColumnEquil.Scale = value;
				this.NotifyPropertyChanged("Scale");
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000BE RID: 190 RVA: 0x000052D6 File Offset: 0x000034D6
		// (set) Token: 0x060000BF RID: 191 RVA: 0x0000530B File Offset: 0x0000350B
		public double Pressure
		{
			get
			{
				if (!this._isPressurePSI)
				{
					return this.Method.SeparationColumnEquil.Pressure;
				}
				return this.Method.SeparationColumnEquil.Pressure / 0.0689475729;
			}
			set
			{
				this.Method.SeparationColumnEquil.Pressure = (this._isPressurePSI ? (value * 0.0689475729) : value);
				this.NotifyPropertyChanged("Pressure");
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000C0 RID: 192 RVA: 0x0000533E File Offset: 0x0000353E
		// (set) Token: 0x060000C1 RID: 193 RVA: 0x00005346 File Offset: 0x00003546
		public BindableBalticMethod Method { get; private set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x0000534F File Offset: 0x0000354F
		public double ColumnVolume
		{
			get
			{
				return this.Method.SeparationColumnVolume;
			}
		}

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x060000C3 RID: 195 RVA: 0x0000535C File Offset: 0x0000355C
		// (remove) Token: 0x060000C4 RID: 196 RVA: 0x00005394 File Offset: 0x00003594
		public event AnalyticalColEquilUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x060000C5 RID: 197 RVA: 0x000053CC File Offset: 0x000035CC
		// (remove) Token: 0x060000C6 RID: 198 RVA: 0x00005404 File Offset: 0x00003604
		public event AnalyticalColEquilUserControl.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x00005439 File Offset: 0x00003639
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x00005441 File Offset: 0x00003641
		public string PressureToolTip
		{
			get
			{
				return this._pressureTolTip;
			}
			set
			{
				this._pressureTolTip = value;
				this.NotifyPropertyChanged("PressureToolTip");
			}
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00005455 File Offset: 0x00003655
		public AnalyticalColEquilUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
		{
			this.Method = method;
			this._instrument = instrument;
			this._experiment = experiment;
			this.InitializeComponent();
			base.DataContext = this;
			this.SetDefaultToolTip();
		}

		// Token: 0x060000CA RID: 202 RVA: 0x00005488 File Offset: 0x00003688
		public AnalyticalColEquilUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, bool isPressurePSI, ExperimentInfo experiment)
		{
			this.Method = method;
			this._instrument = instrument;
			this._isPressurePSI = isPressurePSI;
			this._experiment = experiment;
			this.InitializeComponent();
			base.DataContext = this;
			this.lblPressUnits.Content = (this._isPressurePSI ? "PSI" : "bar");
			this.SetDefaultToolTip();
		}

		// Token: 0x060000CB RID: 203 RVA: 0x000054EC File Offset: 0x000036EC
		private void SetDefaultToolTip()
		{
			try
			{
				if (this._experiment != null)
				{
					if (this._experiment.Separator != null)
					{
						this.PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this._experiment.Separator.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0), this._isPressurePSI ? "PSI" : "bar");
					}
					else
					{
						this.PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this._experiment.Separator.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0), this._isPressurePSI ? "PSI" : "bar");
					}
				}
				else
				{
					this.PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this._experiment.Separator.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0), this._isPressurePSI ? "PSI" : "bar");
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060000CC RID: 204 RVA: 0x000056D4 File Offset: 0x000038D4
		public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
		{
			this._experiment = experiment;
			this.Method = method;
			this.Scale = this.Method.SeparationColumnEquil.Scale;
			this.Pressure = this.Method.SeparationColumnEquil.Pressure / (this._isPressurePSI ? 0.0689475729 : 1.0);
			AnalyticalColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent != null)
			{
				modificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)));
			}
			this._isPressIllegalChar = false;
			this._isScaleIllegalChar = false;
			AnalyticalColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(true);
			}
			this.ResetErrorConditions();
		}

		// Token: 0x060000CD RID: 205 RVA: 0x000057F4 File Offset: 0x000039F4
		private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
		{
			this.Method.SeparationColumnEquil.RevertToDefault();
			this.Scale = this.Method.SeparationColumnEquil.Scale;
			this.Pressure = this.Method.SeparationColumnEquil.Pressure / (this._isPressurePSI ? 0.0689475729 : 1.0);
			AnalyticalColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent != null)
			{
				modificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)));
			}
			this._isPressIllegalChar = false;
			this._isScaleIllegalChar = false;
			AnalyticalColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(true);
			}
			this.ResetErrorConditions();
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00005913 File Offset: 0x00003B13
		private void txtScale_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				this.Scale -= 1.0;
			}
			else
			{
				this.Scale += 1.0;
			}
			this.ValidateParameters();
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00005954 File Offset: 0x00003B54
		private void txtPressure_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				if (this._isPressurePSI)
				{
					this.Pressure -= 10.0;
				}
				else
				{
					this.Pressure -= 0.1;
				}
			}
			else if (this._isPressurePSI)
			{
				this.Pressure += (this._isPressurePSI ? 10.0 : 0.1);
			}
			else
			{
				this.Pressure += 0.1;
			}
			this.ValidateParameters();
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x000059F4 File Offset: 0x00003BF4
		private void txtScale_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			AnalyticalColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)));
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00005A98 File Offset: 0x00003C98
		private void txtPressure_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			AnalyticalColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.SeparationColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)));
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00005B3C File Offset: 0x00003D3C
		private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
		{
			if (Severity.Error.Equals(e.Severity))
			{
				this._isValidateError = true;
				this.SetErrorCondition(e.Subject, e.Message);
			}
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00005B80 File Offset: 0x00003D80
		public void ValidateParameters()
		{
			if (this._instrument == null)
			{
				return;
			}
			BalticMethod method = this.Method.ToBalticMethod(null);
			ProcedureInfo procedure = this._instrument.GetElutionProcedure(method.ElutionName);
			ProcedureArguments arguments = procedure.CreateArguments();
			ProcedureArguments advArguments = procedure.CreateAdvancedArguments();
			ChildProcedureArguments advChildArguments = procedure.CreateAdvancedChildArguments();
			ElutionMethodUtil.PopulateMethodArguments(this._instrument.IsColumnOvenConnected, method, arguments, advArguments, advChildArguments, null, null);
			foreach (ProcedureArgument item in advArguments)
			{
				arguments.Add(new ProcedureArgument(item));
			}
			this._instrument.ValidationMessageReported += this.ValidationErrorHandler;
			try
			{
				this._isValidateError = (this._isValidateScaleError = (this._isValidatePressError = false));
				if (this._experiment != null)
				{
					this._instrument.ValidateMethodProcedureOffLine(this._experiment, procedure, arguments, advChildArguments);
					this.Method.SeparationColumnEquil.EquilTime = (double)arguments["separator_equil_time"].Value;
					if (arguments.Contains("calibrantTime"))
					{
						this.Method.AdvancedSettings.CalibrantTime = (double)arguments["calibrantTime"].Value;
					}
				}
				else
				{
					this._instrument.ValidateProcedureOffLine(procedure, arguments, advChildArguments);
				}
				if (!this._isValidateError)
				{
					this.ResetErrorConditions();
				}
			}
			finally
			{
				this._instrument.ValidationMessageReported -= this.ValidationErrorHandler;
			}
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00005D18 File Offset: 0x00003F18
		private void SetErrorCondition(string subject, string message)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (subject == "separator_equilibration_pressure")
				{
					string tempMsg = message;
					if (this._isPressurePSI && message.ToLower().Contains("value must be between"))
					{
						double minPress = 50.0;
						double maxPress = 1000.0;
						try
						{
							string[] numbers = Regex.Split(message, "([-+]?[0-9]*\\.?[0-9]+)");
							int idx = 0;
							if (numbers.Length > 1)
							{
								string[] array = numbers;
								for (int i = 0; i < array.Length; i++)
								{
									double retNum;
									if (double.TryParse(Convert.ToString(array[i]), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum))
									{
										if (idx == 0)
										{
											minPress = retNum;
										}
										else if (idx == 1)
										{
											maxPress = retNum;
										}
										idx++;
									}
								}
								tempMsg = string.Format(CultureInfo.InvariantCulture, "Value must be between {0:0.0} and {1:0.0}", minPress / 0.0689475729, maxPress / 0.0689475729);
							}
						}
						catch (Exception)
						{
						}
					}
					this.bdrPressure.BorderBrush = Brushes.Red;
					this.PressureToolTip = tempMsg;
					this._isValidatePressError = true;
				}
				else if (subject == "separator_equilibration_volumemultiplier")
				{
					this.bdrScale.BorderBrush = Brushes.Red;
					this.txtScale.ToolTip = message;
					this._isValidateScaleError = true;
				}
				if (!this._isValidateScaleError && !this._isScaleIllegalChar)
				{
					this.bdrScale.BorderBrush = Brushes.Transparent;
					this.txtScale.ClearValue(FrameworkElement.ToolTipProperty);
				}
				if (!this._isValidatePressError && !this._isPressIllegalChar)
				{
					this.bdrPressure.BorderBrush = Brushes.Transparent;
					this.SetDefaultToolTip();
				}
				AnalyticalColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(!this._isValidatePressError && !this._isValidateScaleError);
			}), Array.Empty<object>());
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00005D60 File Offset: 0x00003F60
		private void ResetErrorConditions()
		{
			if (!this._isScaleIllegalChar && this.txtScale != null)
			{
				this.bdrScale.BorderBrush = Brushes.Transparent;
				this.txtScale.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!this._isPressIllegalChar && this.txtPressure != null)
			{
				this.bdrPressure.BorderBrush = Brushes.Transparent;
				this.SetDefaultToolTip();
			}
			AnalyticalColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isPressIllegalChar && !this._isScaleIllegalChar);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00005DE8 File Offset: 0x00003FE8
		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (e.Action == ValidationErrorEventAction.Added)
			{
				if (textBox == this.txtPressure)
				{
					this._isPressIllegalChar = true;
				}
				else
				{
					this._isScaleIllegalChar = true;
				}
			}
			else if (textBox == this.txtPressure)
			{
				this._isPressIllegalChar = false;
			}
			else
			{
				this._isScaleIllegalChar = false;
			}
			AnalyticalColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isPressIllegalChar && !this._isScaleIllegalChar);
		}

		// Token: 0x04000063 RID: 99
		private readonly bool _isPressurePSI;

		// Token: 0x04000064 RID: 100
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x04000065 RID: 101
		private bool _isValidateError;

		// Token: 0x04000066 RID: 102
		private bool _isValidatePressError;

		// Token: 0x04000067 RID: 103
		private bool _isValidateScaleError;

		// Token: 0x04000068 RID: 104
		private bool _isPressIllegalChar;

		// Token: 0x04000069 RID: 105
		private bool _isScaleIllegalChar;

		// Token: 0x0400006D RID: 109
		private ExperimentInfo _experiment;

		// Token: 0x0400006E RID: 110
		private string _pressureTolTip;

		// Token: 0x020000A2 RID: 162
		// (Invoke) Token: 0x060006AB RID: 1707
		public delegate void ValidationUpdate(bool isValid);

		// Token: 0x020000A3 RID: 163
		// (Invoke) Token: 0x060006AF RID: 1711
		public delegate void ModificationUpdate(bool isModified);
	}
}
