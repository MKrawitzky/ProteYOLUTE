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
