using System.Windows.Media;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class ActivatableAndErrorAwareDataContext : BindableBase, IActivatable, IErrorAware, ISignalizeAware, ISignalizeTextAware, IVisibleEnableable, IToolTipable
{
	private bool _isActive;

	private Brush _activeBrush;

	private bool _isSignalize;

	private Brush _signalizeBrush;

	private bool _isSignalizeText;

	private Brush _signalizeTextBrush;

	private bool _isVisible = true;

	private bool _isNearTransparent;

	private bool _isToolTipVisible;

	public bool IsActive
	{
		get
		{
			if (_isActive && !_isSignalize)
			{
				return !_isSignalizeText;
			}
			return false;
		}
		set
		{
			SetProperty(ref _isActive, value, "IsActive");
		}
	}

	public Brush ActiveBrush
	{
		get
		{
			return _activeBrush;
		}
		set
		{
			SetProperty(ref _activeBrush, value, "ActiveBrush");
		}
	}

	private string Error
	{
		set
		{
			if (!object.Equals(ErrorMessage, value))
			{
				ErrorMessage = value;
				OnPropertyChanged("ErrorMessage");
				OnPropertyChanged("HasError");
			}
		}
	}

	public bool HasError => ErrorMessage != null;

	public string ErrorMessage { get; private set; }

	public bool IsSignalize
	{
		get
		{
			return _isSignalize;
		}
		set
		{
			_isSignalize = value;
			OnPropertyChanged("IsSignalize");
			OnPropertyChanged("IsActive");
		}
	}

	public Brush SignalizeBrush
	{
		get
		{
			return _signalizeBrush;
		}
		set
		{
			SetProperty(ref _signalizeBrush, value, "SignalizeBrush");
		}
	}

	public bool IsSignalizeText
	{
		get
		{
			return _isSignalizeText;
		}
		set
		{
			_isSignalizeText = value;
			OnPropertyChanged("IsSignalizeText");
			OnPropertyChanged("IsActive");
		}
	}

	public Brush SignalizeTextBrush
	{
		get
		{
			return _signalizeTextBrush;
		}
		set
		{
			SetProperty(ref _signalizeTextBrush, value, "SignalizeTextBrush");
		}
	}

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			SetProperty(ref _isVisible, value, "IsVisible");
		}
	}

	public bool IsNearTransparent
	{
		get
		{
			return _isNearTransparent;
		}
		set
		{
			SetProperty(ref _isNearTransparent, value, "IsNearTransparent");
		}
	}

	public bool IsToolTipVisible
	{
		get
		{
			return _isToolTipVisible;
		}
		set
		{
			SetProperty(ref _isToolTipVisible, value, "IsToolTipVisible");
		}
	}

	public ActivatableAndErrorAwareDataContext()
	{
	}

	public ActivatableAndErrorAwareDataContext(Brush activeBrush)
	{
		_activeBrush = activeBrush;
	}

	public void SetError(string errorMessage)
	{
		Error = errorMessage;
	}

	public void ClearError()
	{
		Error = null;
	}
}
