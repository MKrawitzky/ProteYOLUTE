// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BalticWpfControlLib.Controls;

public abstract class ArrowLineBase : Shape
{
	protected PathGeometry pathgeo;

	protected PathFigure pathfigLine;

	protected PolyLineSegment polysegLine;

	private PathFigure pathfigHead1;

	private PolyLineSegment polysegHead1;

	private PathFigure pathfigHead2;

	private PolyLineSegment polysegHead2;

	public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register("ArrowAngle", typeof(double), typeof(ArrowLineBase), new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

	public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register("ArrowLength", typeof(double), typeof(ArrowLineBase), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

	public static readonly DependencyProperty ArrowEndsProperty = DependencyProperty.Register("ArrowEnds", typeof(ArrowEnds), typeof(ArrowLineBase), new FrameworkPropertyMetadata(ArrowEnds.End, FrameworkPropertyMetadataOptions.AffectsMeasure));

	public static readonly DependencyProperty IsArrowClosedProperty = DependencyProperty.Register("IsArrowClosed", typeof(bool), typeof(ArrowLineBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

	public double ArrowAngle
	{
		get
		{
			return (double)GetValue(ArrowAngleProperty);
		}
		set
		{
			SetValue(ArrowAngleProperty, value);
		}
	}

	public double ArrowLength
	{
		get
		{
			return (double)GetValue(ArrowLengthProperty);
		}
		set
		{
			SetValue(ArrowLengthProperty, value);
		}
	}

	public ArrowEnds ArrowEnds
	{
		get
		{
			return (ArrowEnds)GetValue(ArrowEndsProperty);
		}
		set
		{
			SetValue(ArrowEndsProperty, value);
		}
	}

	public bool IsArrowClosed
	{
		get
		{
			return (bool)GetValue(IsArrowClosedProperty);
		}
		set
		{
			SetValue(IsArrowClosedProperty, value);
		}
	}

	protected override Geometry DefiningGeometry
	{
		get
		{
			int count = polysegLine.Points.Count;
			if (count > 0)
			{
				if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
				{
					Point startPoint = pathfigLine.StartPoint;
					Point pt = polysegLine.Points[0];
					pathgeo.Figures.Add(CalculateArrow(pathfigHead1, pt, startPoint));
				}
				if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
				{
					Point pt2 = ((count == 1) ? pathfigLine.StartPoint : polysegLine.Points[count - 2]);
					Point pt3 = polysegLine.Points[count - 1];
					pathgeo.Figures.Add(CalculateArrow(pathfigHead2, pt2, pt3));
				}
			}
			return pathgeo;
		}
	}

	public ArrowLineBase()
	{
		pathgeo = new PathGeometry();
		pathfigLine = new PathFigure();
		polysegLine = new PolyLineSegment();
		pathfigLine.Segments.Add(polysegLine);
		pathfigHead1 = new PathFigure();
		polysegHead1 = new PolyLineSegment();
		pathfigHead1.Segments.Add(polysegHead1);
		pathfigHead2 = new PathFigure();
		polysegHead2 = new PolyLineSegment();
		pathfigHead2.Segments.Add(polysegHead2);
	}

	private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
	{
		Matrix matrix = default(Matrix);
		Vector vector = pt1 - pt2;
		vector.Normalize();
		vector *= ArrowLength;
		PolyLineSegment obj = pathfig.Segments[0] as PolyLineSegment;
		obj.Points.Clear();
		matrix.Rotate(ArrowAngle / 2.0);
		pathfig.StartPoint = pt2 + vector * matrix;
		obj.Points.Add(pt2);
		matrix.Rotate(0.0 - ArrowAngle);
		obj.Points.Add(pt2 + vector * matrix);
		pathfig.IsClosed = IsArrowClosed;
		return pathfig;
	}
}
