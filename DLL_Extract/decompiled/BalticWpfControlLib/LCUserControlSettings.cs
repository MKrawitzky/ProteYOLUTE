// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace BalticWpfControlLib;

[Serializable]
public class LCUserControlSettings
{
	public List<string> EnabledTracesList { get; set; }

	public bool IsDiagnosticTracesSelected { get; set; }

	public string ChartGridWidthLeft { get; set; }

	public string ChartGridWidthRight { get; set; }

	public string MainGridHeightTopRow { get; set; }

	public string MainGridHeightBottomRow { get; set; }
}
