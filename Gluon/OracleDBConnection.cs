﻿using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Oracle.DataAccess.Client;

namespace Gluon
{
    public class OracleDbConnection
    {
        /// <summary>
        ///     Connection string to data base
        /// </summary>
        private static string _connectionString;

        public OracleDbConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["OraConnection"].ConnectionString;
        }

        /// <summary>
        ///     Getting packages and their stored procedures with arguments
        /// </summary>
        /// <returns> Dictionary [package [storedproc [args]]]</returns>
        public Dictionary<string, Dictionary<string, List<string>>> GetData()
        {
            var packages = new Dictionary<string, Dictionary<string, List<string>>>();
            using (var dbСonnection = new OracleConnection(_connectionString))
            {
                dbСonnection.Open();

                try
                {
                    var sql = Utils.GetFileBody("requestOraPackagesData.sql");
                    using (var command = new OracleCommand(sql, dbСonnection))
                    {
                        command.CommandType = CommandType.Text;
                        using (var dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var package = dr.GetString(0).ToLower();
                                var procedure = dr.GetString(1).ToLower();
                                var argument = dr.IsDBNull(2) ? ClassesStructureCreator.ReturnVal : dr.GetString(2).ToLower();

                                if (!packages.ContainsKey(package))
                                {
                                    packages.Add(package, new Dictionary<string, List<string>>());
                                }

                                var procedures = packages[package];
                                if (!procedures.ContainsKey(procedure))
                                {
                                    procedures.Add(procedure, new List<string>());
                                }

                                // The argument name is always unique for this stored procedure so just add it to the list
                                var arguments = procedures[procedure];
                                arguments.Add(argument);
                            }
                        }
                    }
                }
                finally
                {
                    dbСonnection.Close();
                }
            }
            return packages;
        }
    }
}