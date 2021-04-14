using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LangParse2
{
    class JavaVisitorToSwift : JavaBaseVisitor<string> 
    {
        private RichTextBox rtb = null;
        private string whiteSpace = " ";

        public JavaVisitorToSwift(RichTextBox rtb) 
        {
            this.rtb = rtb;        
        }
        public override string VisitClassBodyDeclaration(JavaParser.ClassBodyDeclarationContext context)
        {
            MessageBox.Show(context.GetText());
            return base.VisitClassBodyDeclaration(context);
        }

        public override string VisitClassBody(JavaParser.ClassBodyContext context)
        {
            MessageBox.Show(context.GetText());
            return base.VisitClassBody(context);
        }

        public override string VisitClassOrInterfaceModifier(JavaParser.ClassOrInterfaceModifierContext context)
        {
            //AddRTBText(context.GetText());
            //MessageBox.Show(context.ChildCount.ToString());
            MessageBox.Show(context.GetText());
            return base.VisitClassOrInterfaceModifier(context);
        }

        public override string VisitClassDeclaration(JavaParser.ClassDeclarationContext context)
        {
            //AddRTBText("class");
            //AddRTBText(context.Identifier().GetText());
           
             //base.VisitClassDeclaration(context);
            MessageBox.Show(context.GetText());
            return context.GetText();
        }

        private void AddRTBText(string text)
        {
            rtb.AppendText(text + whiteSpace);
        }
    }

    
}
