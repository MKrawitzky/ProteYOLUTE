using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class LineBreakControl : Control
{
	static LineBreakControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LineBreakControl), new FrameworkPropertyMetadata(typeof(LineBreakControl)));
	}
}
