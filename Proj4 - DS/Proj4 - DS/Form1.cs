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
		private List<Cidade> cidades = new List<Cidade>();

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			string caminho = @"..\..\Dados\cidades.dat";

			using (BinaryReader br = new BinaryReader(File.OpenRead(caminho)))
			{
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					char[] nomeChars = br.ReadChars(25);

					// === CORREÇÃO IMPORTANTE ===
					string nome = new string(nomeChars)
						.Replace("\0", "")      // remove caracteres nulos
						.Trim();                // remove espaços extras

					double x = br.ReadDouble();
					double y = br.ReadDouble();

					cidades.Add(new Cidade(nome, x, y));
				}
			}

			pbMapa.Paint += pbMapa_Paint;
			pbMapa.Invalidate();

			arvore.LerArquivoDeRegistros(caminho);

			// Evento de desenho da árvore
			pnlArvore.Paint += pnlArvore_Paint;

			// Força o desenho imediato
			pnlArvore.Invalidate();
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

		private void pnlArvore_Paint(object sender, PaintEventArgs e)
		{
			arvore.Desenhar(pnlArvore);
		}

	
		private void btnBuscarCidade_Click(object sender, EventArgs e)
		{
			string nome = txtNomeCidade.Text.Trim().ToUpperInvariant();

			if (string.IsNullOrWhiteSpace(nome))
			{
				MessageBox.Show("Digite o nome da cidade.");
				return;
			}

			Cidade cidade = cidades.Find(c =>
				c.Nome.Replace("\0", "").Trim().ToUpperInvariant() == nome);

			if (cidade == null)
			{
				MessageBox.Show("Cidade não encontrada!");
				return;
			}

			udX.Value = (decimal)cidade.X;
			udY.Value = (decimal)cidade.Y;

			MessageBox.Show("Cidade encontrada!");
		}

		private void btnExcluirCidade_Click(object sender, EventArgs e)
		{
			string nome = txtNomeCidade.Text.Trim().ToUpperInvariant();

			if (string.IsNullOrWhiteSpace(nome))
			{
				MessageBox.Show("Digite o nome da cidade que deseja excluir.");
				return;
			}

			Cidade cidade = cidades.Find(c =>
				c.Nome.Replace("\0", "").Trim().ToUpperInvariant() == nome);

			if (cidade == null)
			{
				MessageBox.Show("Cidade não encontrada!");
				return;
			}

			cidades.Remove(cidade);

			// exclui da árvore
			arvore.Excluir(cidade);

			// salva arquivo
			SalvarArquivoCidades();

			pbMapa.Invalidate();
			pnlArvore.Invalidate();

			MessageBox.Show("Cidade excluída com sucesso!");
		}



		private void btnIncluirCidade_Click(object sender, EventArgs e)
		{
			string nome = txtNomeCidade.Text.Trim().ToUpperInvariant();

			if (string.IsNullOrWhiteSpace(nome))
			{
				MessageBox.Show("Digite o nome da cidade.");
				return;
			}

			// verifica se já existe
			Cidade existe = cidades.Find(c =>
				c.Nome.Replace("\0", "").Trim().ToUpperInvariant() == nome);

			if (existe != null)
			{
				MessageBox.Show("Cidade já existe!");
				return;
			}

			double x = (double)udX.Value;
			double y = (double)udY.Value;

			Cidade novaCidade = new Cidade(nome, x, y);

			// adiciona na lista
			cidades.Add(novaCidade);

			// adiciona na árvore
			arvore.IncluirNovoDado(novaCidade);

			// salva no arquivo
			SalvarArquivoCidades();

			// redesenha mapa + árvore
			pbMapa.Invalidate();
			pnlArvore.Invalidate();

			MessageBox.Show("Cidade incluída com sucesso!");
		}



		private void tpCadastro_Click(object sender, EventArgs e) { }
		private void label2_Click(object sender, EventArgs e) { }

		private void SalvarArquivoCidades()
		{
			string caminho = @"..\..\Dados\cidades.dat";

			using (BinaryWriter bw = new BinaryWriter(File.Open(caminho, FileMode.Create)))
			{
				foreach (var c in cidades)
				{
					// escreve nome com tamanho fixo 25 chars
					string nome = c.Nome.PadRight(25, '\0');
					bw.Write(nome.ToCharArray());

					bw.Write(c.X);
					bw.Write(c.Y);
				}
			}
		}

		private void txtNomeCidade_Leave(object sender, EventArgs e)
		{
			string nome = txtNomeCidade.Text.Trim().ToUpperInvariant();

			if (string.IsNullOrWhiteSpace(nome))
				return;

			Cidade existe = cidades.Find(c =>
				c.Nome.Replace("\0", "").Trim().ToUpperInvariant() == nome);

			if (existe != null)
			{
				MessageBox.Show("Cidade já existe!");
				txtNomeCidade.Focus();
			}
		}

		private void pbMapa_MouseClick(object sender, MouseEventArgs e)
		{
			int w = pbMapa.Width;
			int h = pbMapa.Height;

			// coordenadas proporcionais
			double x = (double)e.X / w;
			double y = (double)e.Y / h;

			udX.Value = (decimal)x;
			udY.Value = (decimal)y;
		}

		
		
		


	}


}
