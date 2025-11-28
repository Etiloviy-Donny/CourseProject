using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject.Models
{
    public class RequestWithPhone
    {
        public int IdRequest { get; set; }
        public System.DateTime RequestDate { get; set; }
        public string Address { get; set; }
        public string UserPhone { get; set; } // Вместо IdUser
        public string CountersNumber { get; set; }
        public string Comment { get; set; }
        public string Master { get; set; }
        public string Status { get; set; }
    }
}
