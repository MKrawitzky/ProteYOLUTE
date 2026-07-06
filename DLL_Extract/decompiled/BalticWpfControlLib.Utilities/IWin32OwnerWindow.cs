// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Windows.Interop;

namespace BalticWpfControlLib.Utilities;

public interface IWin32OwnerWindow : IWin32Window
{
	bool? IsMaximized { get; }
}
