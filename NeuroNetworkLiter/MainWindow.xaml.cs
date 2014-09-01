// Changed 2014 09 02 12:06 AM Karavaev Vadim

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
		private int _numberOfShot;
		private Bitmap _bitmapInput;
		private ImageSource _imgSourceFromBitmap;
		private ImageSource memorySource1;
		private ImageSource memorySource2;
		private ImageSource memorySource3;


		private readonly Web NW1 = new Web(8, 10, new int[8, 10]);
		private readonly Web NW2 = new Web(8, 10, new int[8, 10]);
		private readonly Web NW3 = new Web(8, 10, new int[8, 10]);

		public MainWindow()
		{
			InitializeComponent();

			RefreshGraph();
			byte[,] sou = new byte[2, 2] {{100, 200}, {0, 50}};


			_refresh = delegate
			{
				int number = _rnd.Next(1, 4);
				int font = _rnd.Next(97, 103); // a, b, c
				string imagePath = "Images/" + Convert.ToString(number) + ((char) font) + ".bmp";
				_bitmapInput = new Bitmap(imagePath);

				for (var x = 0; x < 8; x++)
				{
					for (var y = 0; y < 10; y++)
					{
						int n = ((_bitmapInput.GetPixel(x, y).B) + (_bitmapInput.GetPixel(x, y).G) + (_bitmapInput.GetPixel(x, y).R))/3;
						if (n >= 220)
							n = 0; // Определяем, закрашен ли пиксель
						else
							n = 1;

						NW1.input[x, y] = n; // Присваиваем соответствующее значение каждой ячейке входных данных
						NW2.input[x, y] = n; // Присваиваем соответствующее значение каждой ячейке входных данных
						NW3.input[x, y] = n; // Присваиваем соответствующее значение каждой ячейке входных данных
					}
				}

				bool result = false;
				NW1.mul_w();
				NW1.Sum();
				if (number == 1)
				{
					if (NW1.Rez())
					{
						Ans1.Fill = Brushes.Green;
						result = true;
					}
					else
					{
						Ans1.Fill = Brushes.Red;
						NW1.incW(NW1.input);
						result = false;
					}
				}
				else
				{
					if (NW1.Rez())
					{
						Ans1.Fill = Brushes.Red;
						NW1.decW(NW1.input);
						result = false;
					}
					else
					{
						Ans1.Fill = Brushes.Blue;
					}
				}
				Memory1.Source = WeightToBitmap(NW1.weight);

				NW2.mul_w();
				NW2.Sum();
				if (number == 2)
				{
					if (NW2.Rez())
					{
						Ans2.Fill = Brushes.Green;
						result = true;
					}
					else
					{
						Ans2.Fill = Brushes.Red;
						NW2.incW(NW2.input);
						result = false;
					}
				}
				else
				{
					if (NW2.Rez())
					{
						Ans2.Fill = Brushes.Red;
						NW2.decW(NW2.input);
						result = false;
					}
					else
					{
						Ans2.Fill = Brushes.Blue;
					}
				}
				Memory2.Source = WeightToBitmap(NW2.weight);

				NW3.mul_w();
				NW3.Sum();
				if (number == 3)
				{
					if (NW3.Rez())
					{
						Ans3.Fill = Brushes.Green;
						result = true;
					}
					else
					{
						Ans3.Fill = Brushes.Red;
						NW3.incW(NW3.input);
						result = false;
					}
				}
				else
				{
					if (NW3.Rez())
					{
						Ans3.Fill = Brushes.Red;
						NW3.decW(NW3.input);
						result = false;
					}
					else
					{
						Ans3.Fill = Brushes.Blue;
					}
				}
				if (result)
					_numberOfShot++;
				Memory3.Source = WeightToBitmap(NW3.weight);

				_speed = LearnSpeed.Value;
				_imgSourceFromBitmap = Imaging.CreateBitmapSourceFromHBitmap(_bitmapInput.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				InputImage.Source = _imgSourceFromBitmap;
				if (_counter++ == 20)
				{
					inputList.Add((Convert.ToDouble(_numberOfShot)/(_counter)) - 0.01);
					_numberOfShot = 0;
					_counter = 0;
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
				Dispatcher.BeginInvoke(DispatcherPriority.Input, _refresh);
			}
		}

		private ImageSource WeightToBitmap(int[,] weight)
		{
			int i = 0;
			var input = new byte[weight.Length*3];
			for (int m = 0; m < 10; m++)
			{
				for (int s = 0; s < 8; s++)
				{
					int b = weight[s, m];
					int val = b*1;
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
			using (MemoryStream ms = new MemoryStream(input))
			{
				int w = 8;
				int h = 10;

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
		public int limit = 2000; // Порог - выбран экспериментально, для быстрого обучения
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
			for (int x = 0; x <= 7; x++)
			{
				for (int y = 0; y <= 9; y++) // Пробегаем по каждому аксону
				{
					mul[x, y] = input[x, y]*weight[x, y]; // Умножаем его сигнал (0 или 1) на его собственный вес и сохраняем в массив.
				}
			}
		}

		// сложение
		public int Sum()
		{
			sum = 0;
			for (int x = 0; x <= 7; x++)
			{
				for (int y = 0; y <= 9; y++)
				{
					sum += mul[x, y];
				}
			}
			return sum;
		}

		public void incW(int[,] inP)
		{
			for (int x = 0; x <= 7; x++)
			{
				for (int y = 0; y <= 9; y++)
				{
					weight[x, y] += inP[x, y];
				}
			}
		}

		public void decW(int[,] inP)
		{
			for (int x = 0; x <= 7; x++)
			{
				for (int y = 0; y <= 9; y++)
				{
					weight[x, y] -= inP[x, y];
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