// Changed 2014 09 03 12:31 AM Karavaev Vadim

namespace NeuroNetworkLiter
{
	internal class Neuron
	{
		/// <summary>
		///     Входная информация
		/// </summary>
		public readonly int[,] Input;

		/// <summary>
		///     Массив для хранения весов синапсов
		/// </summary>
		private readonly int[,] _weight;

		/// <summary>
		///     Возвращает таблицу весов
		/// </summary>
		public int[,] Weight { get { return _weight; } }

		/// <summary>
		///     Массив для хранения отмасштабированных сигналов
		/// </summary>
		private readonly int[,] _sinapsScale;

		/// <summary>
		///     Сумма для хранения отмасштабированных сигналов
		/// </summary>
		private int _axonPower;

		/// <summary>
		/// Конструктор нового нейрона
		/// </summary>
		/// <param name="x">Количество дендритов по ширине</param>
		/// <param name="y">Количество дендритов по высоте</param>
		public Neuron(int x, int y)
		{
			_weight = new int[x, y];
			_sinapsScale = new int[x, y];
			Input = new int[x,y];
		}

		/// <summary>
		///     Масштабирование
		/// </summary>
		public int GetAxonPower()
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
		public void Award(int[,] inP)
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
		public void Punish(int[,] inP)
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