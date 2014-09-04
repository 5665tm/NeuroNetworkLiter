// Changed: 2014 09 04 9:57 : 5665tm

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using DashStyle = System.Windows.Media.DashStyle;
using Image = System.Windows.Controls.Image;
using Pen = System.Windows.Media.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace NeuroNetworkLiter
{
	public delegate void BeginInvokeDelegate();

	public partial class MainWindow
	{
		private const double _TOLERANCE = 0.001;
		private readonly List<double> _inputList = new List<double> { 0 };

		private readonly Neuron[] _neuronNetwork =
		{
			new Neuron(8, 10),
			new Neuron(8, 10),
			new Neuron(8, 10)
		};

		private readonly List<bool> _numberOfShot = new List<bool>();
		private readonly BeginInvokeDelegate _refresh;
		private readonly Random _rnd = new Random();
		private readonly Thread _thread;
		private int _counter;
		private double _speed = 1;

		public MainWindow()
		{
			InitializeComponent();
			RefreshGraph();

			_refresh = delegate
			{
				int number = _rnd.Next(1, 4);
				int font = _rnd.Next(97, 103); // a, b, c
				string imagePath = "Images/" + Convert.ToString(number) + ((char) font) + ".bmp";
				var bitmapInput = new Bitmap(imagePath);

				for (int x = 0; x < 8; x++)
				{
					for (int y = 0; y < 10; y++)
					{
						double n = Math.Min((bitmapInput.GetPixel(x, y).B), (bitmapInput.GetPixel(x, y).G));
						n = Math.Min(n, bitmapInput.GetPixel(x, y).R);
						n = 255 - n;

						_neuronNetwork[0].Input[x, y] = n; // Присваиваем соответствующее значение каждой ячейке входных данных
						_neuronNetwork[1].Input[x, y] = n; // Присваиваем соответствующее значение каждой ячейке входных данных
						_neuronNetwork[2].Input[x, y] = n; // Присваиваем соответствующее значение каждой ячейке входных данных
					}
				}

				int answer = 1;
				double p = _neuronNetwork.Max(x => x.GetAxonPower());
				for (int i = 0; i < _neuronNetwork.Length; i++)
				{
					if (Math.Abs(_neuronNetwork[i].GetAxonPower() - p) < _TOLERANCE)
					{
						answer = i + 1;
					}
				}

				Rectangle[] rec = { Ans1, Ans2, Ans3 };
				bool result = Fill(number, answer, rec);
				_numberOfShot.Add(result);

				_speed = LearnSpeed.Value;

				BitmapToImageControl(bitmapInput, InputImage);

				if (_counter++ > 20)
				{
					_numberOfShot.RemoveRange(0, 1);
				}

				int lol = (from e in _numberOfShot
						   where e
						   select e).Count();
				double newResult = (Convert.ToDouble(lol)/_numberOfShot.Count);
				if (newResult > 0.99)
				{
					newResult = 0.99;
				}
				else if (newResult < 0.01)
				{
					newResult = 0.01;
				}
				_inputList.Add(newResult);
				RefreshGraph();
			};
			_thread = new Thread(Run);
			_thread.Start();
		}

		private void BitmapToImageControl(Bitmap bitmapInput, Image image)
		{
			var b = new Bitmap(160, 200);
			using (Graphics g = Graphics.FromImage(b))
			{
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.DrawImage(bitmapInput, 0, 0, b.Width, b.Height);
				ImageSource imgSourceFromBitmap = Imaging.CreateBitmapSourceFromHBitmap(b.GetHbitmap(), IntPtr.Zero,
					Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				image.Source = imgSourceFromBitmap;
			}

			var listByte = new byte[10*8*3];
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					listByte[(((8*x) + y)*3) + 0] = bitmapInput.GetPixel(y, x).B;
					listByte[(((8*x) + y)*3) + 1] = bitmapInput.GetPixel(y, x).G;
					listByte[(((8*x) + y)*3) + 2] = bitmapInput.GetPixel(y, x).R;
				}
			}

			var input = new byte[listByte.Length*20*20];
			int i = 0;
			for (int m = 0; m < listByte.Length; m += 3)
			{
				for (int k = 0; k < 20; k++)
				{
					for (int s = 0; s < 20; s++)
					{
						input[((k*8*20 + s + (i/8)*8*20*20 + (i%8)*20)*3) + 0] = listByte[m + 0];
						input[((k*8*20 + s + (i/8)*8*20*20 + (i%8)*20)*3) + 1] = listByte[m + 1];
						input[((k*8*20 + s + (i/8)*8*20*20 + (i%8)*20)*3) + 2] = listByte[m + 2];
					}
				}
				i++;
			}
			using (new MemoryStream(input))
			{
				const int W = 8*20;
				const int H = 10*20;

				var bitmap = new Bitmap(W, H, PixelFormat.Format24bppRgb);
				BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.ReadWrite,
					bitmap.PixelFormat);
				IntPtr pNative = bmData.Scan0;
				Marshal.Copy(input, 0, pNative, input.Length);
				bitmap.UnlockBits(bmData);

				image.Source = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
		}

		private bool Fill(int number, int answer, Rectangle[] rec)
		{
			bool result;
			if (number == answer)
			{
				result = true;
				for (int i = 0; i < rec.Length; i++)
				{
					if (i != (number - 1))
					{
						rec[i].Fill = Brushes.Gray;
					}
					else
					{
						rec[i].Fill = Brushes.Green;
					}
				}
			}
			else
			{
				result = false;
				for (int i = 0; i < rec.Length; i++)
				{
					rec[i].Fill = i != (answer - 1) ? Brushes.Gray : Brushes.Red;
					if (i != (number - 1))
					{
						_neuronNetwork[i].Punish(_neuronNetwork[i].Input);
					}
					else
					{
						_neuronNetwork[i].Award(_neuronNetwork[i].Input);
					}
				}
			}
			BitmapToImageControl(WeightToBitmap((_neuronNetwork[0].Weight)), Memory1);
			BitmapToImageControl(WeightToBitmap((_neuronNetwork[1].Weight)), Memory2);
			BitmapToImageControl(WeightToBitmap((_neuronNetwork[2].Weight)), Memory3);
			return result;
		}

		private void RefreshGraph()
		{
			double[] data1 = _inputList.ToArray();
			int np = data1.Length - 1;
			// new double Np+1

			//Теперь нарисуем график
			var aDrawingGroup = new DrawingGroup();

			for (int drawingStage = 0; drawingStage < 10; drawingStage++)
			{
				var drw = new GeometryDrawing();
				var gg = new GeometryGroup();

				//Задный фон
				if (drawingStage == 1)
				{
					drw.Brush = Brushes.White;
					var myRectGeometry = new RectangleGeometry
					{
						Rect = new Rect(0, 0, 1, 1)
					};
					gg.Children.Add(myRectGeometry);
				}

				//Мелкая сетка
				if (drawingStage == 2)
				{
					drw.Brush = Brushes.Blue;
					drw.Pen = new Pen(Brushes.Blue, 0.003);

					var dashes = new DoubleCollection();
					for (int i = 1; i < 10; i++)
					{
						dashes.Add(0.1);
					}
					drw.Pen.DashStyle = new DashStyle(dashes, 0);

					drw.Pen.EndLineCap = PenLineCap.Round;
					drw.Pen.StartLineCap = PenLineCap.Round;
					drw.Pen.DashCap = PenLineCap.Round;

					for (int i = 1; i < 10; i++)
					{
						var myRectGeometry = new LineGeometry(new Point(1.1, i*0.1), new Point(-0.1, i*0.1));
						gg.Children.Add(myRectGeometry);
					}
				}

				//график #1 - линия
				if (drawingStage == 3)
				{
					drw.Brush = Brushes.Black;
					drw.Pen = new Pen(Brushes.Black, 0.005);

					gg = new GeometryGroup();
					for (int i = 0; i < np; i++)
					{
						var l = new LineGeometry(new Point(i/(double) np, 1.0 - (data1[i])),
							new Point((i + 1)/(double) np, 1.0 - (data1[i + 1])));
						gg.Children.Add(l);
					}
				}

				//Обрезание лишнего
				if (drawingStage == 5)
				{
					drw.Brush = Brushes.Transparent;
					drw.Pen = new Pen(Brushes.White, 0.1);

					var myRectGeometry = new RectangleGeometry
					{
						Rect = new Rect(-0.05, -0.05, 1.1, 1.1)
					};
					gg.Children.Add(myRectGeometry);
				}

				//Рамка
				if (drawingStage == 6)
				{
					drw.Brush = Brushes.Transparent;
					drw.Pen = new Pen(Brushes.Black, 0.003);

					var myRectGeometry = new RectangleGeometry
					{
						Rect = new Rect(0, 0, 1, 1)
					};
					gg.Children.Add(myRectGeometry);
				}

				//Надписи
				if (drawingStage == 7)
				{
					drw.Brush = Brushes.Black;
					drw.Pen = new Pen(Brushes.Black, 0.003);

					for (int i = 1; i < 10; i++)
					{
						// Create a formatted text string.
						var formattedText = new FormattedText(
							(100 - i*10).ToString(CultureInfo.InvariantCulture),
							CultureInfo.GetCultureInfo("en-us"),
							FlowDirection.LeftToRight,
							new Typeface("Helvetica"),
							0.05,
							Brushes.Black);

						// Set the font weight to Bold for the formatted text.
						formattedText.SetFontWeight(FontWeights.UltraLight);

						// Build a geometry out of the formatted text.
						Geometry geometry = formattedText.BuildGeometry(new Point(-0.07, i*0.1 - 0.03));
						Geometry geometry2 = formattedText.BuildGeometry(new Point(1.02, i*0.1 - 0.03));
						gg.Children.Add(geometry);
						gg.Children.Add(geometry2);
					}
				}

				drw.Geometry = gg;
				aDrawingGroup.Children.Add(drw);
			}

			Image1.Source = new DrawingImage(aDrawingGroup);
		}

		private void Run()
		{
			while (true)
			{
				Thread.Sleep((int) (3000d/_speed));
				Dispatcher.BeginInvoke(DispatcherPriority.Input, _refresh);
				Thread.Sleep(400);
			}
			// ReSharper disable once FunctionNeverReturns
		}

		private Bitmap WeightToBitmap(double[,] weight)
		{
			int i = 0;
			var input = new byte[weight.Length*3];
			for (int m = 0; m < 10; m++)
			{
				for (int s = 0; s < 8; s++)
				{
					double b = weight[s, m];
					double val = 50 + b/10;
					byte value = 0;
					if (val > 0)
					{
						if (val > 255)
						{
							val = 255;
						}
						value = Convert.ToByte(val);
					}
					input[i++] = value;
					input[i++] = value;
					input[i++] = value;
				}
			}
			using (new MemoryStream(input))
			{
				const int W = 8;
				const int H = 10;

				var bitmap = new Bitmap(W, H, PixelFormat.Format24bppRgb);
				BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.ReadWrite, bitmap.PixelFormat);
				IntPtr pNative = bmData.Scan0;
				Marshal.Copy(input, 0, pNative, input.Length);
				bitmap.UnlockBits(bmData);

				return bitmap;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			_thread.Abort();
		}
	}
}