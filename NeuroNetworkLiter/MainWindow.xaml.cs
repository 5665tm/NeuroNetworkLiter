// Changed 2014 09 01 8:05 PM Karavaev Vadim

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NeuroNetworkLiter
{
	public delegate void BeginInvokeDelegate();

	public partial class MainWindow
	{
		private readonly Random _rnd = new Random();
		private readonly BeginInvokeDelegate _refresh;
		private Bitmap _bitmapInput;
		private double _speed = 1;

		public MainWindow()
		{
			InitializeComponent();

			_refresh = delegate
			{
				ImageSource imgSourceFromBitmap =
					Imaging.CreateBitmapSourceFromHBitmap(_bitmapInput.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				InputImage.Source = imgSourceFromBitmap;
				_speed = LearnSpeed.Value;
			};
			Task.Factory.StartNew(Run);
		}

		private void Run()
		{
			while (true)
			{
				Thread.Sleep((int)(1000d/_speed));
				int number = _rnd.Next(1, 4);
				int font = _rnd.Next(97, 100); // a, b, c
				string imagePath = "Images/" + Convert.ToString(number) + ((char) font) + ".bmp";
				_bitmapInput = new Bitmap(imagePath);
				Dispatcher.BeginInvoke(DispatcherPriority.Input, _refresh);
			}
		}
	}
}