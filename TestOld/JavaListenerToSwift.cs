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
    class JavaListenerToSwift : JavaBaseListener
    {

        private RichTextBox rtb = null;
        private string whiteSpace = " ";
        private string lineBreak = "\r\n";
       

        public JavaListenerToSwift(RichTextBox rtb) 
        {
            this.rtb = rtb;        
        }
        
        public override void EnterClassDeclaration(JavaParser.ClassDeclarationContext context)
        {
            AddRTBText("class " + context.Identifier().GetText() + lineBreak + "{" + lineBreak);
        }

        public override void ExitClassDeclaration(JavaParser.ClassDeclarationContext context)
        {
            AddRTBText("}");
        }

        public override void EnterClassBody(JavaParser.ClassBodyContext context)
        {
            
        }

        public override void ExitClassBody(JavaParser.ClassBodyContext context)
        {
            
        }

        public override void EnterMemberDeclaration(JavaParser.MemberDeclarationContext context)
        {
            //AddRTBText(context.GetText());
        }

        public override void EnterMethodDeclaration(JavaParser.MethodDeclarationContext context)
        {
            AddRTBText(context.GetText());
        }

        public override void EnterConstructorDeclaration(JavaParser.ConstructorDeclarationContext context)
        {
            AddRTBText("init (");
        }

        public override void ExitConstructorDeclaration(JavaParser.ConstructorDeclarationContext context)
        {
            AddRTBText(")" +  lineBreak);
        }
        
        private void AddRTBText(string text)
        {
            rtb.AppendText(text + whiteSpace);
        }

    }
}
