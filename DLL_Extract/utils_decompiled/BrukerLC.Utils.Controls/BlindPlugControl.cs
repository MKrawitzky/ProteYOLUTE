using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class BlindPlugControl : Control
{
	static BlindPlugControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BlindPlugControl), new FrameworkPropertyMetadata(typeof(BlindPlugControl)));
	}
}
