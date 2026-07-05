using System;
using System.Globalization;
using BalticClassLib;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000073 RID: 115
	public class EditCapillaryViewModel : NotificationObject
	{
		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000530 RID: 1328 RVA: 0x00038926 File Offset: 0x00036B26
		// (set) Token: 0x06000531 RID: 1329 RVA: 0x0003892E File Offset: 0x00036B2E
		public double Length
		{
			get
			{
				return this._length;
			}
			set
			{
				this._length = value;
				this.RaisePropertyChanged("Length");
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x06000532 RID: 1330 RVA: 0x00038942 File Offset: 0x00036B42
		// (set) Token: 0x06000533 RID: 1331 RVA: 0x0003894A File Offset: 0x00036B4A
		public string LengthUnit { get; set; }

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000534 RID: 1332 RVA: 0x00038953 File Offset: 0x00036B53
		// (set) Token: 0x06000535 RID: 1333 RVA: 0x0003895B File Offset: 0x00036B5B
		public double MinLength { get; set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x00038964 File Offset: 0x00036B64
		// (set) Token: 0x06000537 RID: 1335 RVA: 0x0003896C File Offset: 0x00036B6C
		public double MaxLength { get; set; }

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x00038975 File Offset: 0x00036B75
		// (set) Token: 0x06000539 RID: 1337 RVA: 0x0003897D File Offset: 0x00036B7D
		public double ID
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
				this.RaisePropertyChanged("ID");
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x0600053A RID: 1338 RVA: 0x00038991 File Offset: 0x00036B91
		// (set) Token: 0x0600053B RID: 1339 RVA: 0x00038999 File Offset: 0x00036B99
		public string IDUnit { get; set; }

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x0600053C RID: 1340 RVA: 0x000389A2 File Offset: 0x00036BA2
		// (set) Token: 0x0600053D RID: 1341 RVA: 0x000389AA File Offset: 0x00036BAA
		public double MinID { get; set; }

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x0600053E RID: 1342 RVA: 0x000389B3 File Offset: 0x00036BB3
		// (set) Token: 0x0600053F RID: 1343 RVA: 0x000389BB File Offset: 0x00036BBB
		public double MaxID { get; set; }

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000540 RID: 1344 RVA: 0x000389C4 File Offset: 0x00036BC4
		// (set) Token: 0x06000541 RID: 1345 RVA: 0x000389CC File Offset: 0x00036BCC
		public string LengthTitle { get; set; }

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000542 RID: 1346 RVA: 0x000389D5 File Offset: 0x00036BD5
		// (set) Token: 0x06000543 RID: 1347 RVA: 0x000389DD File Offset: 0x00036BDD
		public string InnerDiameterTitle { get; set; }

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000544 RID: 1348 RVA: 0x000389E6 File Offset: 0x00036BE6
		// (set) Token: 0x06000545 RID: 1349 RVA: 0x000389EE File Offset: 0x00036BEE
		public string LengthRangeToolTip { get; set; }

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x000389F7 File Offset: 0x00036BF7
		// (set) Token: 0x06000547 RID: 1351 RVA: 0x000389FF File Offset: 0x00036BFF
		public string IDRangeToolTip { get; set; }

		// Token: 0x06000548 RID: 1352 RVA: 0x00038A08 File Offset: 0x00036C08
		public EditCapillaryViewModel(BalticPreferences.CapillaryPreference pref)
		{
			this._length = pref.Length;
			this.LengthUnit = pref.LengthUnit;
			this._defaultLength = pref.DefaultLength;
			this.MinLength = pref.MinLength;
			this.MaxLength = pref.MaxLength;
			this._id = pref.ID;
			this.IDUnit = ((pref.IDUnit == "um") ? "µm" : pref.IDUnit);
			this._defaultID = pref.DefaultID;
			this.MinID = pref.MinID;
			this.MaxID = pref.MaxID;
			this.LengthTitle = pref.LengthTitle;
			this.InnerDiameterTitle = pref.InnerDiameterTitle;
			this.LengthRangeToolTip = string.Format(CultureInfo.InvariantCulture, "Valid range is {0} {1} - {2} {3}", new object[] { this.MinLength, this.LengthUnit, this.MaxLength, this.LengthUnit });
			this.IDRangeToolTip = string.Format(CultureInfo.InvariantCulture, "Valid range is {0} {1} - {2} {3}", new object[] { this.MinID, this.IDUnit, this.MaxID, this.IDUnit });
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x00038B56 File Offset: 0x00036D56
		public void Revert()
		{
			this.Length = this._defaultLength;
			this.ID = this._defaultID;
		}

		// Token: 0x040002C2 RID: 706
		private double _length;

		// Token: 0x040002C3 RID: 707
		private readonly double _defaultLength;

		// Token: 0x040002C4 RID: 708
		private double _id;

		// Token: 0x040002C5 RID: 709
		private readonly double _defaultID;
	}
}
