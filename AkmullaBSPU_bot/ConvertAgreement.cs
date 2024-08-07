using Word = Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AkmullaBSPU_bot
{
    internal class ConvertAgreement
    {
        private FileInfo _fileInfo;

        public ConvertAgreement(string fileName)
        {
            if (File.Exists(fileName))
            {
                _fileInfo = new FileInfo(fileName);
            }
            else
            {
                throw new ArgumentException("File not found");
            }
        }

        internal bool StartConvert(string id)
        {
            Word.Application appWord = null;
            try
            {
                string path = String.Format(@"C:\Users\SiBro\source\repos\AkmullaBSPU_bot\AkmullaBSPU_bot\bin\Debug\netcoreapp3.1\{0}", id);
                var di = Directory.CreateDirectory(path);

                appWord = new Word.Application();
                var wordDocument = appWord.Documents.Open(@$"C:\Users\SiBro\source\repos\AkmullaBSPU_bot\AkmullaBSPU_bot\bin\Debug\netcoreapp3.1\{id}Agreement.docx");
                wordDocument.ExportAsFixedFormat(path + @"\Agreement.pdf", Word.WdExportFormat.wdExportFormatPDF);

                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally
            {
                if (appWord != null)
                {
                    appWord.Quit();
                }
            }

            return false;
        }
    }
}
