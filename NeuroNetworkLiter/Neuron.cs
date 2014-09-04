// Changed: 2014 09 04 9:58 : 5665tm

namespace NeuroNetworkLiter
{
	internal class Neuron
	{
		/// <summary>
		///     Входная информация
		/// </summary>
		public readonly double[,] Input;

		/// <summary>
		///     Массив для хранения весов синапсов
		/// </summary>
		private readonly double[,] _weight;

		/// <summary>
		///     Возвращает таблицу весов
		/// </summary>
		public double[,] Weight { get { return _weight; } }

		/// <summary>
		///     Массив для хранения отмасштабированных сигналов
		/// </summary>
		private readonly double[,] _sinapsScale;

		/// <summary>
		///     Сумма для хранения отмасштабированных сигналов
		/// </summary>
		private double _axonPower;

		/// <summary>
		///     Конструктор нового нейрона
		/// </summary>
		/// <param name="x">Количество дендритов по ширине</param>
		/// <param name="y">Количество дендритов по высоте</param>
		public Neuron(int x, int y)
		{
			_weight = new double[x, y];
			_sinapsScale = new double[x, y];
			Input = new double[x, y];
		}

		/// <summary>
		///     Масштабирование
		/// </summary>
		public double GetAxonPower()
		{
			// Масштабирование
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					_sinapsScale[x, y] = Input[x, y]*_weight[x, y];
				}
			}

			// Сложение сигналов
			_axonPower = 0;
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					_axonPower += _sinapsScale[x, y];
				}
			}
			return _axonPower;
		}

		/// <summary>
		///     Указывает нейрону на то что это было его изображение
		/// </summary>
		/// <param name="inP">Входные данные</param>
		public void Award(double[,] inP)
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					_weight[x, y] += (inP[x, y])*3;
				}
			}
		}

		/// <summary>
		///     Указывает нейрону на то что это изображение было не для него
		/// </summary>
		/// <param name="inP">Входные данные</param>
		public void Punish(double[,] inP)
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					_weight[x, y] -= inP[x, y];
				}
			}
		}
	}
}