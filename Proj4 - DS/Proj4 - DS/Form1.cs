using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Proj4
{
	public partial class Form1 : Form
	{
		Arvore<Cidade> arvore = new Arvore<Cidade>();
		public Form1()
		{
			InitializeComponent();
		}

		private void tpCadastro_Click(object sender, EventArgs e)
		{

		}

		private void label2_Click(object sender, EventArgs e)
		{

		}

		private List<Cidade> cidades = new List<Cidade>();



		private void Form1_Load(object sender, EventArgs e)
		{
			string caminho = @"..\..\Dados\cidades.dat";

			using (BinaryReader br = new BinaryReader(File.OpenRead(caminho)))
			{
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					char[] nomeChars = br.ReadChars(25);
					string nome = new string(nomeChars).TrimEnd();

					double x = br.ReadDouble();
					double y = br.ReadDouble();

					cidades.Add(new Cidade(nome, x, y));
				}
			}

			pbMapa.Paint += pbMapa_Paint;
			pbMapa.Invalidate();
		}



		private void pnlArvore_Paint(object sender, PaintEventArgs e)
		{
			arvore.Desenhar(pnlArvore);
		}

		private void btnIncluirCidade_Click(object sender, EventArgs e)
		{

		}

		private void pbMapa_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			int w = pbMapa.Width;
			int h = pbMapa.Height;

			foreach (var c in cidades)
			{
				
				float px = (float)(c.X * w);
				float py = (float)(c.Y * h);

		
				g.FillEllipse(Brushes.Red, px - 3, py - 3, 6, 6);

				
				g.DrawString(c.Nome, this.Font, Brushes.Black, px + 5, py - 10);
			}
		}


	}
}
