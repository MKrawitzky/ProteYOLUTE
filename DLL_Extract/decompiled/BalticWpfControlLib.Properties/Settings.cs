using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace BalticWpfControlLib.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.12.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[UserScopedSetting]
	[DebuggerNonUserCode]
	public LCUserControlSettings LCUserControlV2
	{
		get
		{
			return (LCUserControlSettings)this["LCUserControlV2"];
		}
		set
		{
			this["LCUserControlV2"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("900, 567")]
	public Size LCControlSize
	{
		get
		{
			return (Size)this["LCControlSize"];
		}
		set
		{
			this["LCControlSize"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("0, 0")]
	public Point LCControlPos
	{
		get
		{
			return (Point)this["LCControlPos"];
		}
		set
		{
			this["LCControlPos"] = value;
		}
	}
}
