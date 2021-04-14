using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Web.Script.Serialization;
using System.Windows;
using System.Text.RegularExpressions;

namespace LangParse2
{
    
    class TextHighLight
    {
        private RichTextBox rtb = null;
        private Paragraph paragraph = null;
        private static IList<ReservedWord> reservedWordList = null;        
        static List<char> specialCharacterList = null;
        static string text;

        private string[] specialWords = { "public", "class", "static" };
        private char[] chrs = { '.', ')', '(', '[', ']', '>', '<', ':', ';', '\n', '\t', '\r' };

        public TextHighLight(ref RichTextBox rtb)
        {
            this.rtb = rtb;
            paragraph = new Paragraph();
            rtb.Document = new FlowDocument(paragraph);

            JavaScriptSerializer jss = new JavaScriptSerializer();
            DictionaryReservedWord javaDic = jss.Deserialize<DictionaryReservedWord>(ReaderTextFile("javaWordList.json"));
            reservedWordList = javaDic.Words;


            specialCharacterList = new List<char>(chrs);
            rtb.TextChanged += rtbSourceLang_TextChanged;

         
            
        }

        public void AddText(string text, string color, bool lineBreack = false)
        {            
            paragraph.Inlines.Add(new Bold(new Run(text))
            {
                Foreground = (Brush) new BrushConverter().ConvertFromString(color)
            });
            if (lineBreack)
            {
                paragraph.Inlines.Add(new LineBreak());
            }
        }

        public string GetAllText()
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
        }

        public string ReaderTextFile(string filePath)
        {
            string result = "";          
            string line;

            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                result += line;               
            }
            file.Close();
            return result;
        }

        //Aqui eu verifico estaticamente se a cadeia que eu passei consta no meu dicionário    
        public static ResultWord IsReservedWord(string word)
        {
            ResultWord result = new ResultWord();
            result.IsExists = false;
                        
            foreach (ReservedWord tag in reservedWordList)
            {
                if (tag.Word == word)
                {
                    result.IsExists = true;
                    result.Color = tag.Color;
                }
            }
            return result;
        }

        public static ResultWord IsString(string input)
        {
            ResultWord res = new ResultWord();
            res.IsExists = false;
            Regex rx = new Regex("\"([^\"]*)\"");
            foreach (Match match in rx.Matches(input))
            {
                res.StartPosition = match.Index;
                res.EndPosition = match.Index + match.Length;
                res.IsExists = true;
                res.Word = match.ToString();
                res.Color = "#D2231D";
            }
            return res;
        }

        private static bool GetSpecials(char i)
        {
            foreach (var item in specialCharacterList)
            {
                if (item.Equals(i))
                {
                    return true;
                }
            }
            return false;
        }
        
        List<ReservedWord> reservedWordsFound = new List<ReservedWord>();
        //Este metodo varre o texto do elemento Run em busca da palavras que estão no dicionario e guarda sua posição
        internal void CheckWordsInRun(Run theRun) // Não destacar palavras-chave neste método
        {
            // Como, vamos através de seu texto e salve todas as guias que temos para salvar.            
            int sIndex = 0;
            int eIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (Char.IsWhiteSpace(text[i]) | GetSpecials(text[i]))
                {
                    if (i > 0 && !(Char.IsWhiteSpace(text[i - 1]) | GetSpecials(text[i - 1])))
                    {
                        eIndex = i - 1;
                        string word = text.Substring(sIndex, eIndex - sIndex + 1);
                        ResultWord isReservedWord = IsReservedWord(word);
                        ResultWord isString = IsString(word);
                        if (isReservedWord.IsExists)
                        {
                            ReservedWord reservedWord = new ReservedWord();
                            reservedWord.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
                            reservedWord.EndPosition = theRun.ContentStart.GetPositionAtOffset(eIndex + 1, LogicalDirection.Backward);
                            reservedWord.Word = word;
                            reservedWord.Color = isReservedWord.Color;
                            reservedWordsFound.Add(reservedWord);
                        }
                        if (isString.IsExists)
                        {
                            ReservedWord reservedWord = new ReservedWord();
                            reservedWord.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
                            reservedWord.EndPosition = theRun.ContentStart.GetPositionAtOffset(eIndex + 1, LogicalDirection.Backward);
                            reservedWord.Word = word;
                            reservedWord.Color = isString.Color;
                            reservedWordsFound.Add(reservedWord);
                        }
                    }
                    sIndex = i + 1;
                }
            }
            // Como isso funciona. Mas espere. 
            //Se a palavra é a última palavra em meu texto eu nunca vou destacá-lo, 
            //devido Estou à procura de separadores. Vamos adicionar alguma correção para este caso 
            string lastWord = text.Substring(sIndex, text.Length - sIndex);

            ResultWord isReservedWordLast = IsReservedWord(lastWord);
            if (isReservedWordLast.IsExists)
            {
                ReservedWord reservedWord = new ReservedWord();
                reservedWord.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
                reservedWord.EndPosition = theRun.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward); 
                reservedWord.Word = lastWord;
                reservedWord.Color = isReservedWordLast.Color;
                reservedWordsFound.Add(reservedWord);
            }
            ResultWord isStringLast = IsString(lastWord);
            if (isStringLast.IsExists)
            {
                ReservedWord reservedWord = new ReservedWord();
                reservedWord.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
                reservedWord.EndPosition = theRun.ContentStart.GetPositionAtOffset(eIndex + 1, LogicalDirection.Backward);
                reservedWord.Word = lastWord;
                reservedWord.Color = isStringLast.Color;
                reservedWordsFound.Add(reservedWord);
            }
        }
        //Metodo chamado pelo evento de mudança de texto
        private void rtbSourceLang_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rtb.Document == null)
                return;
            rtb.TextChanged -= rtbSourceLang_TextChanged;

            reservedWordsFound.Clear();

            //primeiro limpar todos os formatos
            TextRange documentRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            documentRange.ClearAllProperties();
          
            //Agora vamos criar navegador para percorrer o texto, encontrar todas as palavras-chave, mas não destacar
            TextPointer navigator = rtb.Document.ContentStart;
            while (navigator.CompareTo(rtb.Document.ContentEnd) < 0)
            {
                TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
                if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
                {
                    text = ((Run)navigator.Parent).Text;
                    if (text != "")
                    {
                        CheckWordsInRun((Run)navigator.Parent);
                    }
                }
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            }

            //só depois de todas as palavras-chave são encontrados, em seguida, destacamos-los
            for (int i = 0; i < reservedWordsFound.Count; i++)
            {
                try
                {
                    TextRange range = new TextRange(reservedWordsFound[i].StartPosition, reservedWordsFound[i].EndPosition);
                    range.ApplyPropertyValue(TextElement.ForegroundProperty, (SolidColorBrush)(new BrushConverter().ConvertFrom(reservedWordsFound[i].Color)));
                    range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
                catch { }
            }
            rtb.TextChanged += rtbSourceLang_TextChanged;
        }
    }
    class ResultWord
    {
        public bool IsExists { get; set; }
        public string Color { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string Word { get; set; }
    }
    class DictionaryReservedWord
    {
        public IList<ReservedWord> Words { get; set; }
    }
    class ReservedWord
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Color { get; set; }
        public TextPointer StartPosition { get; set; }
        public TextPointer EndPosition { get; set; }
    }
}
