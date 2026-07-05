using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class TrapControl : Control
{
	static TrapControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TrapControl), new FrameworkPropertyMetadata(typeof(TrapControl)));
	}
}
