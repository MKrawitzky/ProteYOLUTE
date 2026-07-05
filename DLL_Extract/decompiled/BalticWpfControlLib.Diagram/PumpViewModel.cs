using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class PumpViewModel : ActivatableAndErrorAwareDataContext, IPumpViewModel
{
	private double _throughput;

	private string _throughputUnit;

	private double _throughputSetPoint;

	private double _pressure;

	private string _pressureUnit;

	private double _volumeUsed;

	private double _volumeLeft;

	private string _volumeUnit;

	private double _fillLevel;

	private string _title;

	private bool _isService;

	public double Throughput
	{
		get
		{
			return _throughput;
		}
		set
		{
			SetProperty(ref _throughput, value, "Throughput");
		}
	}

	public string ThroughputUnit
	{
		get
		{
			return _throughputUnit;
		}
		set
		{
			SetProperty(ref _throughputUnit, value, "ThroughputUnit");
		}
	}

	public double ThroughputSetPoint
	{
		get
		{
			return _throughputSetPoint;
		}
		set
		{
			SetProperty(ref _throughputSetPoint, value, "ThroughputSetPoint");
		}
	}

	public double Pressure
	{
		get
		{
			return _pressure;
		}
		set
		{
			SetProperty(ref _pressure, value, "Pressure");
		}
	}

	public string PressureUnit
	{
		get
		{
			return _pressureUnit;
		}
		set
		{
			SetProperty(ref _pressureUnit, value, "PressureUnit");
		}
	}

	public double VolumeUsed
	{
		get
		{
			return _volumeUsed;
		}
		set
		{
			SetProperty(ref _volumeUsed, value, "VolumeUsed");
		}
	}

	public double VolumeLeft
	{
		get
		{
			return _volumeLeft;
		}
		set
		{
			SetProperty(ref _volumeLeft, value, "VolumeLeft");
		}
	}

	public string VolumeUnit
	{
		get
		{
			return _volumeUnit;
		}
		set
		{
			SetProperty(ref _volumeUnit, value, "VolumeUnit");
		}
	}

	public double FillLevel
	{
		get
		{
			return _fillLevel;
		}
		set
		{
			SetProperty(ref _fillLevel, value, "FillLevel");
		}
	}

	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			SetProperty(ref _title, value, "Title");
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
			SetProperty(ref _isService, value, "IsService");
		}
	}
}
