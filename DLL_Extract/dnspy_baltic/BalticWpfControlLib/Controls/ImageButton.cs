using System;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000085 RID: 133
	public class ImageButton : Button
	{
		// Token: 0x06000627 RID: 1575 RVA: 0x0003A858 File Offset: 0x00038A58
		static ImageButton()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000628 RID: 1576 RVA: 0x0003A9DC File Offset: 0x00038BDC
		// (set) Token: 0x06000629 RID: 1577 RVA: 0x0003A9EE File Offset: 0x00038BEE
		public double ImageSize
		{
			get
			{
				return (double)base.GetValue(ImageButton.ImageSizeProperty);
			}
			set
			{
				base.SetValue(ImageButton.ImageSizeProperty, value);
			}
		}

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x0600062A RID: 1578 RVA: 0x0003AA01 File Offset: 0x00038C01
		// (set) Token: 0x0600062B RID: 1579 RVA: 0x0003AA13 File Offset: 0x00038C13
		public string NormalImage
		{
			get
			{
				return (string)base.GetValue(ImageButton.NormalImageProperty);
			}
			set
			{
				base.SetValue(ImageButton.NormalImageProperty, value);
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x0003AA21 File Offset: 0x00038C21
		// (set) Token: 0x0600062D RID: 1581 RVA: 0x0003AA33 File Offset: 0x00038C33
		public string HoverImage
		{
			get
			{
				return (string)base.GetValue(ImageButton.HoverImageProperty);
			}
			set
			{
				base.SetValue(ImageButton.HoverImageProperty, value);
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x0003AA41 File Offset: 0x00038C41
		// (set) Token: 0x0600062F RID: 1583 RVA: 0x0003AA53 File Offset: 0x00038C53
		public string PressedImage
		{
			get
			{
				return (string)base.GetValue(ImageButton.PressedImageProperty);
			}
			set
			{
				base.SetValue(ImageButton.PressedImageProperty, value);
			}
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000630 RID: 1584 RVA: 0x0003AA61 File Offset: 0x00038C61
		// (set) Token: 0x06000631 RID: 1585 RVA: 0x0003AA73 File Offset: 0x00038C73
		public string DisabledImage
		{
			get
			{
				return (string)base.GetValue(ImageButton.DisabledImageProperty);
			}
			set
			{
				base.SetValue(ImageButton.DisabledImageProperty, value);
			}
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0003AA81 File Offset: 0x00038C81
		private static void ImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (((string)e.NewValue).Length > 0)
			{
				Application.GetResourceStream(new Uri("pack://application:,,," + (string)e.NewValue));
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000633 RID: 1587 RVA: 0x0003AAB8 File Offset: 0x00038CB8
		// (set) Token: 0x06000634 RID: 1588 RVA: 0x0003AACA File Offset: 0x00038CCA
		public Visibility BorderVisibility
		{
			get
			{
				return (Visibility)base.GetValue(ImageButton.BorderVisibilityProperty);
			}
			set
			{
				base.SetValue(ImageButton.BorderVisibilityProperty, value);
			}
		}

		// Token: 0x0400034B RID: 843
		public static readonly DependencyProperty ImageSizeProperty = DependencyProperty.Register("ImageSize", typeof(double), typeof(ImageButton), new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.AffectsRender));

		// Token: 0x0400034C RID: 844
		public static readonly DependencyProperty NormalImageProperty = DependencyProperty.Register("NormalImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ImageButton.ImageSourceChanged)));

		// Token: 0x0400034D RID: 845
		public static readonly DependencyProperty HoverImageProperty = DependencyProperty.Register("HoverImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ImageButton.ImageSourceChanged)));

		// Token: 0x0400034E RID: 846
		public static readonly DependencyProperty PressedImageProperty = DependencyProperty.Register("PressedImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ImageButton.ImageSourceChanged)));

		// Token: 0x0400034F RID: 847
		public static readonly DependencyProperty DisabledImageProperty = DependencyProperty.Register("DisabledImage", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ImageButton.ImageSourceChanged)));

		// Token: 0x04000350 RID: 848
		public static readonly DependencyProperty BorderVisibilityProperty = DependencyProperty.Register("BorderVisibility", typeof(Visibility), typeof(ImageButton), new FrameworkPropertyMetadata(Visibility.Hidden, FrameworkPropertyMetadataOptions.AffectsRender));
	}
}
