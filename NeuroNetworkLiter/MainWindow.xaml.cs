// Changed 2014 09 01 10:31 PM Karavaev Vadim

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace NeuroNetworkLiter
{
	public delegate void BeginInvokeDelegate();

	public partial class MainWindow
	{
		private readonly Random _rnd = new Random();
		private readonly BeginInvokeDelegate _refresh;
		private double _speed = 1;
		private readonly List<double> inputList = new List<double> {0};
		private int _counter;
		private int _numberOfShot = 0;
		private Bitmap _bitmapInput;
		private ImageSource _imgSourceFromBitmap;
		private ImageSource memorySource1;
		private ImageSource memorySource2;
		private ImageSource memorySource3;


		private Web NW1 = new Web(8, 10, new int[8, 10]);
		private Web NW2 = new Web(8, 10, new int[8, 10]);
		private Web NW3 = new Web(8, 10, new int[8, 10]);

		public MainWindow()
		{
			InitializeComponent();

			RefreshGraph();
			byte[,] sou = new byte[2, 2] {{100, 200}, {0, 50}};
			Memory1.Source = WeightToBitmap(sou);


			_refresh = delegate
			{
				_speed = LearnSpeed.Value;
				_imgSourceFromBitmap = Imaging.CreateBitmapSourceFromHBitmap(_bitmapInput.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				InputImage.Source = _imgSourceFromBitmap;
				if (_counter++ == 20)
				{
					inputList.Add(Convert.ToDouble(_numberOfShot)/_counter);
					RefreshGraph();
				}
			};
			var thread = new Thread(Run);
			thread.Start();
		}

		private void RefreshGraph()
		{
			double[] data1 = inputList.ToArray();
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
					drw.Brush = Brushes.Beige;
					drw.Pen = new Pen(Brushes.LightGray, 0.01);
					var myRectGeometry = new RectangleGeometry {Rect = new Rect(0, 0, 1, 1)};
					gg.Children.Add(myRectGeometry);
				}

				//Мелкая сетка
				if (drawingStage == 2)
				{
					drw.Brush = Brushes.Beige;
					drw.Pen = new Pen(Brushes.Gray, 0.003);
					//drw.Pen.DashStyle = DashStyles.Dot;

					var dashes = new DoubleCollection();
					for (int i = 1; i < 10; i++)
						dashes.Add(0.1);
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
					drw.Brush = Brushes.White;
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
					drw.Pen = new Pen(Brushes.White, 0.2);

					var myRectGeometry = new RectangleGeometry {Rect = new Rect(-0.1, -0.1, 1.2, 1.2)};
					gg.Children.Add(myRectGeometry);
				}


				//Рамка
				if (drawingStage == 6)
				{
					drw.Brush = Brushes.Transparent;
					drw.Pen = new Pen(Brushes.LightGray, 0.01);

					var myRectGeometry = new RectangleGeometry {Rect = new Rect(0, 0, 1, 1)};
					gg.Children.Add(myRectGeometry);
				}


				//Надписи
				if (drawingStage == 7)
				{
					drw.Brush = Brushes.LightGray;
					drw.Pen = new Pen(Brushes.Gray, 0.003);

					for (int i = 1; i < 10; i++)
					{
						// Create a formatted text string.
						var formattedText = new FormattedText(
							(100 - i*10).ToString(CultureInfo.InvariantCulture),
							CultureInfo.GetCultureInfo("en-us"),
							FlowDirection.LeftToRight,
							new Typeface("Verdana"),
							0.05,
							Brushes.Black);

						// Set the font weight to Bold for the formatted text.
						formattedText.SetFontWeight(FontWeights.Light);

						// Build a geometry out of the formatted text.
						Geometry geometry = formattedText.BuildGeometry(new Point(-0.1, i*0.1 - 0.03));
						gg.Children.Add(geometry);
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
				Thread.Sleep((int) (1000d/_speed));
				int number = _rnd.Next(1, 4);
				int font = _rnd.Next(97, 100); // a, b, c
				string imagePath = "Images/" + Convert.ToString(number) + ((char) font) + ".bmp";
				_bitmapInput = new Bitmap(imagePath);

				Dispatcher.BeginInvoke(DispatcherPriority.Input, _refresh);
				Thread.Sleep(500);
			}
		}

		private ImageSource WeightToBitmap(byte[,] weight)
		{
			int i = 0;
			var input = new byte[weight.Length*3];
			foreach (var b in weight)
			{
				input[i++] = b;
				input[i++] = b;
				input[i++] = b;
			}
			using (MemoryStream ms = new MemoryStream(input))
			{
				int w = 1;
				int h = 1;
				int ch = 3; //number of channels (ie. assuming 24 bit RGB in this case)

				Bitmap bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
				IntPtr pNative = bmData.Scan0;
				Marshal.Copy(input, 0, pNative, input.Length);
				bitmap.UnlockBits(bmData);


				return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
		}
	}

	internal class Web
	{
		public int[,] mul; // Тут будем хранить отмасштабированные сигналы
		public int[,] weight; // Массив для хранения весов
		public int[,] input; // Входная информация
		public int limit = 9; // Порог - выбран экспериментально, для быстрого обучения
		public int sum; // Тут сохраним сумму масштабированных сигналов

		public Web(int sizex, int sizey, int[,] inP) // Задаем свойства при создании объекта
		{
			weight = new int[sizex, sizey]; // Определяемся с размером массива (число входов)
			mul = new int[sizex, sizey];

			input = new int[sizex, sizey];
			input = inP; // Получаем входные данные
		}

		// масштабирование
		public void mul_w()
		{
			for (int x = 0; x <= 2; x++)
			{
				for (int y = 0; y <= 4; y++) // Пробегаем по каждому аксону
				{
					mul[x, y] = input[x, y]*weight[x, y]; // Умножаем его сигнал (0 или 1) на его собственный вес и сохраняем в массив.
				}
			}
		}

		// сложение
		public void Sum()
		{
			sum = 0;
			for (int x = 0; x <= 2; x++)
			{
				for (int y = 0; y <= 4; y++)
				{
					sum += mul[x, y];
				}
			}
		}

		// сравнение
		public bool Rez()
		{
			return sum >= limit;
		}
	}
}