using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzexe.Xunlei_Library
{
    public class TaskRecored
    {
        internal  TaskRecored(Prot.TaskBase b)
        {
            TaskId = b.TaskId;
            Type=b.Type;
            Status=(DownloadStatus)b.Status;
            SavePath=b.SavePath?.TrimEnd('\0');
            CreationTime= DateTimeOffset.FromUnixTimeMilliseconds( b.CreationTime).LocalDateTime;
            CompletionTime= DateTimeOffset.FromUnixTimeMilliseconds(b.CompletionTime).LocalDateTime;
            FailureErrorCode=b.FailureErrorCode;
            Name = b.Name?.TrimEnd('\0');
            Url=b.Url?.TrimEnd('\0');
        }


        public Int64 TaskId { get; set; }

        public int Type { get; set; }

        public DownloadStatus Status { get; set; }

        public string? SavePath { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime CompletionTime { get; set; }

        public int FailureErrorCode { get; set; }

        public string? Url { get; set; }

        public string? Name { get; set; } 
    }
}
