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
	// Token: 0x02000082 RID: 130
	public class ArrowPolyline : ArrowLineBase
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000613 RID: 1555 RVA: 0x0003A2C3 File Offset: 0x000384C3
		// (set) Token: 0x06000612 RID: 1554 RVA: 0x0003A2B5 File Offset: 0x000384B5
		public PointCollection Points
		{
			get
			{
				return (PointCollection)base.GetValue(ArrowPolyline.PointsProperty);
			}
			set
			{
				base.SetValue(ArrowPolyline.PointsProperty, value);
			}
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x0003A2D5 File Offset: 0x000384D5
		public ArrowPolyline()
		{
			this.Points = new PointCollection();
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000615 RID: 1557 RVA: 0x0003A2E8 File Offset: 0x000384E8
		protected override Geometry DefiningGeometry
		{
			get
			{
				this.pathgeo.Figures.Clear();
				if (this.Points.Count > 0)
				{
					this.pathfigLine.StartPoint = this.Points[0];
					this.polysegLine.Points.Clear();
					for (int i = 1; i < this.Points.Count; i++)
					{
						this.polysegLine.Points.Add(this.Points[i]);
					}
					this.pathgeo.Figures.Add(this.pathfigLine);
				}
				return base.DefiningGeometry;
			}
		}

		// Token: 0x0400033A RID: 826
		public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(PointCollection), typeof(ArrowPolyline), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));
	}
}
