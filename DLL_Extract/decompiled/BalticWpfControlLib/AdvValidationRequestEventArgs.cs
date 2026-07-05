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
