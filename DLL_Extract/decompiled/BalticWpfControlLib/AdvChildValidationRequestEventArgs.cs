using System.Collections.Generic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvChildValidationRequestEventArgs : ScriptControlEventArgs
{
	private readonly List<ProcedureReportEventArgs> _reports = new List<ProcedureReportEventArgs>();

	public IEnumerable<ProcedureReportEventArgs> Reports => _reports;

	public AdvChildValidationRequestEventArgs(ProcedureInfo procedure, ProcedureArguments arguments, ChildProcedureArguments childArguments)
		: base(procedure, arguments, childArguments)
	{
	}

	public void AddReport(ProcedureReportEventArgs report)
	{
		_reports.Add(report);
	}
}
