// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib;

public class SystemConditionUserControl : UserControl, IComponentConnector
{
	private SystemCondition _condition;

	internal TextBlock Raised;

	internal TextBlock Subject;

	internal TextBlock Severity;

	internal TextBlock Description;

	internal Button Confirm;

	internal Button Close;

	private bool _contentLoaded;

	public SystemCondition SystemCondition
	{
		get
		{
			return _condition;
		}
		set
		{
			_condition = value;
			Update();
		}
	}

	public SystemConditionUserControl()
	{
		InitializeComponent();
	}

	private void Update()
	{
		Raised.Text = _condition.Raised.ToShortDateString();
		Subject.Text = _condition.Subject;
		Description.Text = _condition.Description;
		Severity.Text = _condition.Severity.ToString();
		Confirm.Visibility = ((!_condition.ManualDismiss) ? Visibility.Hidden : Visibility.Visible);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/systemconditionusercontrol.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			Raised = (TextBlock)target;
			break;
		case 2:
			Subject = (TextBlock)target;
			break;
		case 3:
			Severity = (TextBlock)target;
			break;
		case 4:
			Description = (TextBlock)target;
			break;
		case 5:
			Confirm = (Button)target;
			break;
		case 6:
			Close = (Button)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
