using Dapper;
using System;
using System.Collections.Generic;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IStoredProcedureCall : IDisposable
    {
        T Single<T>(string procedureName, DynamicParameters parameters = null); //Execute scalar which will return integer value or string value
       void Execute(string procedureName, DynamicParameters parameters = null);

        T OneRecord<T>(string procedureName, DynamicParameters parameters = null); //Return Entity

        IEnumerable<T> List<T>(string procedureName, DynamicParameters parameters = null); //Return List of Entity

        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters parameters = null); //Return List of Entity
    }
}
