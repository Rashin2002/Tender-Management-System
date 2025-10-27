using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS_NET8.Model
{
    public class MEmployee
    {
        public string EmpId { get; set; } = string.Empty;
        public string EmpFullName { get; set; } = string.Empty;
        public double BasicSalary { get; set; }

        // Fetch employee by ID
        public static MEmployee? GetEmployeeById(string empId)
        {
            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = @"SELECT emp_id, emp_full_name, emp_basic_salary 
                             FROM Employee 
                             WHERE emp_id = @EmpId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EmpId", empId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new MEmployee
                    {
                        EmpId = reader.GetString(0),
                        EmpFullName = reader.GetString(1),
                        BasicSalary = reader.GetDouble(2)
                    };
                }
            }

            return null;
        }
    }

}
