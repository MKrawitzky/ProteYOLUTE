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
	// Token: 0x02000042 RID: 66
	public partial class TrapColEquilUserControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x1400003F RID: 63
		// (add) Token: 0x060003BC RID: 956 RVA: 0x00017E44 File Offset: 0x00016044
		// (remove) Token: 0x060003BD RID: 957 RVA: 0x00017E7C File Offset: 0x0001607C
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060003BE RID: 958 RVA: 0x00017EB1 File Offset: 0x000160B1
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060003BF RID: 959 RVA: 0x00017ECA File Offset: 0x000160CA
		// (set) Token: 0x060003C0 RID: 960 RVA: 0x00017EDC File Offset: 0x000160DC
		public double Scale
		{
			get
			{
				return this.Method.TrapColumnEquil.Scale;
			}
			set
			{
				this.Method.TrapColumnEquil.Scale = value;
				this.NotifyPropertyChanged("Scale");
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060003C1 RID: 961 RVA: 0x00017EFA File Offset: 0x000160FA
		// (set) Token: 0x060003C2 RID: 962 RVA: 0x00017F2F File Offset: 0x0001612F
		public double Pressure
		{
			get
			{
				if (!this._isPressurePSI)
				{
					return this.Method.TrapColumnEquil.Pressure;
				}
				return this.Method.TrapColumnEquil.Pressure / 0.0689475729;
			}
			set
			{
				this.Method.TrapColumnEquil.Pressure = (this._isPressurePSI ? (value * 0.0689475729) : value);
				this.NotifyPropertyChanged("Pressure");
			}
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x060003C3 RID: 963 RVA: 0x00017F62 File Offset: 0x00016162
		public double ColumnVolume
		{
			get
			{
				return this.Method.TrapColumnVolume;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060003C4 RID: 964 RVA: 0x00017F6F File Offset: 0x0001616F
		// (set) Token: 0x060003C5 RID: 965 RVA: 0x00017F77 File Offset: 0x00016177
		public BindableBalticMethod Method { get; private set; }

		// Token: 0x14000040 RID: 64
		// (add) Token: 0x060003C6 RID: 966 RVA: 0x00017F80 File Offset: 0x00016180
		// (remove) Token: 0x060003C7 RID: 967 RVA: 0x00017FB8 File Offset: 0x000161B8
		public event TrapColEquilUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x14000041 RID: 65
		// (add) Token: 0x060003C8 RID: 968 RVA: 0x00017FF0 File Offset: 0x000161F0
		// (remove) Token: 0x060003C9 RID: 969 RVA: 0x00018028 File Offset: 0x00016228
		public event TrapColEquilUserControl.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060003CA RID: 970 RVA: 0x0001805D File Offset: 0x0001625D
		// (set) Token: 0x060003CB RID: 971 RVA: 0x00018065 File Offset: 0x00016265
		public ExperimentInfo Experiment { get; set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060003CC RID: 972 RVA: 0x0001806E File Offset: 0x0001626E
		// (set) Token: 0x060003CD RID: 973 RVA: 0x00018076 File Offset: 0x00016276
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

		// Token: 0x060003CE RID: 974 RVA: 0x0001808C File Offset: 0x0001628C
		public TrapColEquilUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, bool isPressurePSI, ExperimentInfo experiment)
		{
			this.Method = method;
			this._instrument = instrument;
			this._isPressurePSI = isPressurePSI;
			this.Experiment = experiment;
			this.InitializeComponent();
			base.DataContext = this;
			this.lblPressUnits.Content = (this._isPressurePSI ? "PSI" : "bar");
			this.SetDefaultToolTip();
		}

		// Token: 0x060003CF RID: 975 RVA: 0x000180F0 File Offset: 0x000162F0
		private void SetDefaultToolTip()
		{
			try
			{
				ExperimentInfo experiment = this.Experiment;
				if (experiment != null && experiment.Trap != null)
				{
					this.PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this.Experiment.Trap.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0), this._isPressurePSI ? "PSI" : "bar");
				}
				else
				{
					this.PressureToolTip = string.Format("Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this.Experiment.Trap.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0), this._isPressurePSI ? "PSI" : "bar");
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00018244 File Offset: 0x00016444
		public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
		{
			this.Experiment = experiment;
			this.Method = method;
			this.Scale = this.Method.TrapColumnEquil.Scale;
			this.Pressure = this.Method.TrapColumnEquil.Pressure / (this._isPressurePSI ? 0.0689475729 : 1.0);
			TrapColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent != null)
			{
				modificationUpdateEvent(this.IsModified());
			}
			this.RevertToDefault();
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x000182C8 File Offset: 0x000164C8
		private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
		{
			this.Method.TrapColumnEquil.RevertToDefault();
			this.Scale = this.Method.TrapColumnEquil.Scale;
			this.Pressure = this.Method.TrapColumnEquil.Pressure / (this._isPressurePSI ? 0.0689475729 : 1.0);
			TrapColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent != null)
			{
				modificationUpdateEvent(this.IsModified());
			}
			this.RevertToDefault();
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0001834C File Offset: 0x0001654C
		private bool IsModified()
		{
			return (int)(this.Scale * 10.0) != (int)(this.Method.TrapColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.TrapColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0));
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x000183D7 File Offset: 0x000165D7
		private void RevertToDefault()
		{
			this._isPressIllegalChar = false;
			this._isScaleIllegalChar = false;
			TrapColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(true);
			}
			this.ResetErrorConditions();
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x000183FF File Offset: 0x000165FF
		private void txtScale_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				this.Scale -= 1.0;
				return;
			}
			this.Scale += 1.0;
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00018438 File Offset: 0x00016638
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

		// Token: 0x060003D6 RID: 982 RVA: 0x000184D8 File Offset: 0x000166D8
		private void txtPressure_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			TrapColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this.Method.TrapColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.TrapColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)));
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0001857C File Offset: 0x0001677C
		private void txtScale_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			TrapColEquilUserControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this.Method.TrapColumnEquil.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this.Method.TrapColumnEquil.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)));
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x00018620 File Offset: 0x00016820
		private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
		{
			if (Severity.Error.Equals(e.Severity))
			{
				this._isValidateError = true;
				this.SetErrorCondition(e.Subject, e.Message);
			}
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x00018664 File Offset: 0x00016864
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
				if (this.Experiment != null)
				{
					this._instrument.ValidateMethodProcedureOffLine(this.Experiment, procedure, arguments, advChildArguments);
					if (arguments.Contains("trap_equil_time"))
					{
						this.Method.TrapColumnEquil.EquilTime = (double)arguments["trap_equil_time"].Value;
					}
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

		// Token: 0x060003DA RID: 986 RVA: 0x00018808 File Offset: 0x00016A08
		private void SetErrorCondition(string subject, string message)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (subject == "trap_equilibration_pressure")
				{
					string tempMsg = message;
					if (this._isPressurePSI && message.ToLower().Contains("value must be between"))
					{
						double minPress = 50.0;
						double maxPress = this.Experiment.Trap.MaximumPressure;
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
				else if (subject == "trap_equilibration_volumemultiplier")
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
				TrapColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(!this._isValidatePressError && !this._isValidateScaleError);
			}), Array.Empty<object>());
		}

		// Token: 0x060003DB RID: 987 RVA: 0x00018850 File Offset: 0x00016A50
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
				this.txtPressure.ClearValue(Control.BorderThicknessProperty);
				this.SetDefaultToolTip();
			}
			TrapColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isPressIllegalChar && !this._isScaleIllegalChar);
		}

		// Token: 0x060003DC RID: 988 RVA: 0x000188E8 File Offset: 0x00016AE8
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
			TrapColEquilUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isPressIllegalChar && !this._isScaleIllegalChar);
		}

		// Token: 0x0400024C RID: 588
		private readonly bool _isPressurePSI;

		// Token: 0x0400024D RID: 589
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x0400024E RID: 590
		private bool _isValidateError;

		// Token: 0x0400024F RID: 591
		private bool _isValidatePressError;

		// Token: 0x04000250 RID: 592
		private bool _isValidateScaleError;

		// Token: 0x04000251 RID: 593
		private bool _isPressIllegalChar;

		// Token: 0x04000252 RID: 594
		private bool _isScaleIllegalChar;

		// Token: 0x04000257 RID: 599
		private string _pressureTolTip;

		// Token: 0x0200010F RID: 271
		// (Invoke) Token: 0x060007E1 RID: 2017
		public delegate void ValidationUpdate(bool isValid);

		// Token: 0x02000110 RID: 272
		// (Invoke) Token: 0x060007E5 RID: 2021
		public delegate void ModificationUpdate(bool isModified);
	}
}
