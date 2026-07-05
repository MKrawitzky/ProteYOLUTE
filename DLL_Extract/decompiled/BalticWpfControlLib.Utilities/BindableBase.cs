using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BalticWpfControlLib.Utilities;

public abstract class BindableBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
	{
		return SetField(ref storage, value, propertyName);
	}
}
