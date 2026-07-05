using System.Windows.Interop;

namespace BalticWpfControlLib.Utilities;

public interface IWin32OwnerWindow : IWin32Window
{
	bool? IsMaximized { get; }
}
