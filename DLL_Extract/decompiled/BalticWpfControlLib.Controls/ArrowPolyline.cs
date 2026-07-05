using System.Windows;
using System.Windows.Media;

namespace BalticWpfControlLib.Controls;

public class ArrowPolyline : ArrowLineBase
{
	public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(PointCollection), typeof(ArrowPolyline), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

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

	protected override Geometry DefiningGeometry
	{
		get
		{
			pathgeo.Figures.Clear();
			if (Points.Count > 0)
			{
				pathfigLine.StartPoint = Points[0];
				polysegLine.Points.Clear();
				for (int i = 1; i < Points.Count; i++)
				{
					polysegLine.Points.Add(Points[i]);
				}
				pathgeo.Figures.Add(pathfigLine);
			}
			return base.DefiningGeometry;
		}
	}

	public ArrowPolyline()
	{
		Points = new PointCollection();
	}
}
