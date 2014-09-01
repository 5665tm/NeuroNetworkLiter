// Changed 2014 09 01 9:25 PM Karavaev Vadim

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
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
		private Bitmap _bitmapInput;
		private double _speed = 1;

		private Web NW1 = new Web(8, 10, new int[8, 10]);
		private Web NW2 = new Web(8, 10, new int[8, 10]);
		private Web NW3 = new Web(8, 10, new int[8, 10]);

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
			var thread = new Thread(Run);
			thread.Start();
			List<double> inputList = new List<double>();
			inputList.Add(1);
			inputList.Add(0.5);
			inputList.Add(0.05);
			inputList.Add(0.03);
			inputList.Add(0.83);
			inputList.Add(0.33);
			CreateGraph(inputList);
		}

		private void CreateGraph(List<double> input)
		{
			double[] data1 = input.ToArray();
			int np = data1.Length - 1;
			// new double Np+1

			//Теперь нарисуем график
			var aDrawingGroup = new DrawingGroup();


			for (int DrawingStage = 0; DrawingStage < 10; DrawingStage++)
			{
				var drw = new GeometryDrawing();
				var gg = new GeometryGroup();


				//Задный фон
				if (DrawingStage == 1)
				{
					drw.Brush = Brushes.Beige;
					drw.Pen = new Pen(Brushes.LightGray, 0.01);
					var myRectGeometry = new RectangleGeometry();
					myRectGeometry.Rect = new Rect(0, 0, 1, 1);
					gg.Children.Add(myRectGeometry);
				}

				//Мелкая сетка
				if (DrawingStage == 2)
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
				if (DrawingStage == 3)
				{
					drw.Brush = Brushes.White;
					drw.Pen = new Pen(Brushes.Black, 0.005);

					gg = new GeometryGroup();
					for (int i = 0; i < np; i++)
					{
						LineGeometry l = new LineGeometry(new Point(i/(double) np, 1.0 - (data1[i])),
							new Point((i + 1)/(double) np, 1.0 - (data1[i + 1])));
						gg.Children.Add(l);
					}
				}

				//Обрезание лишнего
				if (DrawingStage == 5)
				{
					drw.Brush = Brushes.Transparent;
					drw.Pen = new Pen(Brushes.White, 0.2);

					var myRectGeometry = new RectangleGeometry();
					myRectGeometry.Rect = new Rect(-0.1, -0.1, 1.2, 1.2);
					gg.Children.Add(myRectGeometry);
				}


				//Рамка
				if (DrawingStage == 6)
				{
					drw.Brush = Brushes.Transparent;
					drw.Pen = new Pen(Brushes.LightGray, 0.01);

					var myRectGeometry = new RectangleGeometry {Rect = new Rect(0, 0, 1, 1)};
					gg.Children.Add(myRectGeometry);
				}


				//Надписи
				if (DrawingStage == 7)
				{
					drw.Brush = Brushes.LightGray;
					drw.Pen = new Pen(Brushes.Gray, 0.003);

					for (int i = 1; i < 10; i++)
					{
						// Create a formatted text string.
						var formattedText = new FormattedText(
							(1 - i*0.1).ToString(CultureInfo.InvariantCulture),
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