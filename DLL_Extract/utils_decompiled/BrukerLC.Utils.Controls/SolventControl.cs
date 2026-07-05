using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class SolventControl : Control
{
	static SolventControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SolventControl), new FrameworkPropertyMetadata(typeof(SolventControl)));
	}
}
