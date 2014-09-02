// Changed 2014 09 03 12:31 AM Karavaev Vadim

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
		private readonly int[,] _weight;

		/// <summary>
		///     ���������� ������� �����
		/// </summary>
		public int[,] Weight { get { return _weight; } }

		/// <summary>
		///     ������ ��� �������� ������������������ ��������
		/// </summary>
		private readonly int[,] _sinapsScale;

		/// <summary>
		///     ����� ��� �������� ������������������ ��������
		/// </summary>
		private int _axonPower;

		/// <summary>
		/// ����������� ������ �������
		/// </summary>
		/// <param name="x">���������� ��������� �� ������</param>
		/// <param name="y">���������� ��������� �� ������</param>
		public Neuron(int x, int y)
		{
			_weight = new int[x, y];
			_sinapsScale = new int[x, y];
			Input = new int[x,y];
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
					_sinapsScale[x, y] = Input[x, y]*_weight[x, y];
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
		///     ��������� ������� �� �� ��� ��� ���� ��� �����������
		/// </summary>
		/// <param name="inP">������� ������</param>
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
		///     ��������� ������� �� �� ��� ��� ����������� ���� �� ��� ����
		/// </summary>
		/// <param name="inP">������� ������</param>
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