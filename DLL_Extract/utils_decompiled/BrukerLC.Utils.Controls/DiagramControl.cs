using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BrukerLC.Utils.Attached;
using BrukerLC.Utils.Events;

namespace BrukerLC.Utils.Controls;

public class DiagramControl : Control
{
	private readonly Dictionary<string, string> _errorDict = new Dictionary<string, string>();

	private const string PROPERTY_CATEGORY = "Diagram Control Properties";

	public static readonly DependencyProperty SiblingHasErrorProperty;

	public static readonly DependencyProperty PumpADataContextProperty;

	public static readonly DependencyProperty PumpBDataContextProperty;

	public static readonly DependencyProperty PumpLogDataContextProperty;

	public static readonly DependencyProperty SolventADataContextProperty;

	public static readonly DependencyProperty SolventBDataContextProperty;

	public static readonly DependencyProperty SensorADataContextProperty;

	public static readonly DependencyProperty SensorBDataContextProperty;

	public static readonly DependencyProperty ValveADataContextProperty;

	public static readonly DependencyProperty ValveBDataContextProperty;

	public static readonly DependencyProperty ValveCDataContextProperty;

	public static readonly DependencyProperty ValveDDataContextProperty;

	public static readonly DependencyProperty WasteADataContextProperty;

	public static readonly DependencyProperty WasteBDataContextProperty;

	public static readonly DependencyProperty WasteCDataContextProperty;

	public static readonly DependencyProperty WasteDDataContextProperty;

	public static readonly DependencyProperty InjectionDataContextProperty;

	public static readonly DependencyProperty BlindPlugDataContextProperty;

	public static readonly DependencyProperty PumpAToValveDTopLinkDataContextProperty;

	public static readonly DependencyProperty PumpAToValveDBottomLinkDataContextProperty;

	public static readonly DependencyProperty SolventAtoValveDLinkDataContextProperty;

	public static readonly DependencyProperty WasteDtoValveDLinkDataContextProperty;

	public static readonly DependencyProperty SensorAtoValveDLinkDataContextProperty;

	public static readonly DependencyProperty CapillarySensorAtoValveDDataContextProperty;

	public static readonly DependencyProperty SensorBtoValveCLinkDataContextProperty;

	public static readonly DependencyProperty CapillarySensorBtoValveCDataContextProperty;

	public static readonly DependencyProperty PumpBToValveCBottomLinkDataContextProperty;

	public static readonly DependencyProperty PumpBtoValveCTopLinkDataContextProperty;

	public static readonly DependencyProperty SolventBtoValveCTopLinkDataContextProperty;

	public static readonly DependencyProperty WasteCtoValveCLinkDataContextProperty;

	public static readonly DependencyProperty WasteBtoValveBLinkDataContextProperty;

	public static readonly DependencyProperty InjectionToValveALinkDataContextProperty;

	public static readonly DependencyProperty ValveDToValveALinkDataContextProperty;

	public static readonly DependencyProperty CapillaryValveDToValveADataContextProperty;

	public static readonly DependencyProperty ValveAToValveBLinkDataContextProperty;

	public static readonly DependencyProperty SensorAToMidLinkDataContextProperty;

	public static readonly DependencyProperty CapillarySensorAToMidDataContextProperty;

	public static readonly DependencyProperty CapillarySensorBToMidDataContextProperty;

	public static readonly DependencyProperty MidToValveBLinkDataContextProperty;

	public static readonly DependencyProperty CapillaryMidToValveBDataContextProperty;

	public static readonly DependencyProperty ValveBToOutputLinkDataContextProperty;

	public static readonly DependencyProperty CapillaryValveBToOutputDataContextProperty;

	public static readonly DependencyProperty SensorBToMidLinkDataContextProperty;

	public static readonly DependencyProperty WasteAtoValveALinkDataContextProperty;

	public static readonly DependencyProperty BlingPlugToValveCLinkDataContextProperty;

	public static readonly DependencyProperty ValveAToPlugLinkDataContextProperty;

	public static readonly DependencyProperty MixTeeToInjectionValveLinkDataContextProperty;

	public static readonly DependencyProperty InjectionValveToSeparatorLinkDataContextProperty;

	public static readonly DependencyProperty ValveAPlugDataContextDataContextProperty;

	public static readonly DependencyProperty ValveBPlugDataContextDataContextProperty;

	public static readonly DependencyProperty OutputDataContextProperty;

	public static readonly DependencyProperty LoopDataContextProperty;

	public static readonly DependencyProperty CapillaryLoopDataContextProperty;

	public static readonly DependencyProperty TrapDataContextProperty;

	public static readonly DependencyProperty CapillaryTrapDataContextProperty;

	public static readonly DependencyProperty LineBreakOnPumpALink1DataContextProperty;

	public static readonly DependencyProperty LineBreakOnPumpALink2DataContextProperty;

	public static readonly DependencyProperty TemperatureDataContextProperty;

	[Description("Gets or sets the TrapDataContext.")]
	[Category("Diagram Control Properties")]
	public object TrapDataContext
	{
		get
		{
			return GetValue(TrapDataContextProperty);
		}
		set
		{
			SetValue(TrapDataContextProperty, value);
		}
	}

	[Description("Gets or sets the CapillaryTrapDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillaryTrapDataContext
	{
		get
		{
			return GetValue(CapillaryTrapDataContextProperty);
		}
		set
		{
			SetValue(CapillaryTrapDataContextProperty, value);
		}
	}

	[Description("Gets or sets the data context of the loop control.")]
	[Category("Diagram Control Properties")]
	public object LoopDataContext
	{
		get
		{
			return GetValue(LoopDataContextProperty);
		}
		set
		{
			SetValue(LoopDataContextProperty, value);
		}
	}

	[Description("Gets or sets the balloon data context of the loop control.")]
	[Category("Diagram Control Properties")]
	public object CapillaryLoopDataContext
	{
		get
		{
			return GetValue(CapillaryLoopDataContextProperty);
		}
		set
		{
			SetValue(CapillaryLoopDataContextProperty, value);
		}
	}

	[Description("Gets or sets the SiblingHasError Property.")]
	[Category("Diagram Control Properties")]
	public bool SiblingHasError
	{
		get
		{
			return (bool)GetValue(SiblingHasErrorProperty);
		}
		set
		{
			SetValue(SiblingHasErrorProperty, value);
		}
	}

	[Description("Gets or sets the PumpADataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpADataContext
	{
		get
		{
			return GetValue(PumpADataContextProperty);
		}
		set
		{
			SetValue(PumpADataContextProperty, value);
		}
	}

	[Description("Gets or sets the PumpBDataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpBDataContext
	{
		get
		{
			return GetValue(PumpBDataContextProperty);
		}
		set
		{
			SetValue(PumpBDataContextProperty, value);
		}
	}

	[Description("Gets or sets the PumpLogDataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpLogDataContext
	{
		get
		{
			return GetValue(PumpLogDataContextProperty);
		}
		set
		{
			SetValue(PumpLogDataContextProperty, value);
		}
	}

	[Description("Gets or sets the SolventADataContext.")]
	[Category("Diagram Control Properties")]
	public object SolventADataContext
	{
		get
		{
			return GetValue(SolventADataContextProperty);
		}
		set
		{
			SetValue(SolventADataContextProperty, value);
		}
	}

	[Description("Gets or sets the SolventBDataContext.")]
	[Category("Diagram Control Properties")]
	public object SolventBDataContext
	{
		get
		{
			return GetValue(SolventBDataContextProperty);
		}
		set
		{
			SetValue(SolventBDataContextProperty, value);
		}
	}

	[Description("Gets or sets the SensorADataContext.")]
	[Category("Diagram Control Properties")]
	public object SensorADataContext
	{
		get
		{
			return GetValue(SensorADataContextProperty);
		}
		set
		{
			SetValue(SensorADataContextProperty, value);
		}
	}

	[Description("Gets or sets the SensorBDataContext.")]
	[Category("Diagram Control Properties")]
	public object SensorBDataContext
	{
		get
		{
			return GetValue(SensorBDataContextProperty);
		}
		set
		{
			SetValue(SensorBDataContextProperty, value);
		}
	}

	[Description("Gets or sets the ValveADataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveADataContext
	{
		get
		{
			return GetValue(ValveADataContextProperty);
		}
		set
		{
			SetValue(ValveADataContextProperty, value);
		}
	}

	[Description("Gets or sets the ValveBDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveBDataContext
	{
		get
		{
			return GetValue(ValveBDataContextProperty);
		}
		set
		{
			SetValue(ValveBDataContextProperty, value);
		}
	}

	[Description("Gets or sets the ValveCDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveCDataContext
	{
		get
		{
			return GetValue(ValveCDataContextProperty);
		}
		set
		{
			SetValue(ValveCDataContextProperty, value);
		}
	}

	[Description("Gets or sets the ValveDDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveDDataContext
	{
		get
		{
			return GetValue(ValveDDataContextProperty);
		}
		set
		{
			SetValue(ValveDDataContextProperty, value);
		}
	}

	[Description("Gets or sets the WasteADataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteADataContext
	{
		get
		{
			return GetValue(WasteADataContextProperty);
		}
		set
		{
			SetValue(WasteADataContextProperty, value);
		}
	}

	[Description("Gets or sets the WasteBDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteBDataContext
	{
		get
		{
			return GetValue(WasteBDataContextProperty);
		}
		set
		{
			SetValue(WasteBDataContextProperty, value);
		}
	}

	[Description("Gets or sets the WasteCDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteCDataContext
	{
		get
		{
			return GetValue(WasteCDataContextProperty);
		}
		set
		{
			SetValue(WasteCDataContextProperty, value);
		}
	}

	[Description("Gets or sets the WasteDDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteDDataContext
	{
		get
		{
			return GetValue(WasteDDataContextProperty);
		}
		set
		{
			SetValue(WasteDDataContextProperty, value);
		}
	}

	[Description("Gets or sets the OutputDataContext.")]
	[Category("Diagram Control Properties")]
	public object OutputDataContext
	{
		get
		{
			return GetValue(OutputDataContextProperty);
		}
		set
		{
			SetValue(OutputDataContextProperty, value);
		}
	}

	[Description("Gets or sets the InjectionDataContext.")]
	[Category("Diagram Control Properties")]
	public object InjectionDataContext
	{
		get
		{
			return GetValue(InjectionDataContextProperty);
		}
		set
		{
			SetValue(InjectionDataContextProperty, value);
		}
	}

	[Description("Gets or sets the BlindPlugDataContext.")]
	[Category("Diagram Control Properties")]
	public object BlindPlugDataContext
	{
		get
		{
			return GetValue(BlindPlugDataContextProperty);
		}
		set
		{
			SetValue(BlindPlugDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the PumpAToValveDTopLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpAToValveDTopLinkDataContext
	{
		get
		{
			return GetValue(PumpAToValveDTopLinkDataContextProperty);
		}
		set
		{
			SetValue(PumpAToValveDTopLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the PumpAToValveDBottomLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpAToValveDBottomLinkDataContext
	{
		get
		{
			return GetValue(PumpAToValveDBottomLinkDataContextProperty);
		}
		set
		{
			SetValue(PumpAToValveDBottomLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the SolventAtoValveDLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object SolventAtoValveDLinkDataContext
	{
		get
		{
			return GetValue(SolventAtoValveDLinkDataContextProperty);
		}
		set
		{
			SetValue(SolventAtoValveDLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the WasteDtoValveDLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteDtoValveDLinkDataContext
	{
		get
		{
			return GetValue(WasteDtoValveDLinkDataContextProperty);
		}
		set
		{
			SetValue(WasteDtoValveDLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the SensorAtoValveDLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object SensorAtoValveDLinkDataContext
	{
		get
		{
			return GetValue(SensorAtoValveDLinkDataContextProperty);
		}
		set
		{
			SetValue(SensorAtoValveDLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillarySensorAtoValveDDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillarySensorAtoValveDDataContext
	{
		get
		{
			return GetValue(CapillarySensorAtoValveDDataContextProperty);
		}
		set
		{
			SetValue(CapillarySensorAtoValveDDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the SensorBtoValveCLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object SensorBtoValveCLinkDataContext
	{
		get
		{
			return GetValue(SensorBtoValveCLinkDataContextProperty);
		}
		set
		{
			SetValue(SensorBtoValveCLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillarySensorBtoValveCDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillarySensorBtoValveCDataContext
	{
		get
		{
			return GetValue(CapillarySensorBtoValveCDataContextProperty);
		}
		set
		{
			SetValue(CapillarySensorBtoValveCDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the PumpBToValveCBottomLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpBToValveCBottomLinkDataContext
	{
		get
		{
			return GetValue(PumpBToValveCBottomLinkDataContextProperty);
		}
		set
		{
			SetValue(PumpBToValveCBottomLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the PumpBtoValveCTopLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object PumpBtoValveCTopLinkDataContext
	{
		get
		{
			return GetValue(PumpBtoValveCTopLinkDataContextProperty);
		}
		set
		{
			SetValue(PumpBtoValveCTopLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the SolventBtoValveCTopLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object SolventBtoValveCTopLinkDataContext
	{
		get
		{
			return GetValue(SolventBtoValveCTopLinkDataContextProperty);
		}
		set
		{
			SetValue(SolventBtoValveCTopLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the WasteCtoValveCLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteCtoValveCLinkDataContext
	{
		get
		{
			return GetValue(WasteCtoValveCLinkDataContextProperty);
		}
		set
		{
			SetValue(WasteCtoValveCLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the WasteBtoValveBLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteBtoValveBLinkDataContext
	{
		get
		{
			return GetValue(WasteBtoValveBLinkDataContextProperty);
		}
		set
		{
			SetValue(WasteBtoValveBLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the InjectionToValveALinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object InjectionToValveALinkDataContext
	{
		get
		{
			return GetValue(InjectionToValveALinkDataContextProperty);
		}
		set
		{
			SetValue(InjectionToValveALinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the ValveDToValveALinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveDToValveALinkDataContext
	{
		get
		{
			return GetValue(ValveDToValveALinkDataContextProperty);
		}
		set
		{
			SetValue(ValveDToValveALinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillaryValveDToValveDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillaryValveDToValveADataContext
	{
		get
		{
			return GetValue(CapillaryValveDToValveADataContextProperty);
		}
		set
		{
			SetValue(CapillaryValveDToValveADataContextProperty, value);
		}
	}

	[Description(" Gets or sets the ValveAToValveBLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveAToValveBLinkDataContext
	{
		get
		{
			return GetValue(ValveAToValveBLinkDataContextProperty);
		}
		set
		{
			SetValue(ValveAToValveBLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the SensorAToMidLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object SensorAToMidLinkDataContext
	{
		get
		{
			return GetValue(SensorAToMidLinkDataContextProperty);
		}
		set
		{
			SetValue(SensorAToMidLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillarySensorAToMidDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillarySensorAToMidDataContext
	{
		get
		{
			return GetValue(CapillarySensorAToMidDataContextProperty);
		}
		set
		{
			SetValue(CapillarySensorAToMidDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillarySensorBToMidDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillarySensorBToMidDataContext
	{
		get
		{
			return GetValue(CapillarySensorBToMidDataContextProperty);
		}
		set
		{
			SetValue(CapillarySensorBToMidDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the MidToValveBLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object MidToValveBLinkDataContext
	{
		get
		{
			return GetValue(MidToValveBLinkDataContextProperty);
		}
		set
		{
			SetValue(MidToValveBLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillaryMidToValveBDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillaryMidToValveBDataContext
	{
		get
		{
			return GetValue(CapillaryMidToValveBDataContextProperty);
		}
		set
		{
			SetValue(CapillaryMidToValveBDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the ValveBToOutputLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveBToOutputLinkDataContext
	{
		get
		{
			return GetValue(ValveBToOutputLinkDataContextProperty);
		}
		set
		{
			SetValue(ValveBToOutputLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the CapillaryValveBToOutputDataContext.")]
	[Category("Diagram Control Properties")]
	public object CapillaryValveBToOutputDataContext
	{
		get
		{
			return GetValue(CapillaryValveBToOutputDataContextProperty);
		}
		set
		{
			SetValue(CapillaryValveBToOutputDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the SensorBToMidLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object SensorBToMidLinkDataContext
	{
		get
		{
			return GetValue(SensorBToMidLinkDataContextProperty);
		}
		set
		{
			SetValue(SensorBToMidLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the WasteAtoValveALinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object WasteAtoValveALinkDataContext
	{
		get
		{
			return GetValue(WasteAtoValveALinkDataContextProperty);
		}
		set
		{
			SetValue(WasteAtoValveALinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the BlingPlugToValveCLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object BlingPlugToValveCLinkDataContext
	{
		get
		{
			return GetValue(BlingPlugToValveCLinkDataContextProperty);
		}
		set
		{
			SetValue(BlingPlugToValveCLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the ValveAToPlugLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveAToPlugLinkDataContext
	{
		get
		{
			return GetValue(ValveAToPlugLinkDataContextProperty);
		}
		set
		{
			SetValue(ValveAToPlugLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the MixTeeToInjectionValveLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object MixTeeToInjectionValveLinkDataContext
	{
		get
		{
			return GetValue(MixTeeToInjectionValveLinkDataContextProperty);
		}
		set
		{
			SetValue(MixTeeToInjectionValveLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the InjectionValveToSeparatorLinkDataContext.")]
	[Category("Diagram Control Properties")]
	public object InjectionValveToSeparatorLinkDataContext
	{
		get
		{
			return GetValue(InjectionValveToSeparatorLinkDataContextProperty);
		}
		set
		{
			SetValue(InjectionValveToSeparatorLinkDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the ValveAPlugDataContextDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveAPlugDataContextDataContext
	{
		get
		{
			return GetValue(ValveAPlugDataContextDataContextProperty);
		}
		set
		{
			SetValue(ValveAPlugDataContextDataContextProperty, value);
		}
	}

	[Description(" Gets or sets the ValveBPlugDataContextDataContext.")]
	[Category("Diagram Control Properties")]
	public object ValveBPlugDataContextDataContext
	{
		get
		{
			return GetValue(ValveBPlugDataContextDataContextProperty);
		}
		set
		{
			SetValue(ValveBPlugDataContextDataContextProperty, value);
		}
	}

	[Description("Gets or sets the LineBreakOnPumpALink1DataContext.")]
	[Category("Diagram Control Properties")]
	public object LineBreakOnPumpALink1DataContext
	{
		get
		{
			return GetValue(LineBreakOnPumpALink1DataContextProperty);
		}
		set
		{
			SetValue(LineBreakOnPumpALink1DataContextProperty, value);
		}
	}

	[Description("Gets or sets the LineBreakOnPumpALink1DataContext.")]
	[Category("Diagram Control Properties")]
	public object LineBreakOnPumpALink2DataContext
	{
		get
		{
			return GetValue(LineBreakOnPumpALink2DataContextProperty);
		}
		set
		{
			SetValue(LineBreakOnPumpALink2DataContextProperty, value);
		}
	}

	[Description("Gets or sets the TemperatureDataContext.")]
	[Category("Diagram Control Properties")]
	public object TemperatureDataContext
	{
		get
		{
			return GetValue(TemperatureDataContextProperty);
		}
		set
		{
			SetValue(TemperatureDataContextProperty, value);
		}
	}

	static DiagramControl()
	{
		SiblingHasErrorProperty = DependencyProperty.Register("SiblingHasError", typeof(bool), typeof(DiagramControl), new FrameworkPropertyMetadata(false));
		PumpADataContextProperty = DependencyProperty.Register("PumpADataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		PumpBDataContextProperty = DependencyProperty.Register("PumpBDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		PumpLogDataContextProperty = DependencyProperty.Register("PumpLogDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SolventADataContextProperty = DependencyProperty.Register("SolventADataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SolventBDataContextProperty = DependencyProperty.Register("SolventBDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SensorADataContextProperty = DependencyProperty.Register("SensorADataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SensorBDataContextProperty = DependencyProperty.Register("SensorBDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveADataContextProperty = DependencyProperty.Register("ValveADataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveBDataContextProperty = DependencyProperty.Register("ValveBDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveCDataContextProperty = DependencyProperty.Register("ValveCDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveDDataContextProperty = DependencyProperty.Register("ValveDDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteADataContextProperty = DependencyProperty.Register("WasteADataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteBDataContextProperty = DependencyProperty.Register("WasteBDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteCDataContextProperty = DependencyProperty.Register("WasteCDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteDDataContextProperty = DependencyProperty.Register("WasteDDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		InjectionDataContextProperty = DependencyProperty.Register("InjectionDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		BlindPlugDataContextProperty = DependencyProperty.Register("BlindPlugDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		PumpAToValveDTopLinkDataContextProperty = DependencyProperty.Register("PumpAToValveDTopLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		PumpAToValveDBottomLinkDataContextProperty = DependencyProperty.Register("PumpAToValveDBottomLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SolventAtoValveDLinkDataContextProperty = DependencyProperty.Register("SolventAtoValveDLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteDtoValveDLinkDataContextProperty = DependencyProperty.Register("WasteDtoValveDLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SensorAtoValveDLinkDataContextProperty = DependencyProperty.Register("SensorAtoValveDLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillarySensorAtoValveDDataContextProperty = DependencyProperty.Register("CapillarySensorAtoValveDDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SensorBtoValveCLinkDataContextProperty = DependencyProperty.Register("SensorBtoValveCLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillarySensorBtoValveCDataContextProperty = DependencyProperty.Register("CapillarySensorBtoValveCDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		PumpBToValveCBottomLinkDataContextProperty = DependencyProperty.Register("PumpBToValveCBottomLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		PumpBtoValveCTopLinkDataContextProperty = DependencyProperty.Register("PumpBtoValveCTopLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SolventBtoValveCTopLinkDataContextProperty = DependencyProperty.Register("SolventBtoValveCTopLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteCtoValveCLinkDataContextProperty = DependencyProperty.Register("WasteCtoValveCLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteBtoValveBLinkDataContextProperty = DependencyProperty.Register("WasteBtoValveBLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		InjectionToValveALinkDataContextProperty = DependencyProperty.Register("InjectionToValveALinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveDToValveALinkDataContextProperty = DependencyProperty.Register("ValveDToValveALinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillaryValveDToValveADataContextProperty = DependencyProperty.Register("CapillaryValveDToValveADataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveAToValveBLinkDataContextProperty = DependencyProperty.Register("ValveAToValveBLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SensorAToMidLinkDataContextProperty = DependencyProperty.Register("SensorAToMidLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillarySensorAToMidDataContextProperty = DependencyProperty.Register("CapillarySensorAToMidDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillarySensorBToMidDataContextProperty = DependencyProperty.Register("CapillarySensorBToMidDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		MidToValveBLinkDataContextProperty = DependencyProperty.Register("MidToValveBLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillaryMidToValveBDataContextProperty = DependencyProperty.Register("CapillaryMidToValveBDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveBToOutputLinkDataContextProperty = DependencyProperty.Register("ValveBToOutputLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillaryValveBToOutputDataContextProperty = DependencyProperty.Register("CapillaryValveBToOutputDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		SensorBToMidLinkDataContextProperty = DependencyProperty.Register("SensorBToMidLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		WasteAtoValveALinkDataContextProperty = DependencyProperty.Register("WasteAtoValveALinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		BlingPlugToValveCLinkDataContextProperty = DependencyProperty.Register("BlingPlugToValveCLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveAToPlugLinkDataContextProperty = DependencyProperty.Register("ValveAToPlugLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		MixTeeToInjectionValveLinkDataContextProperty = DependencyProperty.Register("MixTeeToInjectionValveLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		InjectionValveToSeparatorLinkDataContextProperty = DependencyProperty.Register("InjectionValveToSeparatorLinkDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveAPlugDataContextDataContextProperty = DependencyProperty.Register("ValveAPlugDataContextDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		ValveBPlugDataContextDataContextProperty = DependencyProperty.Register("ValveBPlugDataContextDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		OutputDataContextProperty = DependencyProperty.Register("OutputDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		LoopDataContextProperty = DependencyProperty.Register("LoopDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillaryLoopDataContextProperty = DependencyProperty.Register("CapillaryLoopDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		TrapDataContextProperty = DependencyProperty.Register("TrapDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		CapillaryTrapDataContextProperty = DependencyProperty.Register("CapillaryTrapDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		LineBreakOnPumpALink1DataContextProperty = DependencyProperty.Register("LineBreakOnPumpALink1DataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		LineBreakOnPumpALink2DataContextProperty = DependencyProperty.Register("LineBreakOnPumpALink2DataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		TemperatureDataContextProperty = DependencyProperty.Register("TemperatureDataContext", typeof(object), typeof(DiagramControl), new FrameworkPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramControl), new FrameworkPropertyMetadata(typeof(DiagramControl)));
	}

	public DiagramControl()
	{
		APValidation.AddErrorStatusChangedHandler(this, OnErrorStatusChanged);
	}

	private void OnErrorStatusChanged(object sender, RoutedEventArgs e)
	{
		if (e is ErrorStatusChangedEventArgs errorStatusChangedEventArgs)
		{
			HandleError(errorStatusChangedEventArgs.Guid, errorStatusChangedEventArgs.ErrorMessage, errorStatusChangedEventArgs.HasError);
		}
	}

	private void HandleError(string childId, string errorMessage, bool hasError)
	{
		if (hasError && !_errorDict.ContainsKey(childId))
		{
			_errorDict.Add(childId, errorMessage);
		}
		else
		{
			_errorDict.Remove(childId);
		}
		SiblingHasError = _errorDict.Count > 0;
	}
}
