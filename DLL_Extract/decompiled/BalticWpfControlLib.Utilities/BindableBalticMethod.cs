using System;
using System.Collections.Generic;
using System.Linq;
using BalticClassLib;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib.Utilities;

public class BindableBalticMethod : BindableBase
{
	public class ColumnEquilibration : BindableBase
	{
		private double _pressure;

		private double _scale = 1.0;

		private double _defPressure;

		private double _defScale = 1.0;

		private double _equilTime;

		private double _penetrationDepth = 30.0;

		private BalticInjectionType _injectionMethod;

		private bool _isBottomSense;

		private BalticInjectionType _defInjectionMethod;

		private bool _defIsBottomSense;

		private double _defPenetrationDepth = 30.0;

		public double Pressure
		{
			get
			{
				return _pressure;
			}
			set
			{
				SetField(ref _pressure, value, "Pressure");
			}
		}

		public double Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				SetField(ref _scale, value, "Scale");
			}
		}

		public bool IsBottomSense
		{
			get
			{
				return _isBottomSense;
			}
			set
			{
				SetField(ref _isBottomSense, value, "IsBottomSense");
			}
		}

		public BalticInjectionType InjectionMethod
		{
			get
			{
				return _injectionMethod;
			}
			set
			{
				SetField(ref _injectionMethod, value, "InjectionMethod");
			}
		}

		public double DefaultPressure
		{
			get
			{
				return _defPressure;
			}
			set
			{
				SetField(ref _defPressure, value, "DefaultPressure");
			}
		}

		public double DefaultScale
		{
			get
			{
				return _defScale;
			}
			set
			{
				SetField(ref _defScale, value, "DefaultScale");
			}
		}

		public bool DefaultIsBottomSense
		{
			get
			{
				return _defIsBottomSense;
			}
			set
			{
				SetField(ref _defIsBottomSense, value, "DefaultIsBottomSense");
			}
		}

		public BalticInjectionType DefaultInjectionMethod
		{
			get
			{
				return _defInjectionMethod;
			}
			set
			{
				SetField(ref _defInjectionMethod, value, "DefaultInjectionMethod");
			}
		}

		public double EquilTime
		{
			get
			{
				return _equilTime;
			}
			set
			{
				SetField(ref _equilTime, value, "EquilTime");
			}
		}

		public double PenetrationDepth
		{
			get
			{
				return _penetrationDepth;
			}
			set
			{
				SetField(ref _penetrationDepth, value, "PenetrationDepth");
			}
		}

		public double DefaultPenetrationDepth
		{
			get
			{
				return _defPenetrationDepth;
			}
			set
			{
				SetField(ref _defPenetrationDepth, value, "DefaultPenetrationDepth");
			}
		}

		public ColumnEquilibration()
		{
		}

		public ColumnEquilibration(double pressure, int scale, double defPressure, int defScale, bool isBottomSense, BalticInjectionType injectionMethod, bool defIsBottomSense, BalticInjectionType defInjectionMethod, double equilTime, double penetrationDepth = 30.0, double defPenetrationDepth = 30.0)
		{
			_pressure = pressure;
			_scale = scale;
			EquilTime = equilTime;
			_defPressure = defPressure;
			_defScale = defScale;
			_isBottomSense = isBottomSense;
			_injectionMethod = injectionMethod;
			_penetrationDepth = penetrationDepth;
			_defIsBottomSense = defIsBottomSense;
			_defInjectionMethod = defInjectionMethod;
			_defPenetrationDepth = defPenetrationDepth;
		}

		public void RevertToDefault()
		{
			_pressure = _defPressure;
			_scale = _defScale;
			_isBottomSense = _defIsBottomSense;
			_injectionMethod = _defInjectionMethod;
			_penetrationDepth = _defPenetrationDepth;
		}

		public void Set(BalticMethod.ColumnEquil equil)
		{
			_pressure = equil.Pressure;
			_scale = equil.Scale;
			EquilTime = equil.EquilTime;
			_defPressure = equil.DefaultPressure;
			_defScale = equil.DefaultScale;
			_isBottomSense = equil.IsBottomSense;
			_injectionMethod = equil.InjectionMethod;
			_defIsBottomSense = equil.DefaultIsBottomSense;
			_defInjectionMethod = equil.DefaultInjectionMethod;
			_penetrationDepth = equil.PenetrationDepth;
			_defPenetrationDepth = equil.DefaultPenetrationDepth;
		}

		public BalticMethod.ColumnEquil ToColumnEquil()
		{
			return new BalticMethod.ColumnEquil(_pressure, _defPressure, _scale, _defScale, _isBottomSense, _injectionMethod, _defIsBottomSense, _defInjectionMethod, _equilTime, _penetrationDepth, _defPenetrationDepth);
		}
	}

	public class AdvancedSett : BindableBase
	{
		public class AdvancedParameter : BindableBase
		{
			private string _name;

			private object _value;

			private object _defaultValue;

			private string _unit;

			public string Name
			{
				get
				{
					return _name;
				}
				set
				{
					SetField(ref _name, value, "Name");
				}
			}

			public object Value
			{
				get
				{
					return _value;
				}
				set
				{
					SetField(ref _value, value, "Value");
				}
			}

			public object DefaultValue
			{
				get
				{
					return _defaultValue;
				}
				set
				{
					SetField(ref _defaultValue, value, "DefaultValue");
				}
			}

			public string Unit
			{
				get
				{
					return _unit;
				}
				set
				{
					SetField(ref _unit, value, "Unit");
				}
			}

			public AdvancedParameter()
			{
			}

			public AdvancedParameter(AdvancedParameter advParam)
			{
				Name = advParam.Name;
				Value = advParam.Value;
				DefaultValue = advParam.DefaultValue;
				Unit = advParam.Unit;
			}

			public AdvancedParameter(string name, object value, object defaultValue, string unit)
			{
				Name = name;
				Value = value;
				DefaultValue = defaultValue;
				Unit = unit;
			}
		}

		public class AdvancedChildParameter : BindableBase
		{
			private string _header = "";

			private string _name = "";

			private object _value;

			private object _defaultValue;

			private string _unit;

			private bool _isService;

			public string Header
			{
				get
				{
					return _header;
				}
				set
				{
					SetField(ref _header, value, "Header");
				}
			}

			public string Name
			{
				get
				{
					return _name;
				}
				set
				{
					SetField(ref _name, value, "Name");
				}
			}

			public object Value
			{
				get
				{
					return _value;
				}
				set
				{
					SetField(ref _value, value, "Value");
				}
			}

			public object DefaultValue
			{
				get
				{
					return _defaultValue;
				}
				set
				{
					SetField(ref _defaultValue, value, "DefaultValue");
				}
			}

			public string Unit
			{
				get
				{
					return _unit;
				}
				set
				{
					SetField(ref _unit, value, "Unit");
				}
			}

			public bool IsService
			{
				get
				{
					return _isService;
				}
				set
				{
					SetField(ref _isService, value, "IsService");
				}
			}

			public AdvancedChildParameter()
			{
			}

			public AdvancedChildParameter(AdvancedChildParameter advParam)
			{
				Header = advParam.Header;
				Name = advParam.Name;
				Value = advParam.Value;
				DefaultValue = advParam.DefaultValue;
				Unit = advParam.Unit;
				IsService = advParam.IsService;
			}

			public AdvancedChildParameter(string header, string name, object value, object defaultValue, string unit, bool isService = false)
			{
				Header = header;
				Name = name;
				Value = value;
				DefaultValue = defaultValue;
				Unit = unit;
				_isService = isService;
			}
		}

		private string _header = "";

		private double _calibrantVolume = 1.0;

		private double _calibrantTime;

		private int[] _headerFgColor = new int[3];

		private List<AdvancedParameter> _parameters = new List<AdvancedParameter>();

		private List<AdvancedChildParameter> _childParameters = new List<AdvancedChildParameter>();

		public string Header
		{
			get
			{
				return _header;
			}
			set
			{
				SetField(ref _header, value, "Header");
			}
		}

		public int[] HeaderFgColor
		{
			get
			{
				return _headerFgColor;
			}
			set
			{
				SetField(ref _headerFgColor, value, "HeaderFgColor");
			}
		}

		public List<AdvancedParameter> Parameters
		{
			get
			{
				return _parameters;
			}
			set
			{
				SetField(ref _parameters, value, "Parameters");
			}
		}

		public List<AdvancedChildParameter> ChildParameters
		{
			get
			{
				return _childParameters;
			}
			set
			{
				SetField(ref _childParameters, value, "ChildParameters");
			}
		}

		public bool IsCalibrantInject { get; set; }

		public bool IsExtendedWash { get; set; }

		public double CalibrantVolume
		{
			get
			{
				return _calibrantVolume;
			}
			set
			{
				SetField(ref _calibrantVolume, value, "CalibrantVolume");
			}
		}

		public double CalibrantTime
		{
			get
			{
				return _calibrantTime;
			}
			set
			{
				SetField(ref _calibrantTime, value, "CalibrantTime");
			}
		}

		public AdvancedSett()
		{
		}

		public AdvancedSett(AdvancedSett advParams)
		{
			Header = advParams.Header;
			IsCalibrantInject = advParams.IsCalibrantInject;
			IsExtendedWash = advParams.IsExtendedWash;
			CalibrantVolume = advParams.CalibrantVolume;
			CalibrantTime = advParams.CalibrantTime;
			HeaderFgColor = advParams.HeaderFgColor.ToArray();
			foreach (AdvancedParameter parameter in advParams.Parameters)
			{
				Parameters.Add(new AdvancedParameter(parameter));
			}
			foreach (AdvancedChildParameter childParameter in advParams.ChildParameters)
			{
				ChildParameters.Add(new AdvancedChildParameter(childParameter));
			}
		}

		public void RevertToDefault()
		{
			foreach (AdvancedParameter parameter in Parameters)
			{
				parameter.Value = parameter.DefaultValue;
			}
			foreach (AdvancedChildParameter childParameter in ChildParameters)
			{
				childParameter.Value = childParameter.DefaultValue;
			}
		}

		public void RevertChildrenToDefault()
		{
			foreach (AdvancedChildParameter childParameter in ChildParameters)
			{
				childParameter.Value = childParameter.DefaultValue;
			}
		}

		public void Set(BalticMethod.AdvancedSett advSett)
		{
			_header = advSett.Header;
			IsCalibrantInject = advSett.IsCalibrantInject;
			IsExtendedWash = advSett.IsCalibrantInject;
			_calibrantVolume = advSett.CalibrantVolume;
			_calibrantTime = advSett.CalibrantTime;
			_headerFgColor = advSett.HeaderFgColor.ToArray();
			_parameters.Clear();
			foreach (BalticMethod.AdvancedSett.AdvancedParameter parameter in advSett.Parameters)
			{
				Parameters.Add(new AdvancedParameter(parameter.Name, parameter.Value, parameter.DefaultValue, parameter.Unit));
			}
			_childParameters.Clear();
			foreach (BalticMethod.AdvancedSett.AdvancedChildParameter childParameter in advSett.ChildParameters)
			{
				ChildParameters.Add(new AdvancedChildParameter(childParameter.Header, childParameter.AdvParameter.Name, childParameter.AdvParameter.Value, childParameter.AdvParameter.DefaultValue, childParameter.AdvParameter.Unit));
			}
			if (Parameters.Count == 0)
			{
				Parameters.Add(new AdvancedParameter("MS Calibrant Injection", IsCalibrantInject, false, ""));
				Parameters.Add(new AdvancedParameter("Extended Wash", IsExtendedWash, false, ""));
			}
		}
	}

	public class ElutionType
	{
		public string Name { get; set; }

		public string DisplayName { get; set; }

		public string LegacyName { get; set; }

		public bool HasLegacyOption => !string.IsNullOrEmpty(LegacyName);

		public bool IsLegacy { get; set; }

		public ElutionType(string name, string displayName, string legacyName, bool isLegacy)
		{
			Name = name;
			DisplayName = displayName;
			LegacyName = legacyName;
			IsLegacy = isLegacy;
			base._002Ector();
		}
	}

	private readonly BalticMethod _balticMethod;

	private bool _isIsocratic;

	private bool _isSetTemperature = true;

	private BalticGradientList _gradientTable = new BalticGradientList();

	private double _gradientTime;

	private double _ovenTemperature;

	public List<ElutionType> ExperimentTypes { get; set; }

	public string ElutionName { get; set; }

	public string Description { get; set; }

	public bool IsIsocratic
	{
		get
		{
			return _isIsocratic;
		}
		set
		{
			SetField(ref _isIsocratic, value, "IsIsocratic");
		}
	}

	public bool IsSetTemperature
	{
		get
		{
			return _isSetTemperature;
		}
		set
		{
			SetField(ref _isSetTemperature, value, "IsSetTemperature");
		}
	}

	public bool UsesTrapColumn { get; set; }

	public bool UsesSepColumn { get; set; }

	public BalticGradientList GradientTable
	{
		get
		{
			return _gradientTable;
		}
		set
		{
			SetField(ref _gradientTable, value, "GradientTable");
		}
	}

	public string TrapColumnName { get; set; }

	public string SeparationColumnName { get; set; }

	public double TrapColumnVolume { get; set; }

	public double SeparationColumnVolume { get; set; }

	public ColumnEquilibration TrapColumnEquil { get; set; } = new ColumnEquilibration();


	public ColumnEquilibration SeparationColumnEquil { get; set; } = new ColumnEquilibration();


	public ColumnEquilibration SampleLoading { get; set; } = new ColumnEquilibration();


	public AdvancedSett AdvancedSettings { get; set; } = new AdvancedSett();


	public double OvenTemperature
	{
		get
		{
			return _ovenTemperature;
		}
		set
		{
			SetField(ref _ovenTemperature, value, "OvenTemperature");
		}
	}

	public double AcquisitionTime
	{
		get
		{
			if (_gradientTable.Count <= 0)
			{
				return 0.0;
			}
			return _gradientTable[GradientTable.Count - 1].Time + _gradientTable[GradientTable.Count - 1].Duration;
		}
	}

	public double GradientTime
	{
		get
		{
			return _gradientTime;
		}
		set
		{
			SetField(ref _gradientTime, value, "GradientTime");
		}
	}

	public BindableBalticMethod(BalticMethod balticMethod, BalticInstrumentFacade instrument)
	{
		ExperimentTypes = new List<ElutionType>();
		foreach (ProcedureInfo elutionTypeInfo in instrument.ElutionTypeInfoList)
		{
			ExperimentTypes.Add(new ElutionType(elutionTypeInfo.Name, elutionTypeInfo.DisplayName, elutionTypeInfo.LegacyName, elutionTypeInfo.IsLegacy));
		}
		_balticMethod = balticMethod;
		SetMethod(balticMethod);
	}

	public void Reset(double columnOvenMinTemperature)
	{
		SetMethod(new BalticMethod(columnOvenMinTemperature));
	}

	public void Refresh(ChildProcedureArguments advChildArguments)
	{
		SetMethod(_balticMethod);
		foreach (AdvancedSett.AdvancedChildParameter setting in AdvancedSettings.ChildParameters)
		{
			ChildProcedureArgument childProcedureArgument = advChildArguments.SingleOrDefault((ChildProcedureArgument s) => s.Header == setting.Header && s.Name == setting.Name);
			if (childProcedureArgument != null)
			{
				setting.IsService = childProcedureArgument.IsService;
			}
		}
	}

	private void SetMethod(BalticMethod balticMethod)
	{
		ElutionName = balticMethod.ElutionName;
		OvenTemperature = balticMethod.OvenTemperature;
		TrapColumnName = balticMethod.TrapName;
		SeparationColumnName = balticMethod.SeparatorName;
		SeparationColumnVolume = balticMethod.SeparatorVolume;
		TrapColumnVolume = balticMethod.TrapVolume;
		TrapColumnEquil.Set(balticMethod.TrapColumnEquil);
		SeparationColumnEquil.Set(balticMethod.SeparationColumnEquil);
		SampleLoading.Set(balticMethod.SampleLoading);
		AdvancedSettings.Set(balticMethod.AdvancedSettings);
		IsIsocratic = balticMethod.IsIsocratic;
		UsesTrapColumn = balticMethod.UsesTrapColumn;
		UsesSepColumn = balticMethod.UsesSepColumn;
		IsSetTemperature = balticMethod.IsSetTemperature;
		_gradientTable.Clear();
		for (int i = 0; i < balticMethod.Gradient.Count; i++)
		{
			BalticMethod.GradientItem gradientItem = balticMethod.Gradient[i];
			double duration = 0.0;
			if (i < balticMethod.Gradient.Count - 1)
			{
				duration = (balticMethod.Gradient[i + 1].Time - gradientItem.Time) / 60.0;
			}
			BalticGradientItem item = new BalticGradientItem(gradientItem.Time / 60.0, duration, Math.Round(gradientItem.Mix * 100.0, 1), gradientItem.Flow, _isIsocratic)
			{
				IsTimeEditable = (i > 0)
			};
			_gradientTable.Add(item);
		}
		if (_gradientTable.Count > 0)
		{
			_gradientTable[_gradientTable.Count - 1].IsLastRow = true;
		}
	}

	private static double ViscosityMix(double temperature, double percentageACN)
	{
		double num = temperature + 273.15;
		return Math.Exp(percentageACN * (-3.476 + 726.0 / num) + (1.0 - percentageACN) * (-5.414 + 1566.0 / num) + percentageACN * (1.0 - percentageACN) * (-1.762 + 929.0 / num)) / 100.0;
	}

	private static double ColumnFlow(Column column, double pressure, double ovenTemp = 20.0)
	{
		double num = ViscosityMix(ovenTemp, 0.0);
		double num2 = column.InnerDiamater * 0.5;
		return Math.Pow(10.0, 3.0) * (pressure * Math.Pow(10.0, 6.0) * Math.Pow(column.ParticleDiameter * 0.0001, 2.0) * Math.Pow(0.42, 3.0) * Math.PI * Math.Pow(num2 * 0.1, 2.0) * 60.0) / (180.0 * num * column.Length * 0.1 * Math.Pow(0.5800000000000001, 2.0));
	}

	public BalticMethod ToBalticMethod(BalticColumnList columns = null)
	{
		_balticMethod.ElutionName = ElutionName;
		_balticMethod.IsIsocratic = _isIsocratic;
		_balticMethod.IsSetTemperature = _isSetTemperature;
		_balticMethod.UsesTrapColumn = UsesTrapColumn;
		_balticMethod.UsesSepColumn = UsesSepColumn;
		_balticMethod.OvenTemperature = OvenTemperature;
		_balticMethod.TrapName = TrapColumnName;
		_balticMethod.SeparatorName = SeparationColumnName;
		_balticMethod.TrapVolume = TrapColumnVolume;
		_balticMethod.SeparatorVolume = SeparationColumnVolume;
		_balticMethod.TrapColumnEquil = new BalticMethod.ColumnEquil(TrapColumnEquil.Pressure, TrapColumnEquil.DefaultPressure, TrapColumnEquil.Scale, TrapColumnEquil.DefaultScale, TrapColumnEquil.IsBottomSense, TrapColumnEquil.InjectionMethod, TrapColumnEquil.DefaultIsBottomSense, TrapColumnEquil.DefaultInjectionMethod, TrapColumnEquil.EquilTime);
		_balticMethod.SeparationColumnEquil = new BalticMethod.ColumnEquil(SeparationColumnEquil.Pressure, SeparationColumnEquil.DefaultPressure, SeparationColumnEquil.Scale, SeparationColumnEquil.DefaultScale, SeparationColumnEquil.IsBottomSense, SeparationColumnEquil.InjectionMethod, SeparationColumnEquil.DefaultIsBottomSense, SeparationColumnEquil.DefaultInjectionMethod, SeparationColumnEquil.EquilTime);
		_balticMethod.SampleLoading = new BalticMethod.ColumnEquil(SampleLoading.Pressure, SampleLoading.DefaultPressure, SampleLoading.Scale, SampleLoading.DefaultScale, SampleLoading.IsBottomSense, SampleLoading.InjectionMethod, SampleLoading.DefaultIsBottomSense, SampleLoading.DefaultInjectionMethod, SampleLoading.EquilTime, SampleLoading.PenetrationDepth, SampleLoading.DefaultPenetrationDepth);
		_balticMethod.AdvancedSettings = new BalticMethod.AdvancedSett
		{
			Header = AdvancedSettings.Header,
			CalibrantTime = AdvancedSettings.CalibrantTime,
			CalibrantVolume = AdvancedSettings.CalibrantVolume,
			HeaderFgColor = AdvancedSettings.HeaderFgColor
		};
		foreach (AdvancedSett.AdvancedParameter parameter in AdvancedSettings.Parameters)
		{
			if (parameter.Name == "MS Calibrant Injection")
			{
				_balticMethod.AdvancedSettings.IsCalibrantInject = (bool)parameter.Value;
			}
			_balticMethod.AdvancedSettings.Parameters.Add(new BalticMethod.AdvancedSett.AdvancedParameter(parameter.Name, parameter.Value, parameter.DefaultValue, parameter.Unit));
		}
		foreach (AdvancedSett.AdvancedChildParameter childParameter in AdvancedSettings.ChildParameters)
		{
			_balticMethod.AdvancedSettings.ChildParameters.Add(new BalticMethod.AdvancedSett.AdvancedChildParameter(childParameter.Header, childParameter.Name, childParameter.Value, childParameter.DefaultValue, childParameter.Unit));
		}
		if (UsesSepColumn)
		{
			Column column = columns?.FirstOrDefault((Column x) => x.Name == SeparationColumnName);
			if (column != null)
			{
				if (column.IsAdvancedSettings && column.ColumnVolume > 0.0)
				{
					_balticMethod.SeparatorVolume = column.ColumnVolume;
				}
				if (column.IsAdvancedSettings && column.UnityFlow > 0.0)
				{
					_balticMethod.SeparatorUnityflow = column.UnityFlow;
				}
				else
				{
					_balticMethod.SeparatorUnityflow = ColumnFlow(column, 1.0, OvenTemperature);
				}
			}
		}
		if (UsesTrapColumn)
		{
			Column column2 = columns?.FirstOrDefault((Column x) => x.Name == TrapColumnName);
			if (column2 != null)
			{
				if (column2.IsAdvancedSettings && column2.ColumnVolume > 0.0)
				{
					_balticMethod.TrapVolume = column2.ColumnVolume;
				}
				if (column2.IsAdvancedSettings && column2.UnityFlow > 0.0)
				{
					_balticMethod.TrapUnityflow = column2.UnityFlow;
				}
				else
				{
					_balticMethod.TrapUnityflow = ColumnFlow(column2, 1.0);
				}
			}
		}
		_balticMethod.Gradient.Clear();
		foreach (BalticGradientItem item in _gradientTable)
		{
			_balticMethod.Gradient.Add(new BalticMethod.GradientItem(item.Time * 60.0, item.Flow, 0.01 * item.Composition));
		}
		return _balticMethod;
	}
}
