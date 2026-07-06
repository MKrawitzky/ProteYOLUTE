// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Windows;
using System.Windows.Media;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000080 RID: 128
	public class ArrowLine : ArrowLineBase
	{
		// Token: 0x17000152 RID: 338
		// (get) Token: 0x060005FC RID: 1532 RVA: 0x00039D14 File Offset: 0x00037F14
		// (set) Token: 0x060005FB RID: 1531 RVA: 0x00039D01 File Offset: 0x00037F01
		public double X1
		{
			get
			{
				return (double)base.GetValue(ArrowLine.X1Property);
			}
			set
			{
				base.SetValue(ArrowLine.X1Property, value);
			}
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x00039D39 File Offset: 0x00037F39
		// (set) Token: 0x060005FD RID: 1533 RVA: 0x00039D26 File Offset: 0x00037F26
		public double Y1
		{
			get
			{
				return (double)base.GetValue(ArrowLine.Y1Property);
			}
			set
			{
				base.SetValue(ArrowLine.Y1Property, value);
			}
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000600 RID: 1536 RVA: 0x00039D5E File Offset: 0x00037F5E
		// (set) Token: 0x060005FF RID: 1535 RVA: 0x00039D4B File Offset: 0x00037F4B
		public double X2
		{
			get
			{
				return (double)base.GetValue(ArrowLine.X2Property);
			}
			set
			{
				base.SetValue(ArrowLine.X2Property, value);
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000602 RID: 1538 RVA: 0x00039D83 File Offset: 0x00037F83
		// (set) Token: 0x06000601 RID: 1537 RVA: 0x00039D70 File Offset: 0x00037F70
		public double Y2
		{
			get
			{
				return (double)base.GetValue(ArrowLine.Y2Property);
			}
			set
			{
				base.SetValue(ArrowLine.Y2Property, value);
			}
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000603 RID: 1539 RVA: 0x00039D98 File Offset: 0x00037F98
		protected override Geometry DefiningGeometry
		{
			get
			{
				this.pathgeo.Figures.Clear();
				this.pathfigLine.StartPoint = new Point(this.X1, this.Y1);
				this.polysegLine.Points.Clear();
				this.polysegLine.Points.Add(new Point(this.X2, this.Y2));
				this.pathgeo.Figures.Add(this.pathfigLine);
				return base.DefiningGeometry;
			}
		}

		// Token: 0x0400032B RID: 811
		public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(ArrowLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

		// Token: 0x0400032C RID: 812
		public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(ArrowLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

		// Token: 0x0400032D RID: 813
		public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(ArrowLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

		// Token: 0x0400032E RID: 814
		public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(ArrowLine), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
	}
}
