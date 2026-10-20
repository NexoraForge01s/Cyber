using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cyber
{
    public class ParamsSelectLanguage
    {
        private string[] _paramsen { get; set; }
        public void English(params string[] message)
        {
            _paramsen = message;
        }
        private string[] _paramscn { get; set; }
        public void ChineseSimplified(params string[] message)
        {
            _paramscn = message;
        }
        private string[] _paramsct { get; set; }
        public void ChineseTraditional(params string[] message)
        {
            _paramsct = message;
        }

        public (string[], string[], string[]) ReturnMessage()
        {
            return (_paramsen, _paramscn, _paramsct);
        }
    }

    public class SelectLanguage
    {
        public string English { get; set; }

        public string ChineseSimplified { get; set; }

        public string ChineseTraditional { get; set; }
    }
}
