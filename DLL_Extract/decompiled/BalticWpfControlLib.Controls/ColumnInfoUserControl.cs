using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticClassLib;

namespace BalticWpfControlLib.Controls;

public class ColumnInfoUserControl : UserControl, IComponentConnector
{
	private const string PROPERTY_CATEGORY = "ColumnInfoUserControl Properties";

	public static readonly DependencyProperty ColumnSourceProperty = DependencyProperty.Register("ColumnSource", typeof(Column), typeof(ColumnInfoUserControl), new FrameworkPropertyMetadata(null));

	private bool _contentLoaded;

	[Description("Gets or sets the ColumnSource.")]
	[Category("ColumnInfoUserControl Properties")]
	public Column ColumnSource
	{
		get
		{
			return (Column)GetValue(ColumnSourceProperty);
		}
		set
		{
			SetValue(ColumnSourceProperty, value);
		}
	}

	public ColumnInfoUserControl()
	{
		InitializeComponent();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/controls/columninfousercontrol.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		_contentLoaded = true;
	}
}
