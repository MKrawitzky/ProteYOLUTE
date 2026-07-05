using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class WasteControl : Control
{
	static WasteControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(WasteControl), new FrameworkPropertyMetadata(typeof(WasteControl)));
	}
}
