using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMS_NET8.Database;

namespace TMS_NET8.Model
{
    public class MSalary
    {
        public string? SalaryId { get; set; }
        public string? EmpId { get; set; }
        public double Allowance { get; set; }
        public double GrossSalary { get; set; }
        public double IncomeTaxAmt { get; set; }
        public double AbsenceDeduction { get; set; }
        public string? Month { get; set; }
        public int Year { get; set; }
        public double EPFAmt { get; set; }
        public double ETFAmt { get; set; }
        public double NetSalary { get; set; }

        public static List<MSalary> GetSalariesByEmployee(string empId)
        {
            List<MSalary> salaries = new List<MSalary>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = @"SELECT salary_id, emp_id, allowance, gross_salary, income_tax_amt, 
                                absence_deduction, month, year, epf_amt, etf_amt, net_salary
                         FROM Salary
                         WHERE emp_id = @EmpId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@EmpId", empId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MSalary salary = new MSalary
                    {
                        SalaryId = reader.GetString(0),
                        EmpId = reader.GetString(1),
                        Allowance = reader.GetDouble(2),
                        GrossSalary = reader.GetDouble(3),
                        IncomeTaxAmt = reader.GetDouble(4),
                        AbsenceDeduction = reader.GetDouble(5),
                        Month = reader.GetString(6),
                        Year = reader.GetInt32(7),
                        EPFAmt = reader.GetDouble(8),
                        ETFAmt = reader.GetDouble(9),
                        NetSalary = reader.GetDouble(10)
                    };

                    salaries.Add(salary);
                }
            }

            return salaries;
        }

    }
}
