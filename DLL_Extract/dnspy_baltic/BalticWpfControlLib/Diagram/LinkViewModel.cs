using System;
using System.Windows.Input;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000077 RID: 119
	public class LinkViewModel : ActivatableAndErrorAwareDataContext, ILinkViewModel
	{
		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06000557 RID: 1367 RVA: 0x00038E9D File Offset: 0x0003709D
		// (set) Token: 0x06000558 RID: 1368 RVA: 0x00038EA5 File Offset: 0x000370A5
		public string Header { get; set; } = "unknown";

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06000559 RID: 1369 RVA: 0x00038EAE File Offset: 0x000370AE
		// (set) Token: 0x0600055A RID: 1370 RVA: 0x00038EB6 File Offset: 0x000370B6
		public double Length
		{
			get
			{
				return this._length;
			}
			set
			{
				this._length = value;
				this.OnPropertyChanged("Length");
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600055B RID: 1371 RVA: 0x00038ECA File Offset: 0x000370CA
		// (set) Token: 0x0600055C RID: 1372 RVA: 0x00038ED2 File Offset: 0x000370D2
		public string LengthUnit { get; set; } = "mm";

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x0600055D RID: 1373 RVA: 0x00038EDB File Offset: 0x000370DB
		// (set) Token: 0x0600055E RID: 1374 RVA: 0x00038EE3 File Offset: 0x000370E3
		public double MinLength { get; set; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x0600055F RID: 1375 RVA: 0x00038EEC File Offset: 0x000370EC
		// (set) Token: 0x06000560 RID: 1376 RVA: 0x00038EF4 File Offset: 0x000370F4
		public double MaxLength { get; set; } = 1000.0;

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000561 RID: 1377 RVA: 0x00038EFD File Offset: 0x000370FD
		// (set) Token: 0x06000562 RID: 1378 RVA: 0x00038F05 File Offset: 0x00037105
		public double DefaultLength { get; set; } = 1.0;

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000563 RID: 1379 RVA: 0x00038F0E File Offset: 0x0003710E
		// (set) Token: 0x06000564 RID: 1380 RVA: 0x00038F16 File Offset: 0x00037116
		public double ID
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
				this.OnPropertyChanged("ID");
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000565 RID: 1381 RVA: 0x00038F2A File Offset: 0x0003712A
		// (set) Token: 0x06000566 RID: 1382 RVA: 0x00038F32 File Offset: 0x00037132
		public double MinID { get; set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000567 RID: 1383 RVA: 0x00038F3B File Offset: 0x0003713B
		// (set) Token: 0x06000568 RID: 1384 RVA: 0x00038F43 File Offset: 0x00037143
		public double MaxID { get; set; } = 1000.0;

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000569 RID: 1385 RVA: 0x00038F4C File Offset: 0x0003714C
		// (set) Token: 0x0600056A RID: 1386 RVA: 0x00038F54 File Offset: 0x00037154
		public double DefaultID { get; set; } = 1.0;

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x0600056B RID: 1387 RVA: 0x00038F5D File Offset: 0x0003715D
		// (set) Token: 0x0600056C RID: 1388 RVA: 0x00038F65 File Offset: 0x00037165
		public string IDUnit { get; set; } = "µm";

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x0600056D RID: 1389 RVA: 0x00038F6E File Offset: 0x0003716E
		// (set) Token: 0x0600056E RID: 1390 RVA: 0x00038F76 File Offset: 0x00037176
		public string LengthTitle { get; set; } = "Length:";

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x0600056F RID: 1391 RVA: 0x00038F7F File Offset: 0x0003717F
		// (set) Token: 0x06000570 RID: 1392 RVA: 0x00038F87 File Offset: 0x00037187
		public string InnerDiameterTitle { get; set; } = "Inner Diameter:";

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000571 RID: 1393 RVA: 0x00038F90 File Offset: 0x00037190
		public bool IsEditable { get; }

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000572 RID: 1394 RVA: 0x00038F98 File Offset: 0x00037198
		public bool IsPopupVisible { get; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000573 RID: 1395 RVA: 0x00038FA0 File Offset: 0x000371A0
		// (set) Token: 0x06000574 RID: 1396 RVA: 0x00038FA8 File Offset: 0x000371A8
		public double FactoryLength { get; set; } = 1.0;

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000575 RID: 1397 RVA: 0x00038FB1 File Offset: 0x000371B1
		// (set) Token: 0x06000576 RID: 1398 RVA: 0x00038FB9 File Offset: 0x000371B9
		public double FactoryID { get; set; } = 1.0;

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000577 RID: 1399 RVA: 0x00038FC2 File Offset: 0x000371C2
		public ICommand EditLinkCommand { get; }

		// Token: 0x06000578 RID: 1400 RVA: 0x000036D0 File Offset: 0x000018D0
		public bool CanEditLink(object obj)
		{
			return true;
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x00038FCC File Offset: 0x000371CC
		public void EditLink(object obj)
		{
			string header = obj as string;
			EditCapillaryWindow dlg = new EditCapillaryWindow(this._pref)
			{
				Title = "Edit " + header,
				Vm = 
				{
					Length = this.Length,
					ID = this.ID
				}
			};
			if (dlg.ShowDialog(HelperExtensions.GetActiveWindow()).GetValueOrDefault())
			{
				this.Length = dlg.Vm.Length;
				this.ID = dlg.Vm.ID;
			}
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x00039058 File Offset: 0x00037258
		public LinkViewModel()
		{
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0003911C File Offset: 0x0003731C
		public LinkViewModel(BalticHWProfile.CapillaryItem capillary, BalticPreferences.CapillaryPreference pref, bool isEditable, bool isPopupVisible)
		{
			this._hardwareCapillary = capillary;
			this.IsEditable = isEditable;
			this.IsPopupVisible = isPopupVisible;
			this._pref = pref;
			this.Header = capillary.Header;
			this._length = capillary.Length;
			this.LengthUnit = ((pref.LengthUnit == "um") ? "µm" : pref.LengthUnit);
			this.MinLength = pref.MinLength;
			this.MaxLength = pref.MaxLength;
			this.DefaultLength = pref.DefaultLength;
			this._id = capillary.ID;
			this.IDUnit = ((pref.IDUnit == "um") ? "µm" : pref.IDUnit);
			this.MinID = pref.MinID;
			this.MaxID = pref.MaxID;
			this.DefaultID = pref.DefaultID;
			this.LengthTitle = pref.LengthTitle;
			this.InnerDiameterTitle = pref.InnerDiameterTitle;
			this.FactoryLength = pref.FactoryLength;
			this.FactoryID = pref.FactoryID;
			this.EditLinkCommand = new RelayCommand(new Action<object>(this.EditLink), new Predicate<object>(this.CanEditLink));
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x00039304 File Offset: 0x00037504
		public void Revert()
		{
			this.Length = (this._pref.DefaultLength = this.DefaultLength);
			this.ID = (this._pref.DefaultID = this.DefaultID);
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x00039348 File Offset: 0x00037548
		public void RevertToFactory()
		{
			this.Length = (this.DefaultLength = (this._pref.DefaultLength = this.FactoryLength));
			this.ID = (this.DefaultID = (this._pref.DefaultID = this.FactoryID));
		}

		// Token: 0x040002DA RID: 730
		private double _length = 1.0;

		// Token: 0x040002DB RID: 731
		private double _id = 1.0;

		// Token: 0x040002DC RID: 732
		private BalticHWProfile.CapillaryItem _hardwareCapillary;

		// Token: 0x040002DD RID: 733
		private readonly BalticPreferences.CapillaryPreference _pref;
	}
}
