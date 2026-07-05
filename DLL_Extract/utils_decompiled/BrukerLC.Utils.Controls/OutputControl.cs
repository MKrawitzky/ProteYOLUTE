using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class OutputControl : Control
{
	static OutputControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(OutputControl), new FrameworkPropertyMetadata(typeof(OutputControl)));
	}
}
