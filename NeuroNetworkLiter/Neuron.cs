namespace NeuroNetworkLiter
{
	internal class Neuron
	{
		/// <summary>
		///     ������� ����������
		/// </summary>
		public readonly int[,] Input;

		/// <summary>
		///     ������ ��� �������� ����� ��������
		/// </summary>
		public readonly int[,] Weight;

		/// <summary>
		///     ������ ��� �������� ������������������ ��������
		/// </summary>
		private readonly int[,] _sinapsScale;

		/// <summary>
		///     ����� ��� �������� ������������������ ��������
		/// </summary>
		private int _axonPower;

		/// <summary>
		///     ����������� ������ �������
		/// </summary>
		/// <param name="sizex">������</param>
		/// <param name="sizey">��c���</param>
		/// <param name="inP">����</param>
		public Neuron(int sizex, int sizey, int[,] inP)
		{
			Weight = new int[sizex, sizey];
			_sinapsScale = new int[sizex, sizey];
			Input = new int[sizex, sizey];
			Input = inP;
		}

		/// <summary>
		///     ���������������
		/// </summary>
		public int GetAxonPower()
		{
			// ���������������
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					// �������� ��� ������ (0 ��� 1) �� ��� ����������� ��� � ��������� � ������.
					_sinapsScale[x, y] = Input[x, y]*Weight[x, y];
				}
			}

			// �������� ��������
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
		///     ���������� ������ �� ������ �����
		/// </summary>
		/// <param name="inP">������� ������</param>
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
		///     ���������� ������ �� �������� �����
		/// </summary>
		/// <param name="inP">������� ������</param>
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