using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000083 RID: 131
	public partial class CircularProgressBar : UserControl
	{
		// Token: 0x06000617 RID: 1559 RVA: 0x0003A3B4 File Offset: 0x000385B4
		public CircularProgressBar()
		{
			this.InitializeComponent();
			this.animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, base.Dispatcher);
			this.animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 75);
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x0003A3EA File Offset: 0x000385EA
		private void Start()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			this.animationTimer.Tick += this.HandleAnimationTick;
			this.animationTimer.Start();
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x0003A418 File Offset: 0x00038618
		private void Stop()
		{
			this.animationTimer.Stop();
			Mouse.OverrideCursor = Cursors.Arrow;
			this.animationTimer.Tick -= this.HandleAnimationTick;
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x0003A446 File Offset: 0x00038646
		private void HandleAnimationTick(object sender, EventArgs e)
		{
			this.SpinnerRotate.Angle = (this.SpinnerRotate.Angle + 36.0) % 360.0;
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x0003A474 File Offset: 0x00038674
		private void HandleLoaded(object sender, RoutedEventArgs e)
		{
			CircularProgressBar.SetPosition(this.C0, 3.141592653589793, 0.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C1, 3.141592653589793, 1.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C2, 3.141592653589793, 2.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C3, 3.141592653589793, 3.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C4, 3.141592653589793, 4.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C5, 3.141592653589793, 5.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C6, 3.141592653589793, 6.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C7, 3.141592653589793, 7.0, 0.6283185307179586);
			CircularProgressBar.SetPosition(this.C8, 3.141592653589793, 8.0, 0.6283185307179586);
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x0003A5D8 File Offset: 0x000387D8
		private static void SetPosition(Ellipse ellipse, double offset, double posOffSet, double step)
		{
			ellipse.SetValue(Canvas.LeftProperty, 50.0 + Math.Sin(offset + posOffSet * step) * 50.0);
			ellipse.SetValue(Canvas.TopProperty, 50.0 + Math.Cos(offset + posOffSet * step) * 50.0);
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x0003A641 File Offset: 0x00038841
		private void HandleUnloaded(object sender, RoutedEventArgs e)
		{
			this.Stop();
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x0003A649 File Offset: 0x00038849
		private void HandleVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				this.Start();
				return;
			}
			this.Stop();
		}

		// Token: 0x0400033B RID: 827
		private readonly DispatcherTimer animationTimer;
	}
}
