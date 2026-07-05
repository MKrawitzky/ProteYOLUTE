using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Microsoft.CSharp.RuntimeBinder;

namespace BalticWpfControlLib
{
	// Token: 0x0200003C RID: 60
	public partial class ScriptUserControl : UserControl
	{
		// Auto-generated callsite cache class
		private static class _co_27
		{
			public static dynamic _cp_0;
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600035E RID: 862 RVA: 0x00015FF8 File Offset: 0x000141F8
		// (set) Token: 0x0600035F RID: 863 RVA: 0x00016000 File Offset: 0x00014200
		public ScriptSettingsWindow SettingsDlg { get; private set; }

		// Token: 0x14000038 RID: 56
		// (add) Token: 0x06000360 RID: 864 RVA: 0x0001600C File Offset: 0x0001420C
		// (remove) Token: 0x06000361 RID: 865 RVA: 0x00016044 File Offset: 0x00014244
		public event ScriptUserControl.ScriptControlEventHandler OnScriptActionClick;

		// Token: 0x14000039 RID: 57
		// (add) Token: 0x06000362 RID: 866 RVA: 0x0001607C File Offset: 0x0001427C
		// (remove) Token: 0x06000363 RID: 867 RVA: 0x000160B4 File Offset: 0x000142B4
		public event ScriptUserControl.ScriptControlEventHandler OnScriptApplyClick;

		// Token: 0x1400003A RID: 58
		// (add) Token: 0x06000364 RID: 868 RVA: 0x000160EC File Offset: 0x000142EC
		// (remove) Token: 0x06000365 RID: 869 RVA: 0x00016124 File Offset: 0x00014324
		public event EventHandler<ScriptValidationRequestEventArgs> ScriptValidationRequest;

		// Token: 0x1400003B RID: 59
		// (add) Token: 0x06000366 RID: 870 RVA: 0x0001615C File Offset: 0x0001435C
		// (remove) Token: 0x06000367 RID: 871 RVA: 0x00016194 File Offset: 0x00014394
		public event EventHandler<ScriptControlEventArgs> ScriptArgumentPresetRequest;

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000368 RID: 872 RVA: 0x000161C9 File Offset: 0x000143C9
		public ProcedureInfo Info { get; }

		// Token: 0x06000369 RID: 873 RVA: 0x000161D4 File Offset: 0x000143D4
		public ScriptUserControl(ProcedureInfo pInfo, Window parentWindow, string privatePath, dynamic balticSettings, BalticInstrumentFacade instrument)
		{
			this.InitializeComponent();
			this.Info = pInfo;
			this._ownerWindow = parentWindow;
			this._privatePath = privatePath;
			this._balticSettings = balticSettings;
			this._instrument = instrument;
			this.tbScriptName.Text = this.Info.Name;
			this.imgScriptImage.Source = (ImageSource)new MyImageConverter().Convert("Images/play_24_mba.png");
			this.imgScriptSettings.Source = (ImageSource)new MyImageConverter().Convert(pInfo.Parameters.Any<ProcedureParameter>() ? "Images/settings_24.png" : "Images/settingsDisabled_24.png");
			base.Margin = new Thickness(4.0, 4.0, 4.0, 4.0);
			base.ToolTip = this.Info.Description;
			this.btnSettings.IsEnabled = pInfo.Parameters.Any<ProcedureParameter>();
		}

		// Token: 0x0600036A RID: 874 RVA: 0x000162D0 File Offset: 0x000144D0
		private void ScriptButton_Click(object sender, RoutedEventArgs e)
		{
			this.ScriptAction(false);
		}

		// Token: 0x0600036B RID: 875 RVA: 0x000162D9 File Offset: 0x000144D9
		private void ScriptSettingsButton_Click(object sender, RoutedEventArgs e)
		{
			this.ScriptAction(true);
		}

		// Token: 0x0600036C RID: 876 RVA: 0x000162E4 File Offset: 0x000144E4
		public void ScriptAction(bool forceShowSettings)
		{
			ProcedureArguments args = this.Info.CreateArguments();
			ChildProcedureArguments advArgs = this.Info.CreateAdvancedChildArguments();
			if (this.ScriptArgumentPresetRequest != null)
			{
				ScriptControlEventArgs e = new ScriptControlEventArgs(this.Info, args, null);
				this.ScriptArgumentPresetRequest(this, e);
			}
			ProcedureReportEventArgs[] reports = this.RequestValidation(args, null).ToArray<ProcedureReportEventArgs>();
			if (forceShowSettings || reports.Length != 0)
			{
				if (ScriptUserControl._co_27._cp_0 == null)
				{
					ScriptUserControl._co_27._cp_0 = CallSite<Func<CallSite, Type, ProcedureInfo, ProcedureArguments, ChildProcedureArguments, string, object, bool, ScriptSettingsWindow>>.Create(Binder.InvokeConstructor(CSharpBinderFlags.None, typeof(ScriptUserControl), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				this.SettingsDlg = ScriptUserControl._co_27._cp_0.Target(ScriptUserControl._co_27._cp_0, typeof(ScriptSettingsWindow), this.Info, args, advArgs, this._privatePath, this._balticSettings, BalticInstrumentFacade.IsService);
				this.SettingsDlg.ApplySettingsEvent += this.Dlg_ApplySettingsEvent;
				this.SettingsDlg.Owner = this._ownerWindow;
				ScriptUserControl.HandleReports(this.SettingsDlg.ParameterControl, reports);
				this.SettingsDlg.ParameterControl.ArgumentValueUpdated += this.ParameterControl_ArgumentValueUpdated;
				this.SettingsDlg.ShowDialog();
				if (!this.SettingsDlg.DialogResult.GetValueOrDefault())
				{
					this.SettingsDlg = null;
					return;
				}
				args = this.SettingsDlg.ParameterControl.CreateArguments();
				advArgs = ProcedureParameterControl.CreateChildArguments();
				this.SettingsDlg = null;
			}
			ScriptControlEventArgs scriptControlEventArgs = new ScriptControlEventArgs(this.Info, args, advArgs);
			ScriptUserControl.ScriptControlEventHandler onScriptActionClick = this.OnScriptActionClick;
			if (onScriptActionClick == null)
			{
				return;
			}
			onScriptActionClick(this, scriptControlEventArgs);
		}

		// Token: 0x0600036D RID: 877 RVA: 0x000164B0 File Offset: 0x000146B0
		private void Dlg_ApplySettingsEvent(ProcedureArguments arguments, ChildProcedureArguments childArguments)
		{
			ScriptControlEventArgs scriptControlEventArgs = new ScriptControlEventArgs(this.Info, arguments, childArguments);
			ScriptUserControl.ScriptControlEventHandler onScriptApplyClick = this.OnScriptApplyClick;
			if (onScriptApplyClick == null)
			{
				return;
			}
			onScriptApplyClick(this, scriptControlEventArgs);
		}

		// Token: 0x0600036E RID: 878 RVA: 0x000164E0 File Offset: 0x000146E0
		private void ParameterControl_ArgumentValueUpdated(object sender, EventArgs e)
		{
			ProcedureParameterControl ppc = sender as ProcedureParameterControl;
			IEnumerable<ProcedureReportEventArgs> reports = this.RequestValidation(ppc.CreateArguments(), ProcedureParameterControl.CreateChildArguments());
			ScriptUserControl.HandleReports(ppc, reports);
		}

		// Token: 0x0600036F RID: 879 RVA: 0x00016510 File Offset: 0x00014710
		private static void HandleReports(ProcedureParameterControl ppc, IEnumerable<ProcedureReportEventArgs> reports)
		{
			ppc.ClearErrors();
			foreach (ProcedureReportEventArgs report in reports)
			{
				ppc.SetError(report.Subject, report.Message);
			}
		}

		// Token: 0x06000370 RID: 880 RVA: 0x0001656C File Offset: 0x0001476C
		private IEnumerable<ProcedureReportEventArgs> RequestValidation(ProcedureArguments args, ChildProcedureArguments childArgs = null)
		{
			if (this.ScriptValidationRequest == null)
			{
				return new List<ProcedureReportEventArgs>();
			}
			ScriptValidationRequestEventArgs e = new ScriptValidationRequestEventArgs(this.Info, args, childArgs);
			this.ScriptValidationRequest(this, e);
			return e.Reports;
		}

		// Token: 0x040001FE RID: 510
		private readonly Window _ownerWindow;

		// Token: 0x040001FF RID: 511
		private readonly string _privatePath;

		// Token: 0x04000200 RID: 512
		private readonly dynamic _balticSettings;

		// Token: 0x04000201 RID: 513
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x02000103 RID: 259
		// (Invoke) Token: 0x060007C2 RID: 1986
		public delegate void ScriptControlEventHandler(object sender, ScriptControlEventArgs e);
	}
}
