using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Threading;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class GradientTableUserControl : UserControl, INotifyPropertyChanged, IComponentConnector, IStyleConnector
{
	public delegate void ValidationUpdate(bool isValid, string header = "", string subject = "", string message = "");

	public delegate void GradientMainUpdate(WPFConstants.UpdateType updateType);

	private readonly BalticInstrumentFacade _instrument;

	private int _nIllegalCharEntries;

	private bool _hasMinMaxFlow;

	private double _minFlow;

	private double _maxFlow;

	private bool _isTwoColumn;

	private bool _isOneOrTwoColumn;

	public readonly int DesignWidth = 249;

	private ExperimentInfo _experiment;

	internal Border bdrTable;

	internal DataGrid dgGradient;

	private bool _contentLoaded;

	public ExperimentInfo Experiment
	{
		get
		{
			return _experiment;
		}
		set
		{
			_experiment = value;
			_hasMinMaxFlow = false;
		}
	}

	public double MaxFlow
	{
		get
		{
			if (_experiment != null)
			{
				return 9.0;
			}
			return (_experiment?.Separator?.MaximumFlow).GetValueOrDefault(9.0);
		}
	}

	public BindableBalticMethod Method { get; }

	public bool IsTwoColumn
	{
		get
		{
			return _isTwoColumn;
		}
		set
		{
			_isTwoColumn = value;
			NotifyPropertyChanged("IsTwoColumn");
		}
	}

	public bool IsOneOrTwoColumn
	{
		get
		{
			return _isOneOrTwoColumn;
		}
		set
		{
			_isOneOrTwoColumn = value;
			NotifyPropertyChanged("IsOneOrTwoColumn");
		}
	}

	public BalticGradientList GradientTable
	{
		get
		{
			return Method.GradientTable;
		}
		set
		{
			Method.GradientTable = value;
			NotifyPropertyChanged("GradientTable");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event ValidationUpdate ValidationUpdateEvent;

	public event GradientMainUpdate GradientMainUpdateEvent;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public GradientTableUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
	{
		Method = method;
		_instrument = instrument;
		_experiment = experiment;
		InitializeComponent();
		bdrTable.DataContext = this;
		base.DataContext = this;
		LoadMethod();
		IsTwoColumn = Method.UsesSepColumn && Method.UsesTrapColumn;
		IsOneOrTwoColumn = Method.UsesSepColumn || Method.UsesTrapColumn;
	}

	public void UpdateGradientTableControls()
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			IsTwoColumn = Method.UsesSepColumn && Method.UsesTrapColumn;
			IsOneOrTwoColumn = Method.UsesSepColumn || Method.UsesTrapColumn;
		});
	}

	private void GradientColumnHeader_Click(object sender, RoutedEventArgs e)
	{
		_ = (DataGridColumnHeader)sender;
	}

	private void GradientAddEvent_Click(object sender, RoutedEventArgs e)
	{
		if (dgGradient.SelectedCells.Count > 0)
		{
			int rowIndex = DataGridHelper.GetRowIndex(DataGridHelper.GetCell(dgGradient.SelectedCells[0]));
			BalticGradientItem item = new BalticGradientItem(Method.GradientTable[rowIndex])
			{
				Duration = 0.0
			};
			Method.GradientTable.Insert(rowIndex + 1, item);
			this.GradientMainUpdateEvent?.Invoke(WPFConstants.UpdateType.AddRemove);
			CheckGradientTableEnable();
			ValidateParameters();
			dgGradient.ScrollIntoView(item);
		}
	}

	private void GradientRemoveEvent_Click(object sender, RoutedEventArgs e)
	{
		if (dgGradient.SelectedCells.Count <= 0 || dgGradient.Items.Count <= 1)
		{
			return;
		}
		DataGridCell cell = DataGridHelper.GetCell(dgGradient.SelectedCells[0]);
		Method.GradientTable.RemoveAt(DataGridHelper.GetRowIndex(cell));
		if (Method.GradientTable[Method.GradientTable.Count - 1].Duration > 0.0)
		{
			Method.GradientTable[Method.GradientTable.Count - 1].Duration = 0.0;
		}
		if (Method.GradientTable[0].Time > 0.0)
		{
			Method.GradientTable[0].Time = 0.0;
			if (Method.GradientTable.Count > 1)
			{
				Method.GradientTable[0].Duration = Method.GradientTable[1].Time;
			}
			else
			{
				Method.GradientTable[0].Duration = 0.0;
			}
		}
		this.GradientMainUpdateEvent?.Invoke(WPFConstants.UpdateType.AddRemove);
		CheckGradientTableEnable();
		ValidateParameters();
	}

	private void CheckGradientTableEnable()
	{
		for (int i = 0; i < Method.GradientTable.Count; i++)
		{
			Method.GradientTable[i].IsTimeEditable = i > 0;
			Method.GradientTable[i].IsLastRow = i == Method.GradientTable.Count - 1;
		}
	}

	public void LoadMethod()
	{
		dgGradient.ItemsSource = Method.GradientTable;
		CheckGradientTableEnable();
	}

	private void FlowTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
		this.GradientMainUpdateEvent?.Invoke(WPFConstants.UpdateType.FlowRate);
	}

	public void UpdateGradientTime(double gradientTime, bool reset)
	{
		if (reset)
		{
			dgGradient.Items.Refresh();
			return;
		}
		for (int i = 0; i < Method.GradientTable.Count; i++)
		{
			if (Method.GradientTable[i].SmartName == "GradInitial")
			{
				Method.GradientTable[i].Duration = gradientTime;
				for (int j = i + 1; j < Method.GradientTable.Count; j++)
				{
					Method.GradientTable[j].Time = Method.GradientTable[j - 1].Time + Method.GradientTable[j - 1].Duration;
				}
				break;
			}
		}
	}

	private void CompositionTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DoubleTextBox doubleTextBox = d as DoubleTextBox;
		if (string.Format(CultureInfo.InvariantCulture, "{0:0.0}", (double)e.NewValue) == "0.0")
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
			{
				if (doubleTextBox != null)
				{
					doubleTextBox.SelectionStart = 1;
				}
			});
		}
		ValidateParameters();
		this.GradientMainUpdateEvent?.Invoke(WPFConstants.UpdateType.Composition);
	}

	private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
	{
		ValidateParameters();
		this.GradientMainUpdateEvent?.Invoke(WPFConstants.UpdateType.Time);
	}

	private void TimeTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DoubleTextBox doubleTextBox = d as DoubleTextBox;
		if (string.Format(CultureInfo.InvariantCulture, "{0:0.00}", (double)e.NewValue) == "0.00")
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)delegate
			{
				if (doubleTextBox != null)
				{
					doubleTextBox.SelectionStart = 1;
				}
			});
		}
		ValidateParameters();
		this.GradientMainUpdateEvent?.Invoke(WPFConstants.UpdateType.Time);
		try
		{
			for (int i = 0; i < Method.GradientTable.Count; i++)
			{
				if (i > 0)
				{
					if (Method.GradientTable[i].Time >= Method.GradientTable[i - 1].Time + 0.01)
					{
						Method.GradientTable[i - 1].Duration = Method.GradientTable[i].Time - Method.GradientTable[i - 1].Time;
					}
					else
					{
						Method.GradientTable[i - 1].Duration = Method.GradientTable[i].Time - Method.GradientTable[i].Time;
					}
				}
			}
		}
		catch
		{
		}
	}

	private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
	{
		if (!Severity.Error.Equals(e.Severity))
		{
			return;
		}
		SetErrorCondition(e, isTimeError: false);
		if (_hasMinMaxFlow || !e.Message.ToLower().Contains("flow must be between"))
		{
			return;
		}
		try
		{
			string[] array = Regex.Split(e.Message, "([-+]?[0-9]*\\.?[0-9]+)");
			int num = 0;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (double.TryParse(Convert.ToString(array2[i]), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result))
				{
					switch (num)
					{
					case 0:
						_minFlow = result;
						break;
					case 1:
						_maxFlow = result;
						break;
					}
					_hasMinMaxFlow = true;
					num++;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void ValidateParameters()
	{
		BalticMethod method = Method.ToBalticMethod();
		ProcedureInfo elutionProcedure = _instrument.GetElutionProcedure(Method.ElutionName);
		ProcedureArguments procedureArguments = elutionProcedure.CreateArguments();
		ProcedureArguments procedureArguments2 = elutionProcedure.CreateAdvancedArguments();
		ChildProcedureArguments childProcedureArguments = elutionProcedure.CreateAdvancedChildArguments();
		ElutionMethodUtil.PopulateMethodArguments(_instrument.IsColumnOvenConnected, method, procedureArguments, procedureArguments2, childProcedureArguments);
		foreach (ProcedureArgument item in procedureArguments2)
		{
			procedureArguments.Add(new ProcedureArgument(item));
		}
		_instrument.ValidationMessageReported += ValidationErrorHandler;
		try
		{
			ResetErrorConditions();
			if (_experiment != null)
			{
				_instrument.ValidateMethodProcedureOffLine(_experiment, elutionProcedure, procedureArguments, childProcedureArguments);
			}
			else
			{
				_instrument.ValidateProcedureOffLine(elutionProcedure, procedureArguments, childProcedureArguments);
			}
		}
		finally
		{
			_instrument.ValidationMessageReported -= ValidationErrorHandler;
		}
	}

	private void SetErrorCondition(ProcedureReportEventArgs e, bool isTimeError)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			if (e.IsGradientSubject)
			{
				int num = Convert.ToInt32(e.Subject.Substring(8));
				if (num >= 0 && num < Method.GradientTable.Count)
				{
					if (isTimeError)
					{
						Method.GradientTable[num].IsInTimeOrder = false;
					}
					else
					{
						Method.GradientTable[num].IsParamValid = false;
						Method.GradientTable[num].ParamToolTip = e.Message;
					}
					if (isTimeError && !Method.GradientTable[num].IsParamValid)
					{
						Method.GradientTable[num].ErrorToolTip = e.Message + "\n" + Method.GradientTable[num].ParamToolTip;
					}
					else
					{
						Method.GradientTable[num].ErrorToolTip = e.Message;
					}
				}
			}
			bool flag = true;
			foreach (BalticGradientItem item in Method.GradientTable)
			{
				if (!item.IsParamValid || !item.IsInTimeOrder)
				{
					flag = false;
					break;
				}
			}
			if (Method.GradientTable.Count > 0)
			{
				foreach (BalticGradientItem item2 in Method.GradientTable)
				{
					item2.IsLastRowValid = Method.GradientTable[Method.GradientTable.Count - 1].IsValidState;
				}
			}
			this.ValidationUpdateEvent?.Invoke(flag && _nIllegalCharEntries == 0, e.Header, e.Subject, e.Message);
		});
	}

	private void ResetErrorConditions()
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			bool flag = true;
			foreach (BalticGradientItem item in Method.GradientTable)
			{
				item.IsParamValid = true;
				item.ParamToolTip = null;
				if (item.IsInTimeOrder)
				{
					item.ErrorToolTip = null;
				}
				else
				{
					flag = false;
				}
			}
			if (Method.GradientTable.Count > 0)
			{
				foreach (BalticGradientItem item2 in Method.GradientTable)
				{
					item2.IsLastRowValid = Method.GradientTable[Method.GradientTable.Count - 1].IsValidState;
				}
			}
			this.ValidationUpdateEvent?.Invoke(flag && _nIllegalCharEntries == 0);
		});
	}

	private void Validation_Error(object sender, ValidationErrorEventArgs e)
	{
		if (e.Action == ValidationErrorEventAction.Added)
		{
			_nIllegalCharEntries++;
		}
		else
		{
			_nIllegalCharEntries--;
		}
		this.ValidationUpdateEvent?.Invoke(_nIllegalCharEntries == 0);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/gradienttableusercontrol.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 2:
			bdrTable = (Border)target;
			break;
		case 3:
			dgGradient = (DataGrid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
		{
			EventSetter eventSetter = new EventSetter();
			eventSetter.Event = ButtonBase.Click;
			eventSetter.Handler = new RoutedEventHandler(GradientColumnHeader_Click);
			((Style)target).Setters.Add(eventSetter);
			break;
		}
		case 4:
			((DoubleTextBox)target).AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			((DoubleTextBox)target).LostFocus += TimeTextBox_LostFocus;
			((DoubleTextBox)target).ValueChanged += TimeTextBox_ValueChanged;
			break;
		case 5:
			((DoubleTextBox)target).AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			((DoubleTextBox)target).ValueChanged += CompositionTextBox_ValueChanged;
			break;
		case 6:
			((DoubleTextBox)target).AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			((DoubleTextBox)target).ValueChanged += FlowTextBox_ValueChanged;
			break;
		case 2:
		case 3:
			break;
		}
	}
}
