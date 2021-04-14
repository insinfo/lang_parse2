using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.IO;
using System.Windows;

namespace LangParse2
{   
    public class J2SwiftListener : Java8BaseListener
    {
        internal CommonTokenStream tokens;
        internal TokenStreamRewriter rewriter;

        internal bool inConstructor;
        internal int formalParameterPosition;

        // Some basic type mappings
        internal static IDictionary<string, string> typeMap = new Dictionary<string, string>();
       
        // Some basic modifier mappings (others in context)
        internal static IDictionary<string, string> modifierMap = new Dictionary<string, string>();

        public J2SwiftListener(CommonTokenStream tokens)
        {
            typeMap["String"] = "String";
            typeMap["float"] = "Float";
            typeMap["Float"] = "Float";
            typeMap["int"] = "Int";
            typeMap["Int"] = "Int";
            typeMap["Integer"] = "Int";
            typeMap["long"] = "Int64";
            typeMap["Long"] = "Int64";
            typeMap["boolean"] = "Bool";
            typeMap["Boolean"] = "Bool";
            typeMap["Map"] = "Dictionary";
            typeMap["HashSet"] = "Set";
            typeMap["HashMap"] = "Dictionary";
            typeMap["List"] = "Array";
            typeMap["ArrayList"] = "Array";

            modifierMap["protected"] = "internal";
            modifierMap["volatile"] = "/*volatile*/";
            modifierMap["public"] = "public";
            modifierMap["private"] = "private";

          

            this.tokens = tokens;
            this.rewriter = new TokenStreamRewriter(tokens);
        }

        internal Java8Parser.UnannTypeContext unannType;
        public override void EnterFieldDeclaration(Java8Parser.FieldDeclarationContext ctx)
        {
            //:	fieldModifier* unannType variableDeclaratorList ';'
            // Store the unannType for the variableDeclarator (convenience)
            unannType = ctx.unannType();
        }
        public override void ExitFieldDeclaration(Java8Parser.FieldDeclarationContext ctx)
        {
            // replace on exit because the unannType rules will rewrite it
            replace(ctx.unannType(), "var");
            unannType = null;
        }

        public override void EnterLocalVariableDeclaration(Java8Parser.LocalVariableDeclarationContext ctx)
        {
            //:	variableModifier* unannType variableDeclaratorList
            unannType = ctx.unannType();
        }
        public override void ExitLocalVariableDeclaration(Java8Parser.LocalVariableDeclarationContext ctx)
        {
            replace(ctx.unannType(), "var");
            unannType = null;
        }

        public override void EnterConstantDeclaration(Java8Parser.ConstantDeclarationContext ctx)
        {
            //:	constantModifier* unannType variableDeclaratorList ';'
            unannType = ctx.unannType();
        }
        public override void ExitConstantDeclaration(Java8Parser.ConstantDeclarationContext ctx)
        {
            replace(ctx.unannType(), "var");
            unannType = null;
        }

        public override void ExitVariableDeclarator(Java8Parser.VariableDeclaratorContext ctx)
        {
            //:	variableDeclaratorId ('=' variableInitializer)?
            // We could search the parent contexts for unannType but since we have to remove it anyway we store it.
            // Use the rewritten text, not the original.
            // todo: not sure what's up here, crashing on lambdas
            try
            {
                rewriter.InsertAfter(ctx.variableDeclaratorId().stop, " : " + getText(unannType));
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public override void EnterConstructorDeclaration(Java8Parser.ConstructorDeclarationContext ctx)
        {
            //:	constructorModifier* constructorDeclarator throws_? constructorBody
            // Search children of constructorBody for any explicit constructor invocations
           /* IList<Java8Parser.ExplicitConstructorInvocationContext> eci = ctx.constructorBody().getRuleContexts(typeof(Java8Parser.ExplicitConstructorInvocationContext));
            if (eci.Count > 0)
            {
                rewriter.insertBefore(ctx.constructorDeclarator().start, "convenience ");
            }*/
        }

        public override void EnterConstructorDeclarator(Java8Parser.ConstructorDeclaratorContext ctx)
        {
            //:	typeParameters? simpleTypeName '(' formalParameterList? ')'
            replace(ctx.simpleTypeName(), "init");
            inConstructor = true;
        }

        public override void ExitConstructorDeclaration(Java8Parser.ConstructorDeclarationContext ctx)
        {
            inConstructor = false;
        }

        public override void EnterFormalParameterList(Java8Parser.FormalParameterListContext ctx)
        {
            // called from methodDeclarator
            //:	formalParameters ',' lastFormalParameter
            //    |	lastFormalParameter
            formalParameterPosition = 0;
        }

        public override void EnterFormalParameters(Java8Parser.FormalParametersContext ctx)
        {
            // called from formalParameterList
            //:	formalParameter (',' formalParameter)*
            //    |	receiverParameter (',' formalParameter)*
            formalParameterPosition = 0;
        }

        public override void ExitFormalParameter(Java8Parser.FormalParameterContext ctx)
        {
            rewriter.InsertAfter(ctx.variableDeclaratorId().stop, " : " + getText(ctx.unannType()));

            //:	variableModifier* unannType variableDeclaratorId
            if (formalParameterPosition++ > 0 || inConstructor)
            {
                replace(ctx.unannType(), "_");
            }
            else
            {
                removeRight(ctx.unannType());
            }
        }

        public override void ExitMethodHeader(Java8Parser.MethodHeaderContext ctx)
        {
            //:	result methodDeclarator throws_?
            //|	typeParameters annotation* result methodDeclarator throws_?

            if (!ctx.result().GetText().Equals("void"))
            {
                rewriter.InsertAfter(ctx.methodDeclarator().stop, " -> " + getText(ctx.result()));
            }
            replace(ctx.result(), "func");
        }

        public override void EnterPackageDeclaration(Java8Parser.PackageDeclarationContext ctx)
        {
            rewriter.InsertBefore(ctx.start, "// ");
        }

        public override void EnterPrimaryNoNewArray_lfno_primary(Java8Parser.PrimaryNoNewArray_lfno_primaryContext ctx)
        {
            if (ctx.GetText().Equals("this"))
            {
                replace(ctx, "self");
            }
        }

        public override void EnterFieldModifier(Java8Parser.FieldModifierContext ctx)
        {
            // changed in 1.2
            //if ( ctx.getText().equals( "static" )) { replace( ctx, "class" ); }
        }
        public override void EnterMethodModifier(Java8Parser.MethodModifierContext ctx)
        {
            if (ctx.GetText().Equals("static"))
            {
                replace(ctx, "class");
            }
        }

        public override void EnterLiteral(Java8Parser.LiteralContext ctx)
        {
            //IntegerLiteral
            //        |	FloatingPointLiteral
            //        |	BooleanLiteral
            //        |	CharacterLiteral
            //        |	StringLiteral
            //        |	NullLiteral
            if (ctx.GetText().Equals("null"))
            {
                replace(ctx, "nil");
            }
            else
            {
                if (ctx.FloatingPointLiteral() != null)
                {
                    string text = ctx.GetText();
                    if (text.ToLower().EndsWith("f", StringComparison.Ordinal))
                    {
                        text = text.Substring(0, text.Length - 1);
                        replace(ctx, text);
                    }
                }
            }
        }


        public override void ExitClassInstanceCreationExpression(Java8Parser.ClassInstanceCreationExpressionContext ctx)
        {
            //:	'new' typeArguments? annotation* Identifier ('.' annotation* Identifier)* typeArgumentsOrDiamond? '(' argumentList? ')' classBody?
            //|	expressionName '.' 'new' typeArguments? annotation* Identifier typeArgumentsOrDiamond? '(' argumentList? ')' classBody?
            //|	primary '.' 'new' typeArguments? annotation* Identifier typeArgumentsOrDiamond? '(' argumentList? ')' classBody?
            if (ctx.start.Text.Equals("new"))
            {
                replaceFirst(ctx, Java8Lexer.Identifier, mapType(ctx.Identifier()[0].GetText()));
                rewriter.Delete(ctx.start);
                rewriter.Delete(ctx.start.TokenIndex + 1); // space
            }
        }

        public override void EnterClassInstanceCreationExpression_lfno_primary(Java8Parser.ClassInstanceCreationExpression_lfno_primaryContext ctx)
        {
            //:	'new' typeArguments? annotation* Identifier ('.' annotation* Identifier)* typeArgumentsOrDiamond? '(' argumentList? ')' classBody?
            //|	expressionName '.' 'new' typeArguments? annotation* Identifier typeArgumentsOrDiamond? '(' argumentList? ')' classBody?
            if (ctx.start.Text.Equals("new"))
            {                                                           
                replaceFirst(ctx, Java8Lexer.Identifier, mapType(ctx.Identifier()[0].GetText()));
                rewriter.Delete(ctx.start);
                rewriter.Delete(ctx.start.TokenIndex + 1); // space
            }
        }

        public override void EnterThrowStatement(Java8Parser.ThrowStatementContext ctx)
        {
            //:	'throw' expression ';'
            rewriter.InsertBefore(ctx.start, "throwException() /* ");
            rewriter.InsertAfter(ctx.stop, " */");
        }

        public override void EnterCastExpression(Java8Parser.CastExpressionContext ctx)
        {
            //:	'(' primitiveType ')' unaryExpression
            //    |	'(' referenceType additionalBound* ')' unaryExpressionNotPlusMinus
            //    |	'(' referenceType additionalBound* ')' lambdaExpression
            if (ctx.primitiveType() != null)
            {
                replace(ctx.primitiveType(), mapType(ctx.primitiveType()));
            }
        }

        public override void ExitUnannType(Java8Parser.UnannTypeContext ctx)
        {
            // mapping may already have been done by more specific rule but this shouldn't hurt it
            // todo: this needs to be more specific, preventing rewrites on generic type args
            //if ( !ctx.getText().contains( "<" ) && !ctx.getText().contains( "[" )) {
            replace(ctx, mapType(getText(ctx)));
            //}
        }

        public override void ExitArrayType(Java8Parser.ArrayTypeContext ctx)
        {
            //:	primitiveType dims
            //|	classOrInterfaceType dims
            //|	typeVariable dims
            ParserRuleContext rule;
            if (ctx.primitiveType() != null)
            {
                rule = ctx.primitiveType();
            }
            else if (ctx.classOrInterfaceType() != null)
            {
                rule = ctx.classOrInterfaceType();
            }
            else
            {
                rule = ctx.typeVariable();
            }
            replace(ctx, "[" + mapType(rule) + "]");
        }

        public override void ExitUnannArrayType(Java8Parser.UnannArrayTypeContext ctx)
        {
            //:	unannPrimitiveType dims
            //|	unannClassOrInterfaceType dims
            //|	unannTypeVariable dims
            ParserRuleContext rule;
            if (ctx.unannPrimitiveType() != null)
            {
                rule = ctx.unannPrimitiveType();
            }
            else if (ctx.unannClassOrInterfaceType() != null)
            {
                rule = ctx.unannClassOrInterfaceType();
            }
            else
            {
                rule = ctx.unannTypeVariable();
            }
            replace(ctx, "[" + mapType(rule) + "]");
        }

        public override void EnterExplicitConstructorInvocation(Java8Parser.ExplicitConstructorInvocationContext ctx)
        {
            //:	typeArguments? 'this' '(' argumentList? ')' ';'
            //    |	typeArguments? 'super' '(' argumentList? ')' ';'
            //    |	expressionName '.' typeArguments? 'super' '(' argumentList? ')' ';'
            //    |	primary '.' typeArguments? 'super' '(' argumentList? ')' ';'
            IList<ITerminalNode> thisTokens = ctx.GetTokens(Java8Lexer.THIS);
            if (thisTokens != null && thisTokens.Count > 0)
            {
                rewriter.Replace(thisTokens[0].Symbol.TokenIndex, "self.init");
            }
            
        }

        public override void EnterImportDeclaration(Java8Parser.ImportDeclarationContext ctx)
        {
            rewriter.InsertBefore(ctx.start, "// ");
        }

        public override void EnterSuperclass(Java8Parser.SuperclassContext ctx)
        {
            //:	'extends' classType
            replaceFirst(ctx, Java8Lexer.EXTENDS, " : ");
        }

        public override void EnterSuperinterfaces(Java8Parser.SuperinterfacesContext ctx)
        {
            //:	'implements' interfaceTypeList
            replaceFirst(ctx, Java8Lexer.IMPLEMENTS, " : ");
        }

        public override void ExitFieldModifier(Java8Parser.FieldModifierContext ctx)
        {
            replace(ctx, mapModifier(ctx));
        }
        public override void ExitMethodModifier(Java8Parser.MethodModifierContext ctx)
        {
            replace(ctx, mapModifier(ctx));
        }
        public override void ExitClassModifier(Java8Parser.ClassModifierContext ctx)
        {
            replace(ctx, mapModifier(ctx));
        }

        public override void EnterNormalInterfaceDeclaration(Java8Parser.NormalInterfaceDeclarationContext ctx)
        {
            //:	interfaceModifier* 'interface' Identifier typeParameters? extendsInterfaces? interfaceBody
            IList<ITerminalNode> intfTokens = ctx.GetTokens(Java8Lexer.INTERFACE);
            rewriter.Replace(intfTokens[0].Symbol.TokenIndex, "protocol");
        }

        public override void ExitBasicForStatement(Java8Parser.BasicForStatementContext ctx)
        {
            //:	'for' '(' forInit? ';' expression? ';' forUpdate? ')' statement
            deleteFirst(ctx, Java8Lexer.RPAREN);
            replaceFirst(ctx, Java8Lexer.LPAREN, " "); // todo: should check spacing here
            if (!ctx.statement().start.Text.Equals("{"))
            {
                rewriter.InsertBefore(ctx.statement().start, "{ ");
                rewriter.InsertAfter(ctx.statement().stop, " }");
            }
        }

        public override void ExitWhileStatement(Java8Parser.WhileStatementContext ctx)
        {
            //:	'while' '(' expression ')' statement
            deleteFirst(ctx, Java8Lexer.RPAREN);
            deleteFirst(ctx, Java8Lexer.LPAREN);
            if (!ctx.statement().start.Text.Equals("{"))
            {
                rewriter.InsertBefore(ctx.statement().start, "{ ");
                rewriter.InsertAfter(ctx.statement().stop, " }");
            }
        }

        public override void ExitMethodInvocation(Java8Parser.MethodInvocationContext ctx)
        {
            // todo: make a map for these
            if (ctx.GetText().StartsWith("System.out.println"))
            {
                replace(ctx, "println(" + getText(ctx.argumentList()) + ")");
            }
        }

        public override void ExitEnhancedForStatement(Java8Parser.EnhancedForStatementContext ctx)
        {
            //:	'for' '(' variableModifier* unannType variableDeclaratorId ':' expression ')' statement
            if (!ctx.statement().start.Text.Equals("{"))
            {
                rewriter.InsertBefore(ctx.statement().start, "{ ");
                rewriter.InsertAfter(ctx.statement().stop, " }");
            }
            string st = getText(ctx.statement());

            string @out = "for " + getText(ctx.variableDeclaratorId()) + " : " + getText(ctx.unannType()) + " in " + getText(ctx.expression()) + " " + st;

            replace(ctx, @out);

        }

        public override void ExitUnannClassType_lfno_unannClassOrInterfaceType(Java8Parser.UnannClassType_lfno_unannClassOrInterfaceTypeContext ctx)
        {
            //unannClassType_lfno_unannClassOrInterfaceType
            //:	Identifier typeArguments?
            replaceFirst(ctx, ctx.Identifier().Symbol.Type, mapType(ctx.Identifier().GetText()));
        }

        public override void ExitRelationalExpression(Java8Parser.RelationalExpressionContext ctx)
        {
            //:	shiftExpression
            //    |	relationalExpression '<' shiftExpression
            //    |	relationalExpression '>' shiftExpression
            //    |	relationalExpression '<=' shiftExpression
            //    |	relationalExpression '>=' shiftExpression
            //    |	relationalExpression 'instanceof' referenceType
            replaceFirst(ctx, Java8Lexer.INSTANCEOF, "is");
        }

        //
        // util
        //
        private void deleteFirst(ParserRuleContext ctx, int token)
        {
            IList<ITerminalNode> tokens = ctx.GetTokens(token);
            rewriter.Delete(tokens[0].Symbol.TokenIndex);
        }
        private void replaceFirst(ParserRuleContext ctx, int token, string str)
        {
            IList<ITerminalNode> tokens = ctx.GetTokens(token);
            if (tokens == null || tokens.Count == 0)
            {
                return;
            }
            rewriter.Replace(tokens[0].Symbol.TokenIndex, str);
        }


        // Get possibly rewritten text
        private string getText(ParserRuleContext ctx)
        {
            if (ctx == null)
            {
                return "";
            }
            return rewriter.GetText(new Interval(ctx.start.TokenIndex, ctx.stop.TokenIndex));
        }

        private void replace(ParserRuleContext ctx, string s)
        {
            rewriter.Replace(ctx.start, ctx.stop, s);
        }

        // remove context and hidden tokens to right
        private void removeRight(ParserRuleContext ctx)
        {
            rewriter.Delete(ctx.start, ctx.stop);
            IList<IToken> htr = tokens.GetHiddenTokensToRight(ctx.stop.TokenIndex);
            foreach (IToken token in htr)
            {
                rewriter.Delete(token);
            }
        }

        public virtual string mapType(ParserRuleContext ctx)
        {
            //if ( ctx instanceof Java8Parser.UnannArrayTypeContext ) { }
            //String text = ctx.getText();
            string text = getText(ctx);
            return mapType(text);
        }
        public virtual string mapType(string text)
        {
            //MessageBox.Show(text);
            string mapText = typeMap[text];
            return string.ReferenceEquals(mapText, null) ? text : mapText;
        }

        public virtual string mapModifier(ParserRuleContext ctx)
        {
            //if ( ctx instanceof Java8Parser.UnannArrayTypeContext ) { }
            //String text = ctx.getText();
            string text = getText(ctx);
            return mapModifier(text);
        }
        public virtual string mapModifier(string text)
        {
            //MessageBox.Show(text);
            string mapText = modifierMap[text];
            return string.ReferenceEquals(mapText, null) ? text : mapText;
        }
    }


}
