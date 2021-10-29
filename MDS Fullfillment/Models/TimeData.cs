using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Data.SqlClient;

namespace MDS_Fullfillment.Models
{
    /// <summary>
    /// Static repository of the Time Collection Interface Data for dropdowns
    /// </summary>
    public static class TimeInterface
    {
        public static List<User> Users = new List<User>();
        public static List<Account> Accounts = new List<Account>();
        public static List<Task> Tasks = new List<Task>();

        public static void InterfaceSync()
        {
            while (true)
            {
                //pull data from SQL
                DataCon.GetTimeInterfaceData(); //udpates the class properties
                Thread.Sleep(10000); //10 second delay
            }
        }
       
    }
    /// <summary>
    /// Non static object to allow for serialization of the static time interface class
    /// </summary>
    public class TimeInterfaceJSON
    {
        public List<User> Users { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Task> Tasks { get; set; }

        public TimeInterfaceJSON()
        {
            this.Users = TimeInterface.Users;
            this.Accounts = TimeInterface.Accounts;
            this.Tasks = TimeInterface.Tasks;
        }
    }
    public static class DataCon
    {
        private static string conStr = @"Data Source=DESKTOP-R4OM28D\SQLEXPRESS;Initial Catalog=Aeragen;User ID=sa;Password=Electronics1!";
        //private static string conStr = @"Data Source=PRD-V-SQL-01;Initial Catalog=MDS;User ID=bchaikin;Password=brian12";

        private static SqlConnection con = new SqlConnection(conStr);
        public static void GetTimeInterfaceData()
        {
            try
            {
                SQLGetAccounts();

                SQLGetUsers();

                SQLGetTasks();
            }
            catch
            {

            }
            


        }

        public static void SQLGetAccounts()
        {
            List<Account> accountsTemp = new List<Account>();

            lock (con)
            {
                con.Open();

                //Query each of the three tables and populate the lists
                try
                {
                    //verify table name
                    string query = @"Select * from Accounts";

                    //string query = @"Select Rtrim(AccountNumber), Rtrim(AccountName) From DC00MDS.dbo.Client Where Status = 23 Order By AccountName";
                    
                    SqlCommand cmd = new SqlCommand(query, con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Account account = new Account(int.Parse(reader[0].ToString()), reader[1].ToString());
                        accountsTemp.Add(account);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                

                con.Close();

                lock (TimeInterface.Accounts)
                {
                    TimeInterface.Accounts = accountsTemp;
                }

            }
        }
        public static void SQLGetUsers()
        {
            List<User> usersTemp = new List<User>();

            lock (con)
            {
                con.Open();

                //Query each of the three tables and populate the lists
                try
                {
                    string query = @"Select * From Users";

                    //string query = @"Select EmployeeNo, Employee_NameFirst +' '+ Employee_NameLast From MDS.dbo.MDS_Payroll_Employee Order By Employee_NameFirst";
                    SqlCommand cmd = new SqlCommand(query, con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        User account = new User(int.Parse(reader[0].ToString()), reader[1].ToString());
                        usersTemp.Add(account);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                

                con.Close();


                lock (TimeInterface.Users)
                {
                    TimeInterface.Users = usersTemp;
                }
               
            }
        }
        public static void SQLGetTasks()
        {
            List<Task> tasksTemp = new List<Task>();

            lock (con)
            {
                con.Open();

                //Query each of the three tables and populate the lists
                try
                {
                    string query = @"Select * From Tasks ";

                    //string query = @"Select Code, Description From MDS.dbo.MDS_Production_Codes Where Prod_Group = 'Warehouse' Order By Code";
                    SqlCommand cmd = new SqlCommand(query, con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Task task = new Task(int.Parse(reader[0].ToString()), reader[1].ToString());
                        tasksTemp.Add(task);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                

                con.Close();


                lock (TimeInterface.Tasks)
                {
                    TimeInterface.Tasks = tasksTemp;
                }

            }
        }

        public static void SQLSubmitTimeData(bool endOfDay, TimeRecord timeRecord)
        {
            if(endOfDay == true)
            {
                //only update the existing record with the stop time
                SQLUpdateStopTime(timeRecord);

            }
            else
            {
                //update previous record with a stop time
                SQLUpdateStopTime(timeRecord);
                //update new record with start time
                SQLInsertStartTime(timeRecord);
            }

        }

        public static void SQLUpdateStopTime(TimeRecord timeRecord)
        {
            try
            {
                con.Open();
                try
                {
                    string query = "update dbo.MDS_Production_WebApp set EndTime = @Time where EndTime is null and UserID = @UserID";
                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@UserID", timeRecord.user);
                    cmd.Parameters.AddWithValue("@Time", timeRecord.Time);

                    int check = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                con.Close();
            }
            catch(Exception e)
            {

            }
            
        }
        public static void SQLInsertStartTime(TimeRecord timeRecord)
        {
            try
            {
                con.Open();
                try
                {
                    string query = "Insert into dbo.MDS_Production_WebApp (Task, Account, UserID, StartTime) values (@Task, @AccNum, @UserID, @Time) ";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Task", timeRecord.task);
                    cmd.Parameters.AddWithValue("@AccNum", timeRecord.account);
                    cmd.Parameters.AddWithValue("@UserID", timeRecord.user);
                    cmd.Parameters.AddWithValue("@Time", timeRecord.Time);

                    int check = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                con.Close();
            }
            catch(Exception e)
            {

            }
            
        }
    }
    public class Account
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public Account(int ID, string Name)
        {
            AccountID = ID;
            AccountName = Name;
        }
    }
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public User(int ID, string Name)
        {
            UserID = ID;
            UserName = Name;
        }

    }
    public class Task
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public Task(int ID, string Name)
        {
            TaskID = ID;
            TaskName = Name;
        }

    }
    public class TimeRecord
    {
        public DateTime Time { get; set; }
        
        public int account { get; set; }
        public int user { get; set; }
        public int task { get; set; }

        public TimeRecord()
        {

            this.Time = DateTime.Now;


        }

    }

}
