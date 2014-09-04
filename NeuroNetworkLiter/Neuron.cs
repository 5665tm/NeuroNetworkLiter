// Changed: 2014 09 04 9:58 : 5665tm

namespace NeuroNetworkLiter
{
	internal class Neuron
	{
		/// <summary>
		///     ������� ����������
		/// </summary>
		public readonly double[,] Input;

		/// <summary>
		///     ������ ��� �������� ����� ��������
		/// </summary>
		private readonly double[,] _weight;

		/// <summary>
		///     ���������� ������� �����
		/// </summary>
		public double[,] Weight { get { return _weight; } }

		/// <summary>
		///     ������ ��� �������� ������������������ ��������
		/// </summary>
		private readonly double[,] _sinapsScale;

		/// <summary>
		///     ����� ��� �������� ������������������ ��������
		/// </summary>
		private double _axonPower;

		/// <summary>
		///     ����������� ������ �������
		/// </summary>
		/// <param name="x">���������� ��������� �� ������</param>
		/// <param name="y">���������� ��������� �� ������</param>
		public Neuron(int x, int y)
		{
			_weight = new double[x, y];
			_sinapsScale = new double[x, y];
			Input = new double[x, y];
		}

		/// <summary>
		///     ���������������
		/// </summary>
		public double GetAxonPower()
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
		///     ��������� ������� �� �� ��� ��� ����������� ���� �� ��� ����
		/// </summary>
		/// <param name="inP">������� ������</param>
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