// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class ScriptControlEventArgs : EventArgs
{
	public ProcedureInfo ProcedureSourceInfo { get; set; }

	public ProcedureArguments ProcedureSourceArgs { get; set; }

	public ChildProcedureArguments ProcedureSourceChildArgs { get; set; }

	public ScriptControlEventArgs(ProcedureInfo pInfo, ProcedureArguments pArgs, ChildProcedureArguments pChildArgs = null)
	{
		ProcedureSourceInfo = pInfo;
		ProcedureSourceArgs = pArgs;
		ProcedureSourceChildArgs = pChildArgs;
	}
}
