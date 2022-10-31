using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKT_STATUS_WEB.Models
{
    public class Log_level
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class Log_log
    {
        public int Id { get; set; }
        public Log_level? Level { get; set; }
        public string? Description { get; set; }
    }


}
