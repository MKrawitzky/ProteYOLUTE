// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

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
