using Bruker.Lc.Business;

namespace BalticWpfControlLib;

internal class ChildProcParam : BaseProcParam
{
	public string Header { get; }

	public ChildProcParam(string header, ProcedureParameter parameter, string imagePath, bool isAppService)
		: base(parameter, imagePath, isAppService)
	{
		Header = header;
	}

	public ChildProcParam(string header, ProcedureParameter parameter, ProcedureArgument argument, string imagePath, bool isAppService)
		: base(parameter, argument, imagePath, isAppService)
	{
		Header = header;
	}

	public ChildProcParam(ChildProcParam item)
		: base(item)
	{
		Header = item.Header;
	}
}
