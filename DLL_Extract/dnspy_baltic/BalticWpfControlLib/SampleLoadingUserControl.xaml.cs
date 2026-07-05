using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
	// Token: 0x02000039 RID: 57
	public partial class SampleLoadingUserControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x14000034 RID: 52
		// (add) Token: 0x06000320 RID: 800 RVA: 0x000147A0 File Offset: 0x000129A0
		// (remove) Token: 0x06000321 RID: 801 RVA: 0x000147D8 File Offset: 0x000129D8
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x06000322 RID: 802 RVA: 0x0001480D File Offset: 0x00012A0D
		private void NotifyPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000323 RID: 803 RVA: 0x00014829 File Offset: 0x00012A29
		// (set) Token: 0x06000324 RID: 804 RVA: 0x0001483B File Offset: 0x00012A3B
		public double Scale
		{
			get
			{
				return this._method.SampleLoading.Scale;
			}
			set
			{
				this._method.SampleLoading.Scale = value;
				this.NotifyPropertyChanged("Scale");
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000325 RID: 805 RVA: 0x00014859 File Offset: 0x00012A59
		// (set) Token: 0x06000326 RID: 806 RVA: 0x0001488E File Offset: 0x00012A8E
		public double Pressure
		{
			get
			{
				if (!this._isPressurePSI)
				{
					return this._method.SampleLoading.Pressure;
				}
				return this._method.SampleLoading.Pressure / 0.0689475729;
			}
			set
			{
				this._method.SampleLoading.Pressure = (this._isPressurePSI ? (value * 0.0689475729) : value);
				this.NotifyPropertyChanged("Pressure");
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000327 RID: 807 RVA: 0x000148C1 File Offset: 0x00012AC1
		public BindableBalticMethod Method
		{
			get
			{
				return this._method;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000328 RID: 808 RVA: 0x000148C9 File Offset: 0x00012AC9
		// (set) Token: 0x06000329 RID: 809 RVA: 0x000148DB File Offset: 0x00012ADB
		public double PenetrationDepth
		{
			get
			{
				return this._method.SampleLoading.PenetrationDepth;
			}
			set
			{
				this._method.SampleLoading.PenetrationDepth = value;
				this.NotifyPropertyChanged("PenetrationDepth");
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600032A RID: 810 RVA: 0x000148F9 File Offset: 0x00012AF9
		public double ColumnVolume
		{
			get
			{
				return this._method.SeparationColumnVolume;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600032B RID: 811 RVA: 0x00014906 File Offset: 0x00012B06
		// (set) Token: 0x0600032C RID: 812 RVA: 0x00014918 File Offset: 0x00012B18
		public bool IsBottomSense
		{
			get
			{
				return this._method.SampleLoading.IsBottomSense;
			}
			set
			{
				this._method.SampleLoading.IsBottomSense = value;
				this.NotifyPropertyChanged("IsBottomSense");
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x0600032D RID: 813 RVA: 0x00014936 File Offset: 0x00012B36
		// (set) Token: 0x0600032E RID: 814 RVA: 0x00014948 File Offset: 0x00012B48
		public BalticInjectionType InjectionMethod
		{
			get
			{
				return this._method.SampleLoading.InjectionMethod;
			}
			set
			{
				this._method.SampleLoading.InjectionMethod = value;
				this.NotifyPropertyChanged("InjectionMethod");
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x0600032F RID: 815 RVA: 0x00014966 File Offset: 0x00012B66
		// (set) Token: 0x06000330 RID: 816 RVA: 0x0001496E File Offset: 0x00012B6E
		public List<SampleLoadingUserControl.ComboBoxItemInjMethod> InjMethodListEnum
		{
			get
			{
				return this._injMethodListEnum;
			}
			set
			{
				this._injMethodListEnum = value;
				this.NotifyPropertyChanged("InjMethodListEnum");
			}
		}

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x06000331 RID: 817 RVA: 0x00014984 File Offset: 0x00012B84
		// (remove) Token: 0x06000332 RID: 818 RVA: 0x000149BC File Offset: 0x00012BBC
		public event SampleLoadingUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x14000036 RID: 54
		// (add) Token: 0x06000333 RID: 819 RVA: 0x000149F4 File Offset: 0x00012BF4
		// (remove) Token: 0x06000334 RID: 820 RVA: 0x00014A2C File Offset: 0x00012C2C
		public event SampleLoadingUserControl.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000335 RID: 821 RVA: 0x00014A61 File Offset: 0x00012C61
		// (set) Token: 0x06000336 RID: 822 RVA: 0x00014A69 File Offset: 0x00012C69
		public ExperimentInfo Experiment
		{
			get
			{
				return this._experiment;
			}
			set
			{
				this._experiment = value;
			}
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000337 RID: 823 RVA: 0x00014A72 File Offset: 0x00012C72
		// (set) Token: 0x06000338 RID: 824 RVA: 0x00014A7A File Offset: 0x00012C7A
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

		// Token: 0x06000339 RID: 825 RVA: 0x00014A90 File Offset: 0x00012C90
		public SampleLoadingUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
		{
			this.InitializeComponent();
			this._method = method;
			this._instrument = instrument;
			this._experiment = experiment;
			base.DataContext = this;
			this.SetDefaultToolTip();
		}

		// Token: 0x0600033A RID: 826 RVA: 0x00014B10 File Offset: 0x00012D10
		public SampleLoadingUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, bool ispressurePSI, ExperimentInfo experiment)
		{
			this._method = method;
			this._instrument = instrument;
			this._isPressurePSI = ispressurePSI;
			this._experiment = experiment;
			this.InitializeComponent();
			base.DataContext = this;
			this.lblPressUnits.Content = (this._isPressurePSI ? "PSI" : "bar");
			this.SetDefaultToolTip();
		}

		// Token: 0x0600033B RID: 827 RVA: 0x00014BB8 File Offset: 0x00012DB8
		private void SetDefaultToolTip()
		{
			try
			{
				if (this._experiment != null)
				{
					this.PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this._method.UsesTrapColumn ? (this._experiment.Trap.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0)) : (this._experiment.Separator.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0)), this._isPressurePSI ? "PSI" : "bar");
				}
				else
				{
					this.PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (this._isPressurePSI ? 0.0689475729 : 1.0), this._experiment.Trap.MaximumPressure / (this._isPressurePSI ? 0.0689475729 : 1.0), this._isPressurePSI ? "PSI" : "bar");
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600033C RID: 828 RVA: 0x00014D40 File Offset: 0x00012F40
		public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
		{
			this._experiment = experiment;
			this._method = method;
			this.Pressure = this._method.SampleLoading.Pressure / (this._isPressurePSI ? 0.0689475729 : 1.0);
			this.Scale = this._method.SampleLoading.Scale;
			this.IsBottomSense = this._method.SampleLoading.IsBottomSense;
			this.InjectionMethod = this._method.SampleLoading.InjectionMethod;
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod);
			}
			this._isPressIllegalChar = false;
			this._isScaleIllegalChar = false;
			this._isPenetrateIllegalChar = false;
			SampleLoadingUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(true);
			}
			this.ResetErrorConditions();
			this.SetDefaultToolTip();
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00014ECC File Offset: 0x000130CC
		private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
		{
			this._method.SampleLoading.RevertToDefault();
			this.Pressure = this._method.SampleLoading.Pressure / (this._isPressurePSI ? 0.0689475729 : 1.0);
			this.Scale = this._method.SampleLoading.Scale;
			this.IsBottomSense = this._method.SampleLoading.IsBottomSense;
			this.InjectionMethod = this._method.SampleLoading.InjectionMethod;
			this.PenetrationDepth = this._method.SampleLoading.PenetrationDepth;
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod);
			}
			this._isPressIllegalChar = false;
			this._isScaleIllegalChar = false;
			this._isPenetrateIllegalChar = false;
			SampleLoadingUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(true);
			}
			this.ResetErrorConditions();
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00015069 File Offset: 0x00013269
		private void txtFactor_MouseWheel(object sender, MouseWheelEventArgs e)
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

		// Token: 0x0600033F RID: 831 RVA: 0x000150A8 File Offset: 0x000132A8
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

		// Token: 0x06000340 RID: 832 RVA: 0x00015145 File Offset: 0x00013345
		private void txtPenetrateDepth_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				this.PenetrationDepth -= 0.1;
			}
			else
			{
				this.PenetrationDepth += 0.1;
			}
			this.ValidateParameters();
		}

		// Token: 0x06000341 RID: 833 RVA: 0x00015184 File Offset: 0x00013384
		private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
		{
			if (Severity.Error.Equals(e.Severity))
			{
				this._isValidateError = true;
				this.SetErrorCondition(e.Subject, e.Message);
			}
		}

		// Token: 0x06000342 RID: 834 RVA: 0x000151C8 File Offset: 0x000133C8
		public void ValidateParameters()
		{
			if (this._instrument == null)
			{
				return;
			}
			this.ResetErrorIndicators();
			BalticMethod method = this._method.ToBalticMethod(null);
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
				this._isValidateError = (this._isValidateScaleError = (this._isValidatePressError = (this._isValidatePenetrateError = false)));
				if (this._experiment != null)
				{
					this._instrument.ValidateMethodProcedureOffLine(this._experiment, procedure, arguments, advChildArguments);
					if (arguments.Contains("column_load_time"))
					{
						this.Method.SampleLoading.EquilTime = (double)arguments["column_load_time"].Value;
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

		// Token: 0x06000343 RID: 835 RVA: 0x0001534C File Offset: 0x0001354C
		private void SetErrorCondition(string subject, string message)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (subject == "column_load_pressure")
				{
					this.bdrPressure.BorderBrush = Brushes.Red;
					this.PressureToolTip = message;
					this._isValidatePressError = true;
				}
				else if (subject == "column_load_volumemultiplier")
				{
					this.bdrScale.BorderBrush = Brushes.Red;
					this.txtScale.ToolTip = message;
					this._isValidateScaleError = true;
				}
				else if (subject == "penetration_depth")
				{
					this.bdrPenetrate.BorderBrush = Brushes.Red;
					this.txtPenetrateDepth.ToolTip = message;
					this._isValidatePenetrateError = true;
				}
				if (!this._isValidateScaleError && !this._isScaleIllegalChar)
				{
					this.bdrScale.BorderBrush = Brushes.Transparent;
					this.txtScale.ClearValue(FrameworkElement.ToolTipProperty);
				}
				if (!this._isValidatePressError && !this._isPressIllegalChar)
				{
					this.txtPressure.BorderBrush = Brushes.Transparent;
					this.SetDefaultToolTip();
				}
				if (!this._isValidatePenetrateError && !this._isPenetrateIllegalChar)
				{
					this.txtPenetrateDepth.BorderBrush = Brushes.Transparent;
					this.SetDefaultToolTip();
				}
				SampleLoadingUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(!this._isValidatePressError && !this._isValidateScaleError && !this._isValidatePenetrateError);
			}), Array.Empty<object>());
		}

		// Token: 0x06000344 RID: 836 RVA: 0x00015394 File Offset: 0x00013594
		private void ResetErrorIndicators()
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
			if (!this._isPenetrateIllegalChar && this.txtPenetrateDepth != null)
			{
				this.bdrPenetrate.BorderBrush = Brushes.Transparent;
				this.SetDefaultToolTip();
			}
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0001541D File Offset: 0x0001361D
		private void ResetErrorConditions()
		{
			this.ResetErrorIndicators();
			SampleLoadingUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidatePressError && !this._isValidateScaleError && !this._isValidatePenetrateError);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x00015454 File Offset: 0x00013654
		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (e.Action == ValidationErrorEventAction.Added)
			{
				if (textBox == this.txtPressure)
				{
					this._isPressIllegalChar = true;
				}
				else if (textBox == this.txtScale)
				{
					this._isScaleIllegalChar = true;
				}
				else
				{
					this._isPenetrateIllegalChar = true;
				}
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
			else if (textBox == this.txtScale)
			{
				this._isScaleIllegalChar = false;
			}
			else
			{
				this._isPenetrateIllegalChar = false;
			}
			SampleLoadingUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidatePressError && !this._isValidateScaleError && !this._isValidatePenetrateError);
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00015510 File Offset: 0x00013710
		private void chkBottomSense_Click(object sender, RoutedEventArgs e)
		{
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod || (int)(this.PenetrationDepth * 10.0) != (int)(this._method.SampleLoading.DefaultPenetrationDepth * 10.0));
			}
		}

		// Token: 0x06000348 RID: 840 RVA: 0x00015614 File Offset: 0x00013814
		private void comboInjeMethod_Selchanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod || (int)(this.PenetrationDepth * 10.0) != (int)(this._method.SampleLoading.DefaultPenetrationDepth * 10.0));
			}
		}

		// Token: 0x06000349 RID: 841 RVA: 0x00015718 File Offset: 0x00013918
		private void txtScale_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod || (int)(this.PenetrationDepth * 10.0) != (int)(this._method.SampleLoading.DefaultPenetrationDepth * 10.0));
			}
		}

		// Token: 0x0600034A RID: 842 RVA: 0x00015824 File Offset: 0x00013A24
		private void txtPressure_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod || (int)(this.PenetrationDepth * 10.0) != (int)(this._method.SampleLoading.DefaultPenetrationDepth * 10.0));
			}
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00015930 File Offset: 0x00013B30
		private void txtPenetrateDepth_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			if (this.ModificationUpdateEvent != null)
			{
				this.ModificationUpdateEvent((int)(this.Scale * 10.0) != (int)(this._method.SampleLoading.DefaultScale * 10.0) || (int)(this.Pressure * 10.0) != (int)(this._method.SampleLoading.DefaultPressure * 10.0 / (this._isPressurePSI ? 0.0689475729 : 1.0)) || this.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense || this.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod || (int)(this.PenetrationDepth * 10.0) != (int)(this._method.SampleLoading.DefaultPenetrationDepth * 10.0));
			}
		}

		// Token: 0x040001DD RID: 477
		private bool _isPressurePSI;

		// Token: 0x040001DE RID: 478
		private bool _isValidateError;

		// Token: 0x040001DF RID: 479
		private bool _isValidatePressError;

		// Token: 0x040001E0 RID: 480
		private bool _isValidateScaleError;

		// Token: 0x040001E1 RID: 481
		private bool _isPressIllegalChar;

		// Token: 0x040001E2 RID: 482
		private bool _isScaleIllegalChar;

		// Token: 0x040001E3 RID: 483
		private bool _isValidatePenetrateError;

		// Token: 0x040001E4 RID: 484
		private bool _isPenetrateIllegalChar;

		// Token: 0x040001E5 RID: 485
		private List<SampleLoadingUserControl.ComboBoxItemInjMethod> _injMethodListEnum = new List<SampleLoadingUserControl.ComboBoxItemInjMethod>
		{
			new SampleLoadingUserControl.ComboBoxItemInjMethod
			{
				InjMethod = BalticInjectionType.PartialLoop,
				ValueString = "Partial Loop"
			},
			new SampleLoadingUserControl.ComboBoxItemInjMethod
			{
				InjMethod = BalticInjectionType.uLPickup,
				ValueString = "µL Pickup"
			}
		};

		// Token: 0x040001E6 RID: 486
		public BindableBalticMethod _method;

		// Token: 0x040001E7 RID: 487
		private BalticInstrumentFacade _instrument;

		// Token: 0x040001EA RID: 490
		private ExperimentInfo _experiment;

		// Token: 0x040001EB RID: 491
		private string _pressureTolTip;

		// Token: 0x020000FC RID: 252
		public class ComboBoxItemInjMethod
		{
			// Token: 0x17000176 RID: 374
			// (get) Token: 0x060007AA RID: 1962 RVA: 0x0003DC22 File Offset: 0x0003BE22
			// (set) Token: 0x060007AB RID: 1963 RVA: 0x0003DC2A File Offset: 0x0003BE2A
			public BalticInjectionType InjMethod { get; set; }

			// Token: 0x17000177 RID: 375
			// (get) Token: 0x060007AC RID: 1964 RVA: 0x0003DC33 File Offset: 0x0003BE33
			// (set) Token: 0x060007AD RID: 1965 RVA: 0x0003DC3B File Offset: 0x0003BE3B
			public string ValueString { get; set; }
		}

		// Token: 0x020000FD RID: 253
		// (Invoke) Token: 0x060007B0 RID: 1968
		private delegate void SetErrorConditionCallback(string subject, string message);

		// Token: 0x020000FE RID: 254
		// (Invoke) Token: 0x060007B4 RID: 1972
		public delegate void ValidationUpdate(bool isValid);

		// Token: 0x020000FF RID: 255
		// (Invoke) Token: 0x060007B8 RID: 1976
		public delegate void ModificationUpdate(bool isModified);
	}
}
