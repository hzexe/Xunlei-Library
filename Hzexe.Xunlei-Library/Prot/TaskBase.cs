using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzexe.Xunlei_Library.Prot
{
    internal class TaskBase
    {
        public Int64 TaskId { get; set; }

        public int Type { get; set; }

        public int Status { get; set; }

        public string? SavePath { get; set; }

        public long CreationTime { get; set; }

        public long CompletionTime { get; set; }

        public int FailureErrorCode { get; set; }

        public string Url { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;


    }
}
