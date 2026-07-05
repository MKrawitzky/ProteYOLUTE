using System.Windows;

namespace BrukerLC.Utils.Events;

public class ErrorStatusChangedEventArgs : RoutedEventArgs
{
	public string Guid { get; private set; }

	public string ErrorMessage { get; private set; }

	public bool HasError { get; private set; }

	public ErrorStatusChangedEventArgs(RoutedEvent routedEvent, string guid, string errorMessage, bool hasError)
		: base(routedEvent)
	{
		Guid = guid;
		ErrorMessage = errorMessage;
		HasError = hasError;
	}
}
