// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000081 RID: 129
	public abstract class ArrowLineBase : Shape
	{
		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000607 RID: 1543 RVA: 0x00039F24 File Offset: 0x00038124
		// (set) Token: 0x06000606 RID: 1542 RVA: 0x00039F11 File Offset: 0x00038111
		public double ArrowAngle
		{
			get
			{
				return (double)base.GetValue(ArrowLineBase.ArrowAngleProperty);
			}
			set
			{
				base.SetValue(ArrowLineBase.ArrowAngleProperty, value);
			}
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000609 RID: 1545 RVA: 0x00039F49 File Offset: 0x00038149
		// (set) Token: 0x06000608 RID: 1544 RVA: 0x00039F36 File Offset: 0x00038136
		public double ArrowLength
		{
			get
			{
				return (double)base.GetValue(ArrowLineBase.ArrowLengthProperty);
			}
			set
			{
				base.SetValue(ArrowLineBase.ArrowLengthProperty, value);
			}
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x0600060B RID: 1547 RVA: 0x00039F6E File Offset: 0x0003816E
		// (set) Token: 0x0600060A RID: 1546 RVA: 0x00039F5B File Offset: 0x0003815B
		public ArrowEnds ArrowEnds
		{
			get
			{
				return (ArrowEnds)base.GetValue(ArrowLineBase.ArrowEndsProperty);
			}
			set
			{
				base.SetValue(ArrowLineBase.ArrowEndsProperty, value);
			}
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x0600060D RID: 1549 RVA: 0x00039F93 File Offset: 0x00038193
		// (set) Token: 0x0600060C RID: 1548 RVA: 0x00039F80 File Offset: 0x00038180
		public bool IsArrowClosed
		{
			get
			{
				return (bool)base.GetValue(ArrowLineBase.IsArrowClosedProperty);
			}
			set
			{
				base.SetValue(ArrowLineBase.IsArrowClosedProperty, value);
			}
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x00039FA8 File Offset: 0x000381A8
		public ArrowLineBase()
		{
			this.pathgeo = new PathGeometry();
			this.pathfigLine = new PathFigure();
			this.polysegLine = new PolyLineSegment();
			this.pathfigLine.Segments.Add(this.polysegLine);
			this.pathfigHead1 = new PathFigure();
			this.polysegHead1 = new PolyLineSegment();
			this.pathfigHead1.Segments.Add(this.polysegHead1);
			this.pathfigHead2 = new PathFigure();
			this.polysegHead2 = new PolyLineSegment();
			this.pathfigHead2.Segments.Add(this.polysegHead2);
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600060F RID: 1551 RVA: 0x0003A04C File Offset: 0x0003824C
		protected override Geometry DefiningGeometry
		{
			get
			{
				int count = this.polysegLine.Points.Count;
				if (count > 0)
				{
					if ((this.ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
					{
						Point pt = this.pathfigLine.StartPoint;
						Point pt2 = this.polysegLine.Points[0];
						this.pathgeo.Figures.Add(this.CalculateArrow(this.pathfigHead1, pt2, pt));
					}
					if ((this.ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
					{
						Point pt3 = ((count == 1) ? this.pathfigLine.StartPoint : this.polysegLine.Points[count - 2]);
						Point pt4 = this.polysegLine.Points[count - 1];
						this.pathgeo.Figures.Add(this.CalculateArrow(this.pathfigHead2, pt3, pt4));
					}
				}
				return this.pathgeo;
			}
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x0003A124 File Offset: 0x00038324
		private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
		{
			Matrix matx = default(Matrix);
			Vector vect = pt1 - pt2;
			vect.Normalize();
			vect *= this.ArrowLength;
			PolyLineSegment polyLineSegment = pathfig.Segments[0] as PolyLineSegment;
			polyLineSegment.Points.Clear();
			matx.Rotate(this.ArrowAngle / 2.0);
			pathfig.StartPoint = pt2 + vect * matx;
			polyLineSegment.Points.Add(pt2);
			matx.Rotate(-this.ArrowAngle);
			polyLineSegment.Points.Add(pt2 + vect * matx);
			pathfig.IsClosed = this.IsArrowClosed;
			return pathfig;
		}

		// Token: 0x0400032F RID: 815
		protected PathGeometry pathgeo;

		// Token: 0x04000330 RID: 816
		protected PathFigure pathfigLine;

		// Token: 0x04000331 RID: 817
		protected PolyLineSegment polysegLine;

		// Token: 0x04000332 RID: 818
		private PathFigure pathfigHead1;

		// Token: 0x04000333 RID: 819
		private PolyLineSegment polysegHead1;

		// Token: 0x04000334 RID: 820
		private PathFigure pathfigHead2;

		// Token: 0x04000335 RID: 821
		private PolyLineSegment polysegHead2;

		// Token: 0x04000336 RID: 822
		public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register("ArrowAngle", typeof(double), typeof(ArrowLineBase), new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

		// Token: 0x04000337 RID: 823
		public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register("ArrowLength", typeof(double), typeof(ArrowLineBase), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

		// Token: 0x04000338 RID: 824
		public static readonly DependencyProperty ArrowEndsProperty = DependencyProperty.Register("ArrowEnds", typeof(ArrowEnds), typeof(ArrowLineBase), new FrameworkPropertyMetadata(ArrowEnds.End, FrameworkPropertyMetadataOptions.AffectsMeasure));

		// Token: 0x04000339 RID: 825
		public static readonly DependencyProperty IsArrowClosedProperty = DependencyProperty.Register("IsArrowClosed", typeof(bool), typeof(ArrowLineBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));
	}
}
