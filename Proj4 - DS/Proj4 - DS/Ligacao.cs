using System;

namespace Proj4
{
	public class Ligacao : IComparable<Ligacao>
	{
		public string origem, destino;
		public int distancia;

		public int CompareTo(Ligacao other)
		{
			return (origem + destino).CompareTo(other.origem + other.destino);
		}


	}
}
