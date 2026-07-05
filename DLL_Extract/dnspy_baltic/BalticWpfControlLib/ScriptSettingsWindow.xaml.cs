using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bruker.Lc.Business;
using Microsoft.CSharp.RuntimeBinder;

namespace BalticWpfControlLib
{
	// Token: 0x0200003A RID: 58
	public partial class ScriptSettingsWindow : Window
	{
		// Auto-generated callsite cache class
		private static class _co_14
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x0600034E RID: 846 RVA: 0x00015C6B File Offset: 0x00013E6B
		public string Description { get; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x0600034F RID: 847 RVA: 0x00015C73 File Offset: 0x00013E73
		public ProcedureParameterControl ParameterControl { get; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x06000350 RID: 848 RVA: 0x00015C7B File Offset: 0x00013E7B
		// (set) Token: 0x06000351 RID: 849 RVA: 0x00015C83 File Offset: 0x00013E83
		public bool IsApplyActive { get; set; }

		// Token: 0x14000037 RID: 55
		// (add) Token: 0x06000352 RID: 850 RVA: 0x00015C8C File Offset: 0x00013E8C
		// (remove) Token: 0x06000353 RID: 851 RVA: 0x00015CC4 File Offset: 0x00013EC4
		public event ScriptSettingsWindow.ApplySettingsDelegate ApplySettingsEvent;

		// Token: 0x06000354 RID: 852 RVA: 0x00015CFC File Offset: 0x00013EFC
		public ScriptSettingsWindow(ProcedureInfo info, ProcedureArguments presets, ChildProcedureArguments childPresets, string privatePath, dynamic balticSettings, bool isService, bool isApplySettings = false)
		{
			if (ScriptSettingsWindow._co_14._cp_1 == null)
			{
				ScriptSettingsWindow._co_14._cp_1 = CallSite<Func<CallSite, Type, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Combine", null, typeof(ScriptSettingsWindow), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Func<CallSite, Type, string, object, object> target = ScriptSettingsWindow._co_14._cp_1.Target;
			CallSite _cpl = ScriptSettingsWindow._co_14._cp_1;
			Type typeFromHandle = typeof(Path);
			if (ScriptSettingsWindow._co_14._cp_0 == null)
			{
				ScriptSettingsWindow._co_14._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "TooltipImageDirectoryName", typeof(ScriptSettingsWindow), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			object tooltipDirectory = target(_cpl, typeFromHandle, privatePath, ScriptSettingsWindow._co_14._cp_0.Target(ScriptSettingsWindow._co_14._cp_0, balticSettings));
			this.InitializeComponent();
			this.Description = info.Description;
			this.ParameterControl = new ProcedureParameterControl(isService);
			if (ScriptSettingsWindow._co_14._cp_2 == null)
			{
				ScriptSettingsWindow._co_14._cp_2 = CallSite<Action<CallSite, ProcedureParameterControl, IEnumerable<ProcedureParameter>, IEnumerable<ChildProcedureParameter>, object, ProcedureArguments, ChildProcedureArguments>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetParameters", null, typeof(ScriptSettingsWindow), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
				}));
			}
			ScriptSettingsWindow._co_14._cp_2.Target(ScriptSettingsWindow._co_14._cp_2, this.ParameterControl, info.Parameters, info.AdvChildParameters, tooltipDirectory, presets, childPresets);
			this.IsApplyActive = info.IsApplyActive;
			base.DataContext = this;
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00006398 File Offset: 0x00004598
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00010170 File Offset: 0x0000E370
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(false);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x00015E90 File Offset: 0x00014090
		private void btnApply_Click(object sender, RoutedEventArgs e)
		{
			ValueTuple<ProcedureArguments, ChildProcedureArguments> parameters = this.ParameterControl.GetParameters();
			ProcedureArguments theProcArgs = parameters.Item1;
			ChildProcedureArguments theChildProcArgs = parameters.Item2;
			if (theProcArgs.Count > 0)
			{
				ScriptSettingsWindow.ApplySettingsDelegate applySettingsEvent = this.ApplySettingsEvent;
				if (applySettingsEvent == null)
				{
					return;
				}
				applySettingsEvent(theProcArgs, theChildProcArgs);
			}
		}

		// Token: 0x02000101 RID: 257
		// (Invoke) Token: 0x060007BE RID: 1982
		public delegate void ApplySettingsDelegate(ProcedureArguments arguments, ChildProcedureArguments childArguments);
	}
}
