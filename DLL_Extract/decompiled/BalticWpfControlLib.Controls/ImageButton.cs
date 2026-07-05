using System;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib.Controls;

public class ImageButton : Button
{
	public static readonly DependencyProperty ImageSizeProperty;

	public static readonly DependencyProperty NormalImageProperty;

	public static readonly DependencyProperty HoverImageProperty;

	public static readonly DependencyProperty PressedImageProperty;

	public static readonly DependencyProperty DisabledImageProperty;

	public static readonly DependencyProperty BorderVisibilityProperty;

	public double ImageSize
	{
		get
		{
			return (double)GetValue(ImageSizeProperty);
		}
		set
		{
			SetValue(ImageSizeProperty, value);
		}
	}

	public string NormalImage
	{
		get
		{
			return (string)GetValue(NormalImageProperty);
		}
		set
		{
			SetValue(NormalImageProperty, value);
		}
	}

	public string HoverImage
	{
		get
		{
			return (string)GetValue(HoverImageProperty);
		}
		set
		{
			SetValue(HoverImageProperty, value);
		}
	}

	public string PressedImage
	{
		get
		{
			return (string)GetValue(PressedImageProperty);
		}
		set
		{
			SetValue(PressedImageProperty, value);
		}
	}

	public string DisabledImage
	{
		get
		{
			return (string)GetValue(DisabledImageProperty);
		}
		set
		{
			SetValue(DisabledImageProperty, value);
		}
	}

	public Visibility BorderVisibility
	{
		get
		{
			return (Visibility)GetValue(BorderVisibilityProperty);
		}
		set
		{
			SetValue(BorderVisibilityProperty, value);
		}
	}

	static ImageButton()
	{
		ImageSizeProperty = DependencyProperty.Register("ImageSize", typeof(double), typeof(ImageButton), new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.AffectsRender));
		NormalImageProperty = DependencyProperty.Register("NormalImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));
		HoverImageProperty = DependencyProperty.Register("HoverImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));
		PressedImageProperty = DependencyProperty.Register("PressedImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));
		DisabledImageProperty = DependencyProperty.Register("DisabledImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));
		BorderVisibilityProperty = DependencyProperty.Register("BorderVisibility", typeof(Visibility), typeof(ImageButton), new FrameworkPropertyMetadata(Visibility.Hidden, FrameworkPropertyMetadataOptions.AffectsRender));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
	}

	private static void ImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (((string)e.NewValue).Length > 0)
		{
			Application.GetResourceStream(new Uri("pack://application:,,," + (string)e.NewValue));
		}
	}
}
