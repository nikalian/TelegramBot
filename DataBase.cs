using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Telegram.Bot.Types;
using System.Reflection.Metadata.Ecma335;

namespace MilanaBot
{
    class DataBase
    {
        string connect = $"Server=88.218.170.159; DataBase=BARBER; User ID=SA; Password={Spravka.PasswordDB}; Integrated Security=False; Encrypt=False";
        public DataTable GetMasters()
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                string cmd = "SELECT * FROM MASTERS";
                SqlCommand command = new SqlCommand(cmd, connection);
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd, connection);
                adapter.Fill(dt);
                connection.Close();
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public DataTable GetServices()
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                string cmd = "SELECT * FROM SERVICES";
                SqlCommand command = new SqlCommand(cmd, connection);
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd, connection);
                adapter.Fill(dt);
                connection.Close();
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string GetSelectedMasterID(string MasterName)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"SELECT ID FROM MASTERS WHERE NAME = '{MasterName}'";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
                return Result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string GetKlientsMessageID(string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                string cmd = $"SELECT ID_MESSAGE FROM KLIENTS WHERE ID_CHAT = '{ChatId}'  ";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
                return Result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public object GetKlientsFullInfo(string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"SELECT* FROM KLIENTS WHERE ID_CHAT = '{ChatId}'  ";
                SqlCommand command = new SqlCommand(cmd, connection);
                DataTable dt = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }
                connection.Close();
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string GetServicesStoimost(string UslugaName)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                string cmd = $"SELECT PRICE FROM SERVICES WHERE NAME LIKE('%{UslugaName}%');";
                SqlCommand command = new SqlCommand(cmd, connection);

                object quryresult = command.ExecuteScalar();
                string result = quryresult.ToString();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string GetMasterNameForKlient(string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"" +
                    $"DECLARE @ID_klnts INT " +
                    $"DECLARE @ID_mstr INT " +
                    $"SET @ID_klnts = (SELECT ID FROM KLIENTS WHERE ID_CHAT = '{ChatId}') " +
                    $"SET @ID_mstr = (SELECT ID_MASTERS FROM RECORD WHERE ID_KLIENT = @ID_klnts) " +
                    $"SELECT NAME FROM MASTERS WHERE ID = @ID_mstr ";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
                return Result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string GetServicesNameForKlient(string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"" +
                    $"DECLARE @ID_klnts INT " +
                    $"DECLARE @ID_srvc INT " +
                    $"SET @ID_klnts = (SELECT ID FROM KLIENTS WHERE ID_CHAT = '{ChatId}') " +
                    $"SET @ID_srvc = (SELECT ID_SERVICES FROM RECORD WHERE ID_KLIENT = @ID_klnts) " +
                    $"SELECT NAME FROM SERVICES WHERE ID = @ID_srvc ";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
                return Result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string GetServicesStoimostForKlient(string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"" +
                    $"DECLARE @ID_klnts INT " +
                    $"DECLARE @ID_srvc INT " +
                    $"SET @ID_klnts = (SELECT ID FROM KLIENTS WHERE ID_CHAT = '{ChatId}') " +
                    $"SET @ID_srvc = (SELECT ID_SERVICES FROM RECORD WHERE ID_KLIENT = @ID_klnts) " +
                    $"SELECT PRICE FROM SERVICES WHERE ID = @ID_srvc ";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
                return Result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public void SetMastersForKlient(string MasterName, string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"" +
                    $"DECLARE @ID_klnts INT; " +
                    $"DECLARE @ID_mstr INT; " +
                    $"SET @ID_mstr = (SELECT ID FROM MASTERS WHERE NAME = '{MasterName}'); " +
                    $"SET @ID_klnts = (SELECT ID FROM KLIENTS WHERE ID_CHAT = '{ChatId}'); " +
                    $"IF(EXISTS(SELECT ID FROM MASTERS WHERE NAME = '{MasterName}')) " +
                    $"BEGIN " +
                    $"IF(EXISTS(SELECT * FROM RECORD WHERE ID_KLIENT = @ID_klnts)) " +
                    $"BEGIN " +
                    $"UPDATE RECORD SET ID_MASTERS = @ID_mstr WHERE ID_KLIENT = @ID_klnts " +
                    $"END " +
                    $"ELSE " +
                    $"BEGIN " +
                    $"INSERT INTO RECORD(ID_MASTERS, ID_KLIENT) VALUES(@ID_mstr, @ID_klnts) " +
                    $"END " +
                    $"END " +
                    $"ELSE " +
                    $"BEGIN " +
                    $"RETURN " +
                    $"END";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public void SetServicesForKlient(string ServiceName, string ChatId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"" +
                    $"DECLARE @ID_klnts INT " +
                    $"DECLARE @ID_servc INT " +
                    $"SET @ID_servc = (SELECT ID FROM SERVICES WHERE NAME = '{ServiceName}') " +
                    $"SET @ID_klnts = (SELECT ID FROM KLIENTS WHERE ID_CHAT = '{ChatId}') " +
                    $"IF(EXISTS(SELECT ID FROM SERVICES WHERE NAME = '{ServiceName}')) " +
                    $"BEGIN " +
                    $"IF(EXISTS(SELECT * FROM RECORD WHERE ID_KLIENT = @ID_klnts)) " +
                    $"BEGIN " +
                    $"UPDATE RECORD SET ID_SERVICES = @ID_servc WHERE ID_KLIENT = @ID_klnts " +
                    $"END " +
                    $"ELSE " +
                    $"BEGIN " +
                    $"RETURN " +
                    $"END " +
                    $"END " +
                    $"ELSE " +
                    $"BEGIN " +
                    $"RETURN " +
                    $"END ";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public void AddKlients(string KlientName, string UserName, string ChatId, string MessageId)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"" +
                    $"IF(EXISTS(SELECT * FROM KLIENTS WHERE USERNAME = '{UserName}')) " +
                    $"BEGIN " +
                    $"  IF(EXISTS(SELECT* FROM KLIENTS WHERE USERNAME = '{UserName}' AND ID_CHAT = '{ChatId}' AND ID_MESSAGE = '{MessageId}')) " +
                    $"  BEGIN " +
                    $"      RETURN " +
                    $"  END " +
                    $"  ELSE " +
                    $"  BEGIN " +
                    $"      IF(EXISTS(SELECT* FROM KLIENTS WHERE ID_CHAT ='{ChatId}' AND USERNAME = '{UserName}')) " +
                    $"      BEGIN" +
                    $"          UPDATE KLIENTS SET ID_MESSAGE = '{MessageId}' WHERE USERNAME = '{UserName}' AND ID_CHAT = '{ChatId}' " +
                    $"      END " +
                    $"      ELSE " +
                    $"      BEGIN " +
                    $"          UPDATE KLIENTS SET ID_CHAT = '{ChatId}' WHERE USERNAME = '{UserName}' " +
                    $"          UPDATE KLIENTS SET ID_MESSAGE = '{MessageId}' WHERE USERNAME = '{UserName}' AND ID_CHAT = '{ChatId}' " +
                    $"      END " +
                    $"  END " +
                    $" END " +
                    $" ELSE " +
                    $" BEGIN " +
                    $"  INSERT INTO KLIENTS(NAME, USERNAME, ID_CHAT, ID_MESSAGE, SCORE) VALUES('{KlientName}', '{UserName}', '{ChatId}', '{MessageId}', '0') " +
                    $" END";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
        public string DropServices(long ChatId, string ServiceName)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"DELETE FROM SERVICES WHERE NAME = '{ServiceName}'";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
                return Result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }

        public void AddKServicesForMass(string[] Elements)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connect);
                connection.Open();
                //Проверка на наличие клиента в базе. Если нету записываем
                string cmd = $"  INSERT INTO SERVICES(NAME, PRICE, KOMMENT) VALUES('{Elements[0]}', '{Elements[1]}', '{Elements[3]}')";
                SqlCommand command = new SqlCommand(cmd, connection);
                object Result = command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                throw;
            }
        }
    }
}
