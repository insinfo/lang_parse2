﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.IO;

namespace LangParse2
{
    class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        //BaseErrorListener implementation
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException("Invalid Expression: {0}", msg, e);
        }

        //IAntlrErrorListener<int> implementation
        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException("Invalid Expression: {0}", msg, e);
        }
    }
}
