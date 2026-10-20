using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cyber
{
    public class FileSetting
    {
        public string Path { get; set; }

        public List<string> FileTypes { get; set; }

        public Dictionary<string, string> Mime { get; set; }

        /// <summary>
        /// 上传最大值 单位KB 
        /// </summary>
        public int? Size { get; set; }

    }   
}
