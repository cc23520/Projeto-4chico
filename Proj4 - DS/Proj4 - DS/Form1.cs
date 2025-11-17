using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Proj4
{
	public partial class Form1 : Form
	{
		Arvore<Cidade> arvore = new Arvore<Cidade>();
		private List<Cidade> cidades = new List<Cidade>();
		private List<Ligacao> ligacoes = new List<Ligacao>();
		private Dictionary<string, Cidade> indexCidades; // chave = nome normalizado


		private string NormalizeNome(string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return string.Empty;

			s = s.Trim().ToUpperInvariant();

			// remove acentos
			string temp = s.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder();
			foreach (var ch in temp)
			{
				var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
				if (cat != UnicodeCategory.NonSpacingMark)
					sb.Append(ch);
			}
			string noAcento = sb.ToString().Normalize(NormalizationForm.FormC);

			// remove caracteres não alfanuméricos (mantém espaços)
			string apenasAlfaNum = Regex.Replace(noAcento, @"[^A-Z0-9\s]", "");

			// compacta espaços
			apenasAlfaNum = Regex.Replace(apenasAlfaNum, @"\s+", " ").Trim();

			return apenasAlfaNum;
		}


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

		
			indexCidades = new Dictionary<string, Cidade>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var c in cidades)
			{
				string chave = NormalizeNome(c.Nome);
				if (!indexCidades.ContainsKey(chave))
					indexCidades.Add(chave, c);
			}


			pbMapa.Paint += pbMapa_Paint;
			pbMapa.Invalidate();

			arvore.LerArquivoDeRegistros(caminho);

			pnlArvore.Paint += pnlArvore_Paint;

			
			pnlArvore.Invalidate();

			string caminhoLig = @"..\..\Dados\GrafoOnibusSaoPaulo.txt";
			int linhasTotais = 0;
			int faltantes = 0;

			if (File.Exists(caminhoLig))
			{
				foreach (string raw in File.ReadAllLines(caminhoLig))
				{
					linhasTotais++;
					string linha = raw.Trim();
					if (string.IsNullOrWhiteSpace(linha)) continue;

					string[] partes = linha.Split(';');
					if (partes.Length < 2) continue;

					string origemRaw = partes[0].Trim();
					string destinoRaw = partes.Length >= 2 ? partes[1].Trim() : "";
					string distRaw = partes.Length >= 3 ? partes[2].Trim() : "";

					// extrair número da distância (mais tolerante)
					int distancia = 0;
					var m = Regex.Match(distRaw, @"\d+");
					if (m.Success)
						int.TryParse(m.Value, out distancia);

					// criar ligação
					Ligacao lig = new Ligacao();
					lig.origem = origemRaw;
					lig.destino = destinoRaw;
					lig.distancia = distancia;

					// tenta localizar cidades normalizando
					string oKey = NormalizeNome(origemRaw);
					string dKey = NormalizeNome(destinoRaw);

					bool okOrigem = indexCidades.ContainsKey(oKey);
					bool okDestino = indexCidades.ContainsKey(dKey);

					if (!okOrigem || !okDestino)
					{
						faltantes++;
						// ainda adiciona a ligação (pode ser útil para debug), mas será ignorada no desenho
						ligacoes.Add(lig);
						continue;
					}

					ligacoes.Add(lig);
				}
			}
			else
			{
				MessageBox.Show("Arquivo de ligações não encontrado:\n" + caminhoLig);
			}

			// show debug: quantos carregados / quantos com falta de cidade
			MessageBox.Show($"Linhas no arquivo: {linhasTotais}\nLigacoes carregadas: {ligacoes.Count}\nLigacoes com cidade faltando: {faltantes}");


		}

		private void pbMapa_Paint(object sender, PaintEventArgs e)
		{



			Graphics g = e.Graphics;

			int w = pbMapa.Width;
			int h = pbMapa.Height;

	
			using (Pen pen = new Pen(Color.Blue, 2))
			using (Font f = new Font(this.Font.FontFamily, 8))
			using (Brush textoBrush = new SolidBrush(Color.Black))
			using (Brush fundoTexto = new SolidBrush(Color.FromArgb(230, Color.White)))
			{
				foreach (var lig in ligacoes)
				{
					string oKey = NormalizeNome(lig.origem);
					string dKey = NormalizeNome(lig.destino);

					if (!indexCidades.TryGetValue(oKey, out Cidade c1) ||
						!indexCidades.TryGetValue(dKey, out Cidade c2))
					{
				
						continue;
					}

					float x1 = (float)(c1.X * w);
					float y1 = (float)(c1.Y * h);
					float x2 = (float)(c2.X * w);
					float y2 = (float)(c2.Y * h);

					g.DrawLine(pen, x1, y1, x2, y2);

					float mx = (x1 + x2) / 2;
					float my = (y1 + y2) / 2;

					string texto = $"{lig.distancia} km";
					SizeF textoSize = g.MeasureString(texto, f);
					RectangleF bg = new RectangleF(mx + 4, my + 4, textoSize.Width + 4, textoSize.Height + 2);
					g.FillRectangle(fundoTexto, bg);
					g.DrawString(texto, f, textoBrush, mx + 6, my + 4);
				}
			}



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
