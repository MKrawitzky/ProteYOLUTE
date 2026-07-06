// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000054 RID: 84
	public sealed class RoundPolyline : Shape
	{
		// Token: 0x0600047E RID: 1150 RVA: 0x0001A338 File Offset: 0x00018538
		private void DefineGeometry()
		{
			PointCollection points = this.Points;
			if (points == null)
			{
				this._geometry = Geometry.Empty;
				return;
			}
			PathFigure figure = new PathFigure();
			if (points.Count > 0)
			{
				figure.StartPoint = points[0];
				if (points.Count > 1)
				{
					double desiredRadius = this.Radius;
					for (int i = 1; i < points.Count - 1; i++)
					{
						Vector v = points[i] - points[i - 1];
						Vector v2 = points[i + 1] - points[i];
						double radius = Math.Min(Math.Min(v.Length, v2.Length) / 2.0, desiredRadius);
						double len = v.Length;
						v.Normalize();
						v *= len - radius;
						LineSegment line = new LineSegment(points[i - 1] + v, true);
						figure.Segments.Add(line);
						v2.Normalize();
						v2 *= radius;
						SweepDirection direction = ((Vector.AngleBetween(v, v2) > 0.0) ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
						ArcSegment arc = new ArcSegment(points[i] + v2, new Size(radius, radius), 0.0, false, direction, true);
						figure.Segments.Add(arc);
					}
					figure.Segments.Add(new LineSegment(points[points.Count - 1], true));
				}
			}
			PathGeometry geometry = new PathGeometry();
			geometry.Figures.Add(figure);
			geometry.FillRule = this.FillRule;
			this._geometry = ((geometry.Bounds == Rect.Empty) ? Geometry.Empty : geometry);
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x0001A507 File Offset: 0x00018707
		protected override Size MeasureOverride(Size constraint)
		{
			this.DefineGeometry();
			return base.MeasureOverride(constraint);
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x06000480 RID: 1152 RVA: 0x0001A516 File Offset: 0x00018716
		protected override Geometry DefiningGeometry
		{
			get
			{
				return this._geometry;
			}
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x06000481 RID: 1153 RVA: 0x0001A51E File Offset: 0x0001871E
		// (set) Token: 0x06000482 RID: 1154 RVA: 0x0001A530 File Offset: 0x00018730
		public double Radius
		{
			get
			{
				return (double)base.GetValue(RoundPolyline.RadiusProperty);
			}
			set
			{
				base.SetValue(RoundPolyline.RadiusProperty, value);
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000483 RID: 1155 RVA: 0x0001A543 File Offset: 0x00018743
		// (set) Token: 0x06000484 RID: 1156 RVA: 0x0001A555 File Offset: 0x00018755
		public FillRule FillRule
		{
			get
			{
				return (FillRule)base.GetValue(RoundPolyline.FillRuleProperty);
			}
			set
			{
				base.SetValue(RoundPolyline.FillRuleProperty, value);
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x0001A568 File Offset: 0x00018768
		// (set) Token: 0x06000486 RID: 1158 RVA: 0x0001A57A File Offset: 0x0001877A
		public PointCollection Points
		{
			get
			{
				return (PointCollection)base.GetValue(RoundPolyline.PointsProperty);
			}
			set
			{
				base.SetValue(RoundPolyline.PointsProperty, value);
			}
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x0001A588 File Offset: 0x00018788
		private static object GetEmptyPointCollection()
		{
			if (RoundPolyline._freezableDefaultValueFactoryCtor == null)
			{
				RoundPolyline._emptyPointCollection = (PointCollection)typeof(PointCollection).GetProperty("Empty", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
				RoundPolyline._freezableDefaultValueFactoryCtor = typeof(DependencyObject).Assembly.GetType("MS.Internal.FreezableDefaultValueFactory").GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
			}
			return RoundPolyline._freezableDefaultValueFactoryCtor.Invoke(new object[] { RoundPolyline._emptyPointCollection });
		}

		// Token: 0x04000286 RID: 646
		public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register("FillRule", typeof(FillRule), typeof(RoundPolyline), new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender));

		// Token: 0x04000287 RID: 647
		public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(PointCollection), typeof(RoundPolyline), new FrameworkPropertyMetadata(RoundPolyline.GetEmptyPointCollection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		// Token: 0x04000288 RID: 648
		public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(RoundPolyline), new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

		// Token: 0x04000289 RID: 649
		private Geometry _geometry;

		// Token: 0x0400028A RID: 650
		private static PointCollection _emptyPointCollection;

		// Token: 0x0400028B RID: 651
		private static ConstructorInfo _freezableDefaultValueFactoryCtor;
	}
}
