using System.Collections.ObjectModel;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

internal class ProcParam : BaseProcParam
{
	public ObservableCollection<ChildProcParam> ChildProcParams { get; set; } = new ObservableCollection<ChildProcParam>();


	public ProcParam(ProcedureParameter parameter, string imagePath, bool isAppService, ObservableCollection<ChildProcParam> childProcParams)
		: base(parameter, imagePath, isAppService)
	{
		foreach (ChildProcParam childProcParam in childProcParams)
		{
			ChildProcParams.Add(new ChildProcParam(childProcParam));
		}
	}

	public ProcParam(ProcedureParameter parameter, ProcedureArgument argument, string imagePath, bool isAppService, ObservableCollection<ChildProcParam> childProcParams)
		: base(parameter, argument, imagePath, isAppService)
	{
		foreach (ChildProcParam childProcParam in childProcParams)
		{
			ChildProcParams.Add(new ChildProcParam(childProcParam));
		}
	}
}
