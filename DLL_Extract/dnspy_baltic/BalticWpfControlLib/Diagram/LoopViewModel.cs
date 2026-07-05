using System;
using System.Windows.Input;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000078 RID: 120
	public class LoopViewModel : ActivatableAndErrorAwareDataContext, ILoopViewModel
	{
		// Token: 0x17000119 RID: 281
		// (get) Token: 0x0600057E RID: 1406 RVA: 0x0003939B File Offset: 0x0003759B
		// (set) Token: 0x0600057F RID: 1407 RVA: 0x000393A3 File Offset: 0x000375A3
		public string Header { get; set; }

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000580 RID: 1408 RVA: 0x000393AC File Offset: 0x000375AC
		// (set) Token: 0x06000581 RID: 1409 RVA: 0x000393B4 File Offset: 0x000375B4
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

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000582 RID: 1410 RVA: 0x000393C8 File Offset: 0x000375C8
		// (set) Token: 0x06000583 RID: 1411 RVA: 0x000393D0 File Offset: 0x000375D0
		public string LengthUnit { get; set; }

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000584 RID: 1412 RVA: 0x000393D9 File Offset: 0x000375D9
		// (set) Token: 0x06000585 RID: 1413 RVA: 0x000393E1 File Offset: 0x000375E1
		public double MinLength { get; set; }

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x06000586 RID: 1414 RVA: 0x000393EA File Offset: 0x000375EA
		// (set) Token: 0x06000587 RID: 1415 RVA: 0x000393F2 File Offset: 0x000375F2
		public double MaxLength { get; set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000588 RID: 1416 RVA: 0x000393FB File Offset: 0x000375FB
		// (set) Token: 0x06000589 RID: 1417 RVA: 0x00039403 File Offset: 0x00037603
		public double DefaultLength { get; set; }

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x0600058A RID: 1418 RVA: 0x0003940C File Offset: 0x0003760C
		// (set) Token: 0x0600058B RID: 1419 RVA: 0x00039414 File Offset: 0x00037614
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

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600058C RID: 1420 RVA: 0x00039428 File Offset: 0x00037628
		// (set) Token: 0x0600058D RID: 1421 RVA: 0x00039430 File Offset: 0x00037630
		public double MinID { get; set; }

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600058E RID: 1422 RVA: 0x00039439 File Offset: 0x00037639
		// (set) Token: 0x0600058F RID: 1423 RVA: 0x00039441 File Offset: 0x00037641
		public double MaxID { get; set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000590 RID: 1424 RVA: 0x0003944A File Offset: 0x0003764A
		// (set) Token: 0x06000591 RID: 1425 RVA: 0x00039452 File Offset: 0x00037652
		public double DefaultID { get; set; }

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000592 RID: 1426 RVA: 0x0003945B File Offset: 0x0003765B
		// (set) Token: 0x06000593 RID: 1427 RVA: 0x00039463 File Offset: 0x00037663
		public string IDUnit { get; set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000594 RID: 1428 RVA: 0x0003946C File Offset: 0x0003766C
		public bool IsEditable { get; }

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000595 RID: 1429 RVA: 0x00039474 File Offset: 0x00037674
		public bool IsPopupVisible { get; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000596 RID: 1430 RVA: 0x0003947C File Offset: 0x0003767C
		// (set) Token: 0x06000597 RID: 1431 RVA: 0x00039484 File Offset: 0x00037684
		public double FactoryLength { get; set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000598 RID: 1432 RVA: 0x0003948D File Offset: 0x0003768D
		// (set) Token: 0x06000599 RID: 1433 RVA: 0x00039495 File Offset: 0x00037695
		public double FactoryID { get; set; }

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x0600059A RID: 1434 RVA: 0x0003949E File Offset: 0x0003769E
		public ICommand EditLinkCommand { get; }

		// Token: 0x0600059B RID: 1435 RVA: 0x000036D0 File Offset: 0x000018D0
		public bool CanEditLink(object obj)
		{
			return true;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x000394A8 File Offset: 0x000376A8
		public void EditLink(object obj)
		{
			string header = obj as string;
			EditCapillaryWindow dlg = new EditCapillaryWindow(this._pref);
			dlg.Title = "Edit " + header;
			dlg.Vm.LengthTitle = this._pref.LengthTitle;
			dlg.Vm.InnerDiameterTitle = this._pref.InnerDiameterTitle;
			dlg.Vm.Length = this._hardwareCapillary.Length;
			dlg.Vm.ID = this._hardwareCapillary.ID;
			if (dlg.ShowDialog(HelperExtensions.GetActiveWindow()).GetValueOrDefault())
			{
				this.Length = (this._hardwareCapillary.Length = dlg.Vm.Length);
				this.ID = (this._hardwareCapillary.ID = dlg.Vm.ID);
			}
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x00039584 File Offset: 0x00037784
		public LoopViewModel(BalticHWProfile.CapillaryItem capillary, BalticPreferences.CapillaryPreference pref, bool isEditable)
		{
			this._hardwareCapillary = capillary;
			this.IsEditable = isEditable;
			this._pref = pref;
			this.Header = capillary.Header;
			this._length = capillary.Length;
			this.LengthUnit = ((pref.LengthUnit == "uL") ? "µm" : pref.LengthUnit);
			this.MinLength = pref.MinLength;
			this.MaxLength = pref.MaxLength;
			this.DefaultLength = pref.DefaultLength;
			this._id = capillary.ID;
			this.IDUnit = ((pref.IDUnit == "uL") ? "µL" : pref.IDUnit);
			this.MinID = pref.MinID;
			this.MaxID = pref.MaxID;
			this.DefaultID = pref.DefaultID;
			this.FactoryLength = pref.FactoryLength;
			this.FactoryID = pref.FactoryID;
			this.EditLinkCommand = new RelayCommand(new Action<object>(this.EditLink), new Predicate<object>(this.CanEditLink));
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x0003969D File Offset: 0x0003789D
		public void Revert()
		{
			this.Length = this.DefaultLength;
			this.ID = this.DefaultID;
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x000396B7 File Offset: 0x000378B7
		public void RevertToFactory()
		{
			this.Length = this.FactoryLength;
			this.ID = this.FactoryID;
		}

		// Token: 0x040002EE RID: 750
		private double _length;

		// Token: 0x040002EF RID: 751
		private double _id;

		// Token: 0x040002F0 RID: 752
		private readonly BalticHWProfile.CapillaryItem _hardwareCapillary;

		// Token: 0x040002F1 RID: 753
		private readonly BalticPreferences.CapillaryPreference _pref;
	}
}
