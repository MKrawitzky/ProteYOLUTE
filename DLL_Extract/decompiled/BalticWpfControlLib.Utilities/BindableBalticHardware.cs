// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BalticClassLib;

namespace BalticWpfControlLib.Utilities;

public class BindableBalticHardware : BindableBase, IDataErrorInfo
{
	[CompilerGenerated]
	private BalticHWProfile _003ChardwareProfile_003EP;

	private static readonly Regex _ipRegex = new Regex("^(\\d{1,3}\\.){3}\\d{1,3}$");

	public string AutosamplerIP
	{
		get
		{
			return _003ChardwareProfile_003EP.Autosampler.DeviceIP;
		}
		set
		{
			_003ChardwareProfile_003EP.Autosampler.DeviceIP = value;
			OnPropertyChanged("AutosamplerIP");
			OnPropertyChanged("PumpHost");
		}
	}

	public string PumpHost
	{
		get
		{
			return _003ChardwareProfile_003EP.LC.DeviceIP;
		}
		set
		{
			_003ChardwareProfile_003EP.LC.DeviceIP = value;
			OnPropertyChanged("PumpHost");
			OnPropertyChanged("AutosamplerIP");
		}
	}

	public bool IsSimulate
	{
		get
		{
			return _003ChardwareProfile_003EP.IsSimulate;
		}
		set
		{
			_003ChardwareProfile_003EP.IsSimulate = value;
			if (value)
			{
				IsCTCSimulate = false;
			}
			OnPropertyChanged("IsSimulate");
		}
	}

	public bool IsCTCSimulate
	{
		get
		{
			return _003ChardwareProfile_003EP.IsCTCSimulate;
		}
		set
		{
			_003ChardwareProfile_003EP.IsCTCSimulate = value;
			if (value)
			{
				IsSimulate = false;
			}
			OnPropertyChanged("IsCTCSimulate");
		}
	}

	public bool IsPressurePSI
	{
		get
		{
			return _003ChardwareProfile_003EP.IsPressurePSI;
		}
		set
		{
			_003ChardwareProfile_003EP.IsPressurePSI = value;
			OnPropertyChanged("IsPressurePSI");
		}
	}

	public int MaxPumpPressure
	{
		get
		{
			return _003ChardwareProfile_003EP.MaxPumpPressure;
		}
		set
		{
			_003ChardwareProfile_003EP.MaxPumpPressure = value;
			OnPropertyChanged("MaxPumpPressure");
		}
	}

	public bool UseIdleProcedure
	{
		get
		{
			return _003ChardwareProfile_003EP.UseIdleProcedure;
		}
		set
		{
			_003ChardwareProfile_003EP.UseIdleProcedure = value;
			OnPropertyChanged("UseIdleProcedure");
		}
	}

	public bool IsColumnOvenConnected
	{
		get
		{
			return _003ChardwareProfile_003EP.IsColumnOvenConnected;
		}
		set
		{
			_003ChardwareProfile_003EP.IsColumnOvenConnected = value;
			OnPropertyChanged("IsColumnOvenConnected");
		}
	}

	public BalticHWProfile.ASProfile.TrayType Slot1TrayType
	{
		get
		{
			return _003ChardwareProfile_003EP.Autosampler.Slot1TrayType;
		}
		set
		{
			_003ChardwareProfile_003EP.Autosampler.Slot1TrayType = value;
			OnPropertyChanged("Slot1TrayType");
		}
	}

	public BalticHWProfile.ASProfile.TrayType Slot2TrayType
	{
		get
		{
			return _003ChardwareProfile_003EP.Autosampler.Slot2TrayType;
		}
		set
		{
			_003ChardwareProfile_003EP.Autosampler.Slot2TrayType = value;
			OnPropertyChanged("Slot2TrayType");
		}
	}

	public string Slot1TrayTypeName
	{
		get
		{
			return _003ChardwareProfile_003EP.Autosampler.Slot1TrayTypeName;
		}
		set
		{
			_003ChardwareProfile_003EP.Autosampler.Slot1TrayTypeName = value;
			if (Enum.TryParse<BalticHWProfile.ASProfile.TrayType>(_003ChardwareProfile_003EP.Autosampler.Slot1TrayTypeName, out var result))
			{
				_003ChardwareProfile_003EP.Autosampler.Slot1TrayType = result;
			}
			OnPropertyChanged("Slot1TrayTypeName");
		}
	}

	public string Slot2TrayTypeName
	{
		get
		{
			return _003ChardwareProfile_003EP.Autosampler.Slot2TrayTypeName;
		}
		set
		{
			_003ChardwareProfile_003EP.Autosampler.Slot2TrayTypeName = value;
			if (Enum.TryParse<BalticHWProfile.ASProfile.TrayType>(_003ChardwareProfile_003EP.Autosampler.Slot2TrayTypeName, out var result))
			{
				_003ChardwareProfile_003EP.Autosampler.Slot2TrayType = result;
			}
			OnPropertyChanged("Slot2TrayTypeName");
		}
	}

	public BalticHWProfile.ASProfile.TrayTemperature TrayTemperature
	{
		get
		{
			return _003ChardwareProfile_003EP.Autosampler.SampleTrayTemperature;
		}
		set
		{
			_003ChardwareProfile_003EP.Autosampler.SampleTrayTemperature = value;
			OnPropertyChanged("TrayTemperature");
		}
	}

	public string ApplicationKey
	{
		get
		{
			return _003ChardwareProfile_003EP.ApplicationKey;
		}
		set
		{
			_003ChardwareProfile_003EP.ApplicationKey = value;
			OnPropertyChanged("ApplicationKey");
		}
	}

	public List<BalticHWProfile.CapillaryItem> ConnectionProfile
	{
		get
		{
			return _003ChardwareProfile_003EP.ConnectionProfile;
		}
		set
		{
			_003ChardwareProfile_003EP.ConnectionProfile = value;
			OnPropertyChanged("ConnectionProfile");
		}
	}

	public string Error => string.Empty;

	public string this[string columnName]
	{
		get
		{
			string result = string.Empty;
			if (!(columnName == "AutosamplerIP"))
			{
				if (columnName == "PumpHost")
				{
					if (!_ipRegex.IsMatch(PumpHost))
					{
						result = "Invalid IP address";
					}
					else if (PumpHost.Equals(AutosamplerIP))
					{
						result = "IP address must not be the same as from the sampler";
					}
				}
			}
			else if (!_ipRegex.IsMatch(AutosamplerIP))
			{
				result = "Invalid IP address";
			}
			else if (AutosamplerIP.Equals(PumpHost))
			{
				result = "IP address must not be the same as from the pump";
			}
			return result;
		}
	}

	public BindableBalticHardware(BalticHWProfile hardwareProfile)
	{
		_003ChardwareProfile_003EP = hardwareProfile;
		base._002Ector();
	}
}
