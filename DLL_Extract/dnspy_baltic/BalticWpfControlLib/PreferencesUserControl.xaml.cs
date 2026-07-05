using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib
{
	// Token: 0x0200002E RID: 46
	public partial class PreferencesUserControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x14000030 RID: 48
		// (add) Token: 0x060002A4 RID: 676 RVA: 0x00012D60 File Offset: 0x00010F60
		// (remove) Token: 0x060002A5 RID: 677 RVA: 0x00012D98 File Offset: 0x00010F98
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060002A6 RID: 678 RVA: 0x00012DCD File Offset: 0x00010FCD
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060002A7 RID: 679 RVA: 0x00012DE6 File Offset: 0x00010FE6
		// (set) Token: 0x060002A8 RID: 680 RVA: 0x00012DF8 File Offset: 0x00010FF8
		public bool IsSkipVialAndContinue
		{
			get
			{
				return this._preferences.Autosampler.IsSkipVialAndContinue;
			}
			set
			{
				this._preferences.Autosampler.IsSkipVialAndContinue = value;
				this.NotifyPropertyChanged("IsSkipVialAndContinue");
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060002A9 RID: 681 RVA: 0x00012E16 File Offset: 0x00011016
		// (set) Token: 0x060002AA RID: 682 RVA: 0x00012E28 File Offset: 0x00011028
		public bool IsViaTrap
		{
			get
			{
				return this._preferences.Pump.IsViaTrap;
			}
			set
			{
				this._preferences.Pump.IsViaTrap = value;
				this.NotifyPropertyChanged("IsViaTrap");
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060002AB RID: 683 RVA: 0x00012E46 File Offset: 0x00011046
		// (set) Token: 0x060002AC RID: 684 RVA: 0x00012E58 File Offset: 0x00011058
		public bool IsIdleFlowOnError
		{
			get
			{
				return this._preferences.Pump.IsIdleFlowOnError;
			}
			set
			{
				this._preferences.Pump.IsIdleFlowOnError = value;
				this.NotifyPropertyChanged("IsIdleFlowOnError");
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060002AD RID: 685 RVA: 0x00012E76 File Offset: 0x00011076
		// (set) Token: 0x060002AE RID: 686 RVA: 0x00012E88 File Offset: 0x00011088
		public bool IsIdleFlowOnStandby
		{
			get
			{
				return this._preferences.Pump.IsIdleFlowOnStandby;
			}
			set
			{
				this._preferences.Pump.IsIdleFlowOnStandby = value;
				this.NotifyPropertyChanged("IsIdleFlowOnStandby");
			}
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060002AF RID: 687 RVA: 0x00012EA6 File Offset: 0x000110A6
		// (set) Token: 0x060002B0 RID: 688 RVA: 0x00012EB8 File Offset: 0x000110B8
		public double StandbyFlow
		{
			get
			{
				return this._preferences.Pump.StandbyFlow;
			}
			set
			{
				this._preferences.Pump.StandbyFlow = value;
				this.NotifyPropertyChanged("StandbyFlow");
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060002B1 RID: 689 RVA: 0x00012ED6 File Offset: 0x000110D6
		// (set) Token: 0x060002B2 RID: 690 RVA: 0x00012EE8 File Offset: 0x000110E8
		public int Composition
		{
			get
			{
				return this._preferences.Pump.Composition;
			}
			set
			{
				this._preferences.Pump.Composition = value;
				this.NotifyPropertyChanged("Composition");
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060002B3 RID: 691 RVA: 0x00012F06 File Offset: 0x00011106
		// (set) Token: 0x060002B4 RID: 692 RVA: 0x00012F18 File Offset: 0x00011118
		public double OvenTemperature
		{
			get
			{
				return this._preferences.Oven.TemperatureSetPt;
			}
			set
			{
				this._preferences.Oven.TemperatureSetPt = value;
				this.NotifyPropertyChanged("OvenTemperature");
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060002B5 RID: 693 RVA: 0x00012F36 File Offset: 0x00011136
		// (set) Token: 0x060002B6 RID: 694 RVA: 0x00012F43 File Offset: 0x00011143
		public bool IsSelfDiagnosticsEnabled
		{
			get
			{
				return this._preferences.IsSelfDiagnosticsEnabled;
			}
			set
			{
				this._preferences.IsSelfDiagnosticsEnabled = value;
				this.NotifyPropertyChanged("IsSelfDiagnosticsEnabled");
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x00012F5C File Offset: 0x0001115C
		// (set) Token: 0x060002B8 RID: 696 RVA: 0x00012F69 File Offset: 0x00011169
		public bool IsExtendedLoggingEnabled
		{
			get
			{
				return this._preferences.IsExtendedLoggingEnabled;
			}
			set
			{
				this._preferences.IsExtendedLoggingEnabled = value;
				this.NotifyPropertyChanged("IsExtendedLoggingEnabled");
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x00012F82 File Offset: 0x00011182
		// (set) Token: 0x060002BA RID: 698 RVA: 0x00012F94 File Offset: 0x00011194
		public bool IsPumpFirmwareLoggingEnabled
		{
			get
			{
				return this._preferences.Pump.IsFirmwareLoggingEnabled;
			}
			set
			{
				this._preferences.Pump.IsFirmwareLoggingEnabled = value;
				this.NotifyPropertyChanged("IsPumpFirmwareLoggingEnabled");
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060002BB RID: 699 RVA: 0x00012FB2 File Offset: 0x000111B2
		// (set) Token: 0x060002BC RID: 700 RVA: 0x00012FBF File Offset: 0x000111BF
		public LedBrightness LedBrightness
		{
			get
			{
				return this._preferences.LedBrightness;
			}
			set
			{
				this._preferences.LedBrightness = value;
				this.NotifyPropertyChanged("LedBrightness");
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060002BD RID: 701 RVA: 0x00012FD8 File Offset: 0x000111D8
		public List<LedBrightness> LedBrightnessValues { get; } = new List<LedBrightness>(3)
		{
			LedBrightness.Off,
			LedBrightness.Intermediate,
			LedBrightness.Full
		};

		// Token: 0x14000031 RID: 49
		// (add) Token: 0x060002BE RID: 702 RVA: 0x00012FE0 File Offset: 0x000111E0
		// (remove) Token: 0x060002BF RID: 703 RVA: 0x00013018 File Offset: 0x00011218
		public event PreferencesUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x060002C0 RID: 704 RVA: 0x00013050 File Offset: 0x00011250
		public PreferencesUserControl(BalticPreferences preferences, ExperimentInfo experiment, BalticInstrumentFacade instrument)
		{
			this.InitializeComponent();
			this._preferences = preferences;
			this._instrument = instrument;
			this._experiment = experiment;
			base.DataContext = this;
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x000130A6 File Offset: 0x000112A6
		private void txtStandby_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x000130AE File Offset: 0x000112AE
		private void StandbyFlowDoubleTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				this.StandbyFlow -= 0.1;
			}
			else
			{
				this.StandbyFlow += 0.1;
			}
			this.ValidateParameters();
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x000130A6 File Offset: 0x000112A6
		private void CompIntegerTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.ValidateParameters();
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x000130F0 File Offset: 0x000112F0
		private void CompIntegerTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				if (this.Composition - 1 >= 0)
				{
					this.Composition--;
				}
			}
			else if (this.Composition + 1 <= 100)
			{
				this.Composition++;
			}
			this.ValidateParameters();
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x000130A6 File Offset: 0x000112A6
		private void txtTemp_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x00013141 File Offset: 0x00011341
		private void OvenTempSetPtDoubleTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				this.OvenTemperature -= 0.1;
			}
			else
			{
				this.OvenTemperature += 0.1;
			}
			this.ValidateParameters();
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00013180 File Offset: 0x00011380
		private void btnRevert_Click(object sender, RoutedEventArgs e)
		{
			ProcedureArguments arguments = this._instrument.GenerateIdleArguments(this._experiment);
			BalticPreferences defaultPref = new BalticPreferences();
			this.IsSkipVialAndContinue = defaultPref.Autosampler.IsSkipVialAndContinue;
			this.IsViaTrap = (bool)arguments["via_trap"].Value;
			this.IsIdleFlowOnError = defaultPref.Pump.IsIdleFlowOnError;
			this.IsIdleFlowOnStandby = defaultPref.Pump.IsIdleFlowOnStandby;
			this.IsSelfDiagnosticsEnabled = defaultPref.IsSelfDiagnosticsEnabled;
			this.IsExtendedLoggingEnabled = defaultPref.IsExtendedLoggingEnabled;
			this.StandbyFlow = (double)arguments["idle_flow_rate"].Value;
			this.Composition = (int)((double)arguments["composition"].Value);
			this.OvenTemperature = (double)arguments["default_oven_temperature"].Value;
			this.LedBrightness = defaultPref.LedBrightness;
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0001326C File Offset: 0x0001146C
		private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
		{
			if (Severity.Error.Equals(e.Severity))
			{
				this._isValidateError = true;
				this.SetErrorCondition(e.Subject, e.Message);
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x000132B0 File Offset: 0x000114B0
		private void ValidateParameters()
		{
			if (this._instrument == null)
			{
				return;
			}
			ProcedureArguments arguments = this._instrument.CreateIdleArguments();
			this._isValidateError = (this._isValidateStandbyFlowError = (this._isValidateCompError = (this._isValidateOvenTempError = false)));
			arguments["composition"].Value = this.Composition;
			arguments["idle_flow_rate"].Value = this.StandbyFlow;
			arguments["default_oven_temperature"].Value = this.OvenTemperature;
			arguments["default_oven_temperature"].Value = this.OvenTemperature;
			arguments["via_trap"].Value = this.IsViaTrap;
			this._instrument.ValidationMessageReported += this.ValidationErrorHandler;
			try
			{
				this._instrument.ValidateMethodProcedureOffLine(this._experiment, this._instrument.GetIdleProcedure(), arguments, new ChildProcedureArguments());
			}
			finally
			{
				this._instrument.ValidationMessageReported -= this.ValidationErrorHandler;
			}
			if (!this._isValidateError)
			{
				this.ResetErrorConditions();
			}
		}

		// Token: 0x060002CA RID: 714 RVA: 0x000133EC File Offset: 0x000115EC
		private void SetErrorCondition(string subject, string message)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (subject == "composition")
				{
					this.txtComposition.BorderBrush = Brushes.Red;
					this.txtComposition.BorderThickness = new Thickness(2.0);
					this.txtComposition.ToolTip = message;
					this._isValidateCompError = true;
				}
				if (subject == "idle_flow_rate")
				{
					this.txtStandby.BorderBrush = Brushes.Red;
					this.txtStandby.BorderThickness = new Thickness(2.0);
					this.txtStandby.ToolTip = message;
					this._isValidateStandbyFlowError = true;
				}
				if (subject == "default_oven_temperature")
				{
					this.txtTemp.BorderBrush = Brushes.Red;
					this.txtTemp.BorderThickness = new Thickness(2.0);
					this.txtTemp.ToolTip = message;
					this._isValidateOvenTempError = true;
				}
				if (!this._isValidateCompError)
				{
					this.txtComposition.ClearValue(Control.BorderBrushProperty);
					this.txtComposition.ClearValue(Control.BorderThicknessProperty);
					this.txtComposition.ClearValue(FrameworkElement.ToolTipProperty);
				}
				if (!this._isValidateStandbyFlowError && !this._isStandbyFlowIllegalChar)
				{
					this.txtStandby.ClearValue(Control.BorderBrushProperty);
					this.txtStandby.ClearValue(Control.BorderThicknessProperty);
					this.txtStandby.ClearValue(FrameworkElement.ToolTipProperty);
				}
				if (!this._isValidateOvenTempError && !this._isOvenTempIllegalChar)
				{
					this.txtTemp.ClearValue(Control.BorderBrushProperty);
					this.txtTemp.ClearValue(Control.BorderThicknessProperty);
					this.txtTemp.ClearValue(FrameworkElement.ToolTipProperty);
				}
				PreferencesUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(!this._isValidateCompError && !this._isValidateStandbyFlowError && !this._isStandbyFlowIllegalChar && !this._isValidateOvenTempError && !this._isOvenTempIllegalChar);
			}), Array.Empty<object>());
		}

		// Token: 0x060002CB RID: 715 RVA: 0x00013434 File Offset: 0x00011634
		private void ResetErrorConditions()
		{
			if (!this._isStandbyFlowIllegalChar && !this._isStandbyFlowIllegalChar)
			{
				this.txtStandby.ClearValue(Control.BorderBrushProperty);
				this.txtStandby.ClearValue(Control.BorderThicknessProperty);
				this.txtStandby.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!this._isValidateCompError)
			{
				this.txtComposition.ClearValue(Control.BorderBrushProperty);
				this.txtComposition.ClearValue(Control.BorderThicknessProperty);
				this.txtComposition.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!this._isOvenTempIllegalChar && !this._isValidateOvenTempError)
			{
				this.txtTemp.ClearValue(Control.BorderBrushProperty);
				this.txtTemp.ClearValue(Control.BorderThicknessProperty);
				this.txtTemp.ClearValue(FrameworkElement.ToolTipProperty);
			}
			PreferencesUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidateCompError && !this._isValidateStandbyFlowError && !this._isStandbyFlowIllegalChar && !this._isValidateOvenTempError && !this._isOvenTempIllegalChar);
		}

		// Token: 0x060002CC RID: 716 RVA: 0x00013538 File Offset: 0x00011738
		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (e.Action == ValidationErrorEventAction.Added)
			{
				if (textBox == this.txtStandby)
				{
					this._isStandbyFlowIllegalChar = true;
				}
				else if (textBox == this.txtTemp)
				{
					this._isOvenTempIllegalChar = true;
				}
			}
			else if (textBox == this.txtStandby)
			{
				this._isStandbyFlowIllegalChar = false;
			}
			else if (textBox == this.txtTemp)
			{
				this._isOvenTempIllegalChar = false;
			}
			PreferencesUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidateCompError && !this._isValidateStandbyFlowError && !this._isStandbyFlowIllegalChar && !this._isValidateOvenTempError && !this._isOvenTempIllegalChar);
		}

		// Token: 0x060002CD RID: 717 RVA: 0x000130A6 File Offset: 0x000112A6
		private void ViaTrapCheckBox_Click(object sender, RoutedEventArgs e)
		{
			this.ValidateParameters();
		}

		// Token: 0x040001AF RID: 431
		private readonly BalticPreferences _preferences;

		// Token: 0x040001B0 RID: 432
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x040001B1 RID: 433
		private readonly ExperimentInfo _experiment;

		// Token: 0x040001B2 RID: 434
		private bool _isValidateError;

		// Token: 0x040001B3 RID: 435
		private bool _isValidateStandbyFlowError;

		// Token: 0x040001B4 RID: 436
		private bool _isValidateCompError;

		// Token: 0x040001B5 RID: 437
		private bool _isStandbyFlowIllegalChar;

		// Token: 0x040001B6 RID: 438
		private const bool _isCompIllegalChar = false;

		// Token: 0x040001B7 RID: 439
		private bool _isValidateOvenTempError;

		// Token: 0x040001B8 RID: 440
		private bool _isOvenTempIllegalChar;

		// Token: 0x020000F7 RID: 247
		// (Invoke) Token: 0x0600079F RID: 1951
		public delegate void ValidationUpdate(bool isValid);
	}
}
