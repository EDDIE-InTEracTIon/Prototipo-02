using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;




namespace PrototipoLecturaPdf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                axAcroPDF1.src = openFileDialog.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri(axAcroPDF1.src);
            List<int> pages = ReadPdfFile(uri.LocalPath,textBox2.Text);
            textBox3.Text = String.Join(",",pages.ToArray());                   
        }


        public List<int> ReadPdfFile(string fileName, String searthText)
        {
            List<int> pages = new List<int>();
            bool existe;
            existe = File.Exists(fileName);
            if (existe)
            {
                PdfReader pdfReader = new PdfReader(fileName);
                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    if (currentPageText.Contains(searthText))
                    {
                        pages.Add(page);
                    }
                }
                pdfReader.Close();
            }
            return pages;
        }

        private static void ExtractTextFromPdf(string newFileNameWithImageAndText, string extractedTextFileName)
        {
            using (Stream newpdfStream = new FileStream(newFileNameWithImageAndText, FileMode.Open, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(newpdfStream);
                string text = PdfTextExtractor.GetTextFromPage(pdfReader, 1, new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy());
                File.WriteAllText(extractedTextFileName, text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri(axAcroPDF1.src);
            ExtractTextFromPdf(uri.LocalPath, "prueba.txt");
            //StreamReader leer = new StreamReader("C:/Users/ccont/Source/Repos/Proyecto-Titulo/PrototipoLecturaPdf/PrototipoLecturaPdf/bin/Debug/prueba.txt");
            StreamReader leer = new StreamReader("C:/Users/ccont/Source/Repos/Proyecto-Titulo/P3_PrototipoLecturaPdf/PrototipoLecturaPdf/bin/Debug/prueba.txt");
        
           String linea;
            try
            {
                linea = leer.ReadLine();
                while (linea != null)
                {
                    richTextBox1.AppendText(linea + "\n");
                    linea = leer.ReadLine();
                }
            }
            catch
            {
                MessageBox.Show("Error leer archivo prueba");
            }

            leer.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri(axAcroPDF1.src);
            if (uri.LocalPath != null)
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                var extension = System.IO.Path.GetExtension(uri.LocalPath);

                var filePath = System.IO.Path.GetDirectoryName(uri.LocalPath);

                var finalPath = System.IO.Path.Combine(filePath, $"{fileName}_annotated.{extension}");


                this.HighlightPDFAnnotation(uri.LocalPath, finalPath, 1, this.textBox2.Text, this.textBox1.Text);
            }
        }

        private void HighlightPDFAnnotation(string inputFile, string highLightFile, int pageno, params string[] textToAnnotate)
        {
            PdfReader reader = new PdfReader(inputFile);
            using (FileStream fs = new FileStream(highLightFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (PdfStamper stamper = new PdfStamper(reader, fs))
                {
                    MyLocationTextExtractionStrategy strategy = new MyLocationTextExtractionStrategy();
                    strategy.UndercontentHorizontalScaling = 100;

                    string currentText = PdfTextExtractor.GetTextFromPage(reader, pageno, strategy);
                    for (int i = 0; i < textToAnnotate.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(textToAnnotate[i])) { continue; }
                        var lstMatches = strategy.GetTextLocations(textToAnnotate[i].Trim(), StringComparison.CurrentCultureIgnoreCase);
                        if (!this.checkBox1.Checked)
                            lstMatches = lstMatches.Take(1).ToList();
                        foreach (iTextSharp.text.Rectangle rectangle in lstMatches)
                        {
                            float[] quadPoints = { rectangle.Left - 3.0f,
                                             rectangle.Bottom,
                                             rectangle.Right,
                                             rectangle.Bottom,
                                             rectangle.Left - 3.0f,
                                             rectangle.Top + 1.0f,
                                             rectangle.Right,
                                             rectangle.Top + 1.0f
                                          };


                            PdfAnnotation highlight = PdfAnnotation.CreateMarkup(stamper.Writer
                                                            , rectangle, null
                                                            , PdfAnnotation.MARKUP_HIGHLIGHT, quadPoints);
                            highlight.Color = BaseColor.YELLOW;


                            PdfGState state = new PdfGState();
                            state.BlendMode = new PdfName("Multiply");


                            PdfAppearance appearance = PdfAppearance.CreateAppearance(stamper.Writer, rectangle.Width, rectangle.Height);

                            appearance.SetGState(state);
                            appearance.Rectangle(0, 0, rectangle.Width, rectangle.Height);
                            appearance.SetColorFill(BaseColor.YELLOW);
                            appearance.Fill();

                            highlight.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, appearance);

                            //añadir anotacion
                            stamper.AddAnnotation(highlight, pageno);
                        }
                    }
                }
            }
            reader.Close();
            axAcroPDF1.src = highLightFile;
        }



    }
}
