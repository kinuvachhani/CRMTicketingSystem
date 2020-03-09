using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMTicketingSystem.DataAccess.Repository
{
    public class SP_call : ISP_call
    {
        private readonly ApplicationDbContext _db;
        private static string ConnectioString = "";

        public SP_call(ApplicationDbContext db)
        {
            _db = db;
            ConnectioString = db.Database.GetDbConnection().ConnectionString;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Execute(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection Con = new SqlConnection(ConnectioString))
            {
                Con.Open();
                Con.Execute(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }

        }

        public IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection Con = new SqlConnection(ConnectioString))
            {
                Con.Open();
                return Con.Query<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public T OneRecord<T>(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection Con = new SqlConnection(ConnectioString))
            {
                Con.Open();
                var value= Con.Query<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
                return (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T));
            }
        }

        public T Single<T>(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection Con = new SqlConnection(ConnectioString))
            {
                Con.Open();
                return (T)Convert.ChangeType(Con.ExecuteScalar<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure),typeof(T));
            }
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection Con = new SqlConnection(ConnectioString))
            {
                Con.Open();
                var Result = SqlMapper.QueryMultiple(Con, procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
                var item1 = Result.Read<T1>().ToList();
                var item2 = Result.Read<T2>().ToList();

                if (item1 != null && item2 != null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1, item2);
                }
            }
            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(),new List<T2>());

        }
    }
}
