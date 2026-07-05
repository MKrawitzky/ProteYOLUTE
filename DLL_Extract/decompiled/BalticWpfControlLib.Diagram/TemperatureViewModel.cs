using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class TemperatureViewModel : ActivatableAndErrorAwareDataContext, ITemperatureViewModel
{
	private double _temperature = 20.0;

	private string _temperatureUnit;

	private double _temperatureSetPoint;

	private string _temperatureSetPointUnit;

	public double Temperature
	{
		get
		{
			return _temperature;
		}
		set
		{
			SetProperty(ref _temperature, value, "Temperature");
		}
	}

	public string TemperatureUnit
	{
		get
		{
			return _temperatureUnit;
		}
		set
		{
			SetProperty(ref _temperatureUnit, value, "TemperatureUnit");
		}
	}

	public double TemperatureSetPoint
	{
		get
		{
			return _temperatureSetPoint;
		}
		set
		{
			SetProperty(ref _temperatureSetPoint, value, "TemperatureSetPoint");
		}
	}

	public string TemperatureSetPointUnit
	{
		get
		{
			return _temperatureSetPointUnit;
		}
		set
		{
			SetProperty(ref _temperatureSetPointUnit, value, "TemperatureSetPointUnit");
		}
	}
}
