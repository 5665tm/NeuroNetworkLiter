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
		public readonly int[,] Weight;

		/// <summary>
		///     Массив для хранения отмасштабированных сигналов
		/// </summary>
		private readonly int[,] _sinapsScale;

		/// <summary>
		///     Сумма для хранения отмасштабированных сигналов
		/// </summary>
		private int _axonPower;

		/// <summary>
		///     Конструктор нового нейрона
		/// </summary>
		/// <param name="sizex">Ширина</param>
		/// <param name="sizey">Выcота</param>
		/// <param name="inP">Вход</param>
		public Neuron(int sizex, int sizey, int[,] inP)
		{
			Weight = new int[sizex, sizey];
			_sinapsScale = new int[sizex, sizey];
			Input = new int[sizex, sizey];
			Input = inP;
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
					// Умножаем его сигнал (0 или 1) на его собственный вес и сохраняем в массив.
					_sinapsScale[x, y] = Input[x, y]*Weight[x, y];
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
		///     Награждает нейрон за верный ответ
		/// </summary>
		/// <param name="inP">Входные данные</param>
		public void Award(int[,] inP)
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					Weight[x, y] += (inP[x, y]) * 5;
				}
			}
		}

		/// <summary>
		///     Наказывает нейрон за неверный ответ
		/// </summary>
		/// <param name="inP">Входные данные</param>
		public void Punish(int[,] inP)
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					Weight[x, y] -= inP[x, y];
				}
			}
		}
	}
}