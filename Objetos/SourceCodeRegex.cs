using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LangParse2
{
   
    class SourceCodeRegex
    {      

        private List<string> _comments = new List<string>();
        private List<string> _quoted = new List<string>();
        private List<string> _numeric = new List<string>();
        private List<string> _keywords = new List<string>();
        private List<string> _identifierTypes = new List<string>();
        private List<string> _custom = new List<string>();

        public string blockComments = @"/\*(.*?)\*/";
        public string lineComments = @"//(.*?)\r?\n";
        public string strings = @"""((\\[^\n]|[^""\n])*)""";
        public string verbatimStrings = @"@(""[^""]*"")+";

        #region Properties

        public List<string> Comments
        {
            get { return _comments; }
        }

        public List<string> Quoted
        {
            get { return _quoted; }
        }

        public List<string> Numeric
        {
            get { return _numeric; }
        }

        public List<string> Keywords
        {
            get { return _keywords; }
        }

        public List<string> IdentifierTypes
        {
            get { return _identifierTypes; }
        }

        public List<string> Custom
        {
            get { return _custom; }
        }

        #endregion

        #region ctor

        public SourceCodeRegex()
        {
            Initialize();
        }

        #endregion

        #region Methods
        private void Initialize()
        {
            _comments.Add("//");
            _comments.Add(@"/\*(?:(?!\*/)(?:.|[\r\n]+))*\*/");

            _quoted.Add(@"([""'])(?:\\\1|.)*?\1");

            _numeric.Add(@"\b\d+\b");

            _keywords.Add(@"\bif\b");
            _keywords.Add(@"\belse\b");
            _keywords.Add(@"\bforeach\b");
            _keywords.Add(@"\bswitch\b");
            _keywords.Add(@"\bcase\b");
       

            _identifierTypes.Add(@"\bint\b");
            _identifierTypes.Add(@"\bdate\b");
            _identifierTypes.Add(@"\bstring\b");
      

        }

        public string StripComments(string input)
        {
            string noComments = Regex.Replace(input,blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
            me => {
            if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                return me.Value.StartsWith("//") ? Environment.NewLine : "";
            // Keep the literal strings
            return me.Value;
            },
            RegexOptions.Singleline);

            return noComments;
        }

        static string StripComments2(string code)
        {
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(code, re, "$1");
        }

        #endregion

    };

}
