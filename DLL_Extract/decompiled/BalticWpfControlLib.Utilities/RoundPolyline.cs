using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BalticWpfControlLib.Utilities;

public sealed class RoundPolyline : Shape
{
	public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register("FillRule", typeof(FillRule), typeof(RoundPolyline), new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(PointCollection), typeof(RoundPolyline), new FrameworkPropertyMetadata(GetEmptyPointCollection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(RoundPolyline), new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

	private Geometry _geometry;

	private static PointCollection _emptyPointCollection;

	private static ConstructorInfo _freezableDefaultValueFactoryCtor;

	protected override Geometry DefiningGeometry => _geometry;

	public double Radius
	{
		get
		{
			return (double)GetValue(RadiusProperty);
		}
		set
		{
			SetValue(RadiusProperty, value);
		}
	}

	public FillRule FillRule
	{
		get
		{
			return (FillRule)GetValue(FillRuleProperty);
		}
		set
		{
			SetValue(FillRuleProperty, value);
		}
	}

	public PointCollection Points
	{
		get
		{
			return (PointCollection)GetValue(PointsProperty);
		}
		set
		{
			SetValue(PointsProperty, value);
		}
	}

	private void DefineGeometry()
	{
		PointCollection points = Points;
		if (points == null)
		{
			_geometry = Geometry.Empty;
			return;
		}
		PathFigure pathFigure = new PathFigure();
		if (points.Count > 0)
		{
			pathFigure.StartPoint = points[0];
			if (points.Count > 1)
			{
				double radius = Radius;
				for (int i = 1; i < points.Count - 1; i++)
				{
					Vector vector = points[i] - points[i - 1];
					Vector vector2 = points[i + 1] - points[i];
					double num = Math.Min(Math.Min(vector.Length, vector2.Length) / 2.0, radius);
					double length = vector.Length;
					vector.Normalize();
					vector *= length - num;
					LineSegment value = new LineSegment(points[i - 1] + vector, isStroked: true);
					pathFigure.Segments.Add(value);
					vector2.Normalize();
					vector2 *= num;
					SweepDirection sweepDirection = ((Vector.AngleBetween(vector, vector2) > 0.0) ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
					ArcSegment value2 = new ArcSegment(points[i] + vector2, new Size(num, num), 0.0, isLargeArc: false, sweepDirection, isStroked: true);
					pathFigure.Segments.Add(value2);
				}
				pathFigure.Segments.Add(new LineSegment(points[points.Count - 1], isStroked: true));
			}
		}
		PathGeometry pathGeometry = new PathGeometry();
		pathGeometry.Figures.Add(pathFigure);
		pathGeometry.FillRule = FillRule;
		_geometry = ((pathGeometry.Bounds == Rect.Empty) ? Geometry.Empty : pathGeometry);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		DefineGeometry();
		return base.MeasureOverride(constraint);
	}

	private static object GetEmptyPointCollection()
	{
		if (_freezableDefaultValueFactoryCtor == null)
		{
			_emptyPointCollection = (PointCollection)typeof(PointCollection).GetProperty("Empty", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
			_freezableDefaultValueFactoryCtor = typeof(DependencyObject).Assembly.GetType("MS.Internal.FreezableDefaultValueFactory").GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
		}
		return _freezableDefaultValueFactoryCtor.Invoke(new object[1] { _emptyPointCollection });
	}
}
