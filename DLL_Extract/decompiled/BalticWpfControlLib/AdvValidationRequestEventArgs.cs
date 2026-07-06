// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Collections.Generic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvValidationRequestEventArgs : ScriptControlEventArgs
{
	private readonly List<ProcedureReportEventArgs> _reports = new List<ProcedureReportEventArgs>();

	public IEnumerable<ProcedureReportEventArgs> Reports => _reports;

	public AdvValidationRequestEventArgs(ProcedureInfo procedure, ProcedureArguments arguments, ChildProcedureArguments childArguments)
		: base(procedure, arguments, childArguments)
	{
	}

	public void AddReport(ProcedureReportEventArgs report)
	{
		_reports.Add(report);
	}
}
