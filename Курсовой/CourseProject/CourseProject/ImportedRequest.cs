
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject
{
    public class ImportedRequest
    {
        public string ClientName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int CountersNumber { get; set; }
        public string Comment { get; set; }
        public string Master { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }

        public ImportedRequest()
        {
            RequestDate = DateTime.Now;
            Status = "На рассмотрении";
            CountersNumber = 1;
        }
    }
}
