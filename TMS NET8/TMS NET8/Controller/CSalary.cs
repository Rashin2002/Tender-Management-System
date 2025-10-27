using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS_NET8.Model;

namespace TMS_NET8.Controller
{
    class CSalary
    {
        //Data Grid view data load
        public List<MSalary> GetSalariesByEmployee(string empId)
        {
            return MSalary.GetSalariesByEmployee(empId);
        }

    }
}
