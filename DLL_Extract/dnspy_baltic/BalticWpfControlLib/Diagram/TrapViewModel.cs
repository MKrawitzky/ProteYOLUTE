using System;
using System.Windows.Input;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200007E RID: 126
	public class TrapViewModel : ActivatableAndErrorAwareDataContext, ITrapViewModel
	{
		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060005CE RID: 1486 RVA: 0x00039953 File Offset: 0x00037B53
		// (set) Token: 0x060005CF RID: 1487 RVA: 0x0003995B File Offset: 0x00037B5B
		public string Header { get; set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060005D0 RID: 1488 RVA: 0x00039964 File Offset: 0x00037B64
		// (set) Token: 0x060005D1 RID: 1489 RVA: 0x0003996C File Offset: 0x00037B6C
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

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060005D2 RID: 1490 RVA: 0x00039980 File Offset: 0x00037B80
		// (set) Token: 0x060005D3 RID: 1491 RVA: 0x00039988 File Offset: 0x00037B88
		public string LengthUnit { get; set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060005D4 RID: 1492 RVA: 0x00039991 File Offset: 0x00037B91
		// (set) Token: 0x060005D5 RID: 1493 RVA: 0x00039999 File Offset: 0x00037B99
		public double MinLength { get; set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060005D6 RID: 1494 RVA: 0x000399A2 File Offset: 0x00037BA2
		// (set) Token: 0x060005D7 RID: 1495 RVA: 0x000399AA File Offset: 0x00037BAA
		public double MaxLength { get; set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060005D8 RID: 1496 RVA: 0x000399B3 File Offset: 0x00037BB3
		// (set) Token: 0x060005D9 RID: 1497 RVA: 0x000399BB File Offset: 0x00037BBB
		public double DefaultLength { get; set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x000399C4 File Offset: 0x00037BC4
		// (set) Token: 0x060005DB RID: 1499 RVA: 0x000399CC File Offset: 0x00037BCC
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

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x000399E0 File Offset: 0x00037BE0
		// (set) Token: 0x060005DD RID: 1501 RVA: 0x000399E8 File Offset: 0x00037BE8
		public double MinID { get; set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x000399F1 File Offset: 0x00037BF1
		// (set) Token: 0x060005DF RID: 1503 RVA: 0x000399F9 File Offset: 0x00037BF9
		public double MaxID { get; set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x060005E0 RID: 1504 RVA: 0x00039A02 File Offset: 0x00037C02
		// (set) Token: 0x060005E1 RID: 1505 RVA: 0x00039A0A File Offset: 0x00037C0A
		public double DefaultID { get; set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x060005E2 RID: 1506 RVA: 0x00039A13 File Offset: 0x00037C13
		// (set) Token: 0x060005E3 RID: 1507 RVA: 0x00039A1B File Offset: 0x00037C1B
		public string IDUnit { get; set; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x060005E4 RID: 1508 RVA: 0x00039A24 File Offset: 0x00037C24
		public bool IsEditable { get; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x060005E5 RID: 1509 RVA: 0x00039A2C File Offset: 0x00037C2C
		public bool IsPopupVisible { get; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x00039A34 File Offset: 0x00037C34
		// (set) Token: 0x060005E7 RID: 1511 RVA: 0x00039A3C File Offset: 0x00037C3C
		public double FactoryLength { get; set; }

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x060005E8 RID: 1512 RVA: 0x00039A45 File Offset: 0x00037C45
		// (set) Token: 0x060005E9 RID: 1513 RVA: 0x00039A4D File Offset: 0x00037C4D
		public double FactoryID { get; set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x060005EA RID: 1514 RVA: 0x00039A56 File Offset: 0x00037C56
		public ICommand EditLinkCommand { get; }

		// Token: 0x060005EB RID: 1515 RVA: 0x000036D0 File Offset: 0x000018D0
		public bool CanEditLink(object obj)
		{
			return true;
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00039A60 File Offset: 0x00037C60
		public void EditLink(object obj)
		{
			string header = obj as string;
			EditCapillaryWindow dlg = new EditCapillaryWindow(this._pref)
			{
				Title = "Edit " + header,
				Vm = 
				{
					Length = this._hardwareCapillary.Length,
					ID = this._hardwareCapillary.ID
				}
			};
			if (dlg.ShowDialog(HelperExtensions.GetActiveWindow()).GetValueOrDefault())
			{
				this.Length = (this._hardwareCapillary.Length = dlg.Vm.Length);
				this.ID = (this._hardwareCapillary.ID = dlg.Vm.ID);
			}
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00039B10 File Offset: 0x00037D10
		public TrapViewModel(BalticHWProfile.CapillaryItem capillary, BalticPreferences.CapillaryPreference pref, bool isEditable)
		{
			this._hardwareCapillary = capillary;
			this.IsEditable = isEditable;
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
			this.FactoryLength = pref.FactoryLength;
			this.FactoryID = pref.FactoryID;
			this.EditLinkCommand = new RelayCommand(new Action<object>(this.EditLink), new Predicate<object>(this.CanEditLink));
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00039C29 File Offset: 0x00037E29
		public void Revert()
		{
			this.Length = this.DefaultLength;
			this.ID = this.DefaultID;
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x00039C43 File Offset: 0x00037E43
		public void RevertToFactory()
		{
			this.Length = this.FactoryLength;
			this.ID = this.FactoryID;
		}

		// Token: 0x04000314 RID: 788
		private double _length;

		// Token: 0x04000315 RID: 789
		private double _id;

		// Token: 0x04000316 RID: 790
		private readonly BalticHWProfile.CapillaryItem _hardwareCapillary;

		// Token: 0x04000317 RID: 791
		private readonly BalticPreferences.CapillaryPreference _pref;
	}
}
