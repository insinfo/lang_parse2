using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.IO;

namespace LangParse2
{
    
    

    public partial class MainWindow : Window
    {
        private const int LANG_TYPE_JAVA = 0;
        private const int LANG_TYPE_SWIFT = 1;
        private TextHighLight textRickSource = null;
        private TextHighLight textRickTarget = null;
        private string inputFileName;

        public MainWindow()
        {
            InitializeComponent();
            if (textRickSource == null) { textRickSource = new TextHighLight(ref rtbSourceLang); }
            if (textRickTarget == null) { textRickTarget = new TextHighLight(ref rtbTargetLang); }

            inputFileName = "testeParse.java";

            LoadStreamIntoRTB(rtbSourceLang);
        }


        private void btnStartParse_Click(object sender, RoutedEventArgs e)
        {
            
            /*
            StreamReader inputStream = new StreamReader(inputFileName);            
            AntlrInputStream input = new AntlrInputStream(inputStream.ReadToEnd());
            JavaLexer lexer = new JavaLexer(input);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            JavaParser parser = new JavaParser(tokens);
            parser.AddErrorListener(new ThrowExceptionErrorListener());   
            IParseTree tree = parser.compilationUnit();
            //rtbSourceLang.AppendText(tree.ToStringTree(parser));           
           // JavaVisitorToSwift visitor = new JavaVisitorToSwift(rtbTargetLang);
            //visitor.Visit(tree);           
            ParseTreeWalker.Default.Walk(new JavaListenerToSwift(rtbTargetLang), tree);
            inputStream.Close();*/


            StreamReader inputStream = new StreamReader(inputFileName);
            AntlrInputStream input = new AntlrInputStream(inputStream.ReadToEnd());
            Java8Lexer lexer = new Java8Lexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            Java8Parser parser = new Java8Parser(tokens);
            IParseTree tree = parser.compilationUnit();
            ParseTreeWalker walker = new ParseTreeWalker();
            J2SwiftListener swiftListener = new J2SwiftListener(tokens);
            walker.Walk(swiftListener, tree);       
            rtbTargetLang.AppendText(swiftListener.rewriter.GetText());
        }

        public void LoadStreamIntoRTB(RichTextBox rtb)
        {           
            TextRange range;
            FileStream fStream;
            if (File.Exists(inputFileName))
            {
                range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                fStream = new FileStream(inputFileName, FileMode.OpenOrCreate);
                range.Load(fStream, DataFormats.Text);
                fStream.Close();
            }
          
        }

       
    }

}
