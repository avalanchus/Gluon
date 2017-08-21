using System.Configuration;
using System.Data;
using GluonTest.OracleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Client;

namespace GluonTest
{
    [TestClass]
    public class GluonUnitTest
    {
        [TestMethod]
        public void TestOracleHelpersNames()
        {
            var sal = HR_PACK.Get_Sal;
            var @return = HR_PACK.GET_SAL.Return;
            Assert.IsTrue(sal == "HR_PACK.GET_SAL");
            Assert.IsTrue(@return == "RETURN");
        }

        [TestMethod]
        public void TestOracleHelpers()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["OraConnection"].ConnectionString;
            using (var dbСonnection = new OracleConnection(connectionString))
            {
                dbСonnection.Open();
                try
                {
                    var command = new OracleCommand(HR_PACK.Get_Sal, dbСonnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(HR_PACK.GET_SAL.Return, OracleDbType.Int32, ParameterDirection.ReturnValue);
                    command.Parameters.Add(HR_PACK.GET_SAL.P_Id, OracleDbType.Int32, 100, ParameterDirection.Input);
                    command.Parameters.Add(HR_PACK.GET_SAL.P_Increment, OracleDbType.Int32, 1, ParameterDirection.Input);
                    command.ExecuteNonQuery();
                    var result = command.Parameters[HR_PACK.GET_SAL.Return].Value.ToString();
                    Assert.IsTrue(result == "24000");
                }
                finally
                {
                    dbСonnection.Close();
                }
            }
        }
    }
}
