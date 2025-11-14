using System;

namespace AgendaAlfabetica
{
	public class NoLista<Dado>
		where Dado : IComparable<Dado>
	{
		private Dado info;
		private NoLista<Dado> prox;

		public NoLista(Dado informacao, NoLista<Dado> ponteiro)
		{
			info = informacao;
			prox = ponteiro;
		}

		public Dado Info
		{
			get
			{
				return info;
			}

			set
			{
				info = value;
			}
		}

		public NoLista<Dado> Prox
		{
			get
			{
				return prox;
			}

			set
			{
				prox = value;
			}
		}
	}
}
