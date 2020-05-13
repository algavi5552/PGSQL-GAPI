using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Data.Common;
using System.IO;

namespace ConsoleApp9
{
    public class Postgres
    {   
        /// <summary>
        /// Достает данные из Postgresql и представляет их в виде списка
        /// </summary>
        /// <param name="query"></param> тело sql-запроса
        /// <param name="server"></param>
        /// <returns></returns>
        public IList<IList<object>> MeetPSQL3(string query, string server)
        {
            IList<IList<object>> dataBaseInfos = new List<IList<object>>();
            String connectionString = Program.configFromFile[0];
            NpgsqlConnection npgSqlConnection = new NpgsqlConnection(connectionString);
            npgSqlConnection.Open();
            NpgsqlCommand npgSqlCommand = new NpgsqlCommand(query, npgSqlConnection);
            NpgsqlDataReader npgSqlDataReader = npgSqlCommand.ExecuteReader();
            foreach (DbDataRecord dbDataRecord in npgSqlDataReader)
            {
                var dbi = new List<object>();
                dbi.Add(server);
                dbi.Add(dbDataRecord.GetString(0));
                dbi.Add(dbDataRecord.GetString(1));  
                
                dataBaseInfos.Add(dbi);                
            }
            npgSqlConnection.Close();
            return dataBaseInfos;
        }


    }
}
