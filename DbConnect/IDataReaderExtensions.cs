using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    
    public static class IDataReaderExtensions
    {
        public static T ConvertTo<T>(this IDataReader rd)
        {
            Type type = typeof(T);
            bool isPrim = type.GetTypeInfo().IsValueType;
            var t = Activator.CreateInstance<T>();

            if (isPrim)
            {
                t = (T)rd.GetValue(0);
            }
            else
            {
                var accessor = TypeAccessor.Create(type);
                var members = accessor.GetMembers();
                
                for (int i = 0; i < rd.FieldCount; i++)
                {
                    if (!rd.IsDBNull(i))
                    {
                        string fieldName = rd.GetName(i);

                        if (members.Any(m => string.Equals(m.Name, fieldName, StringComparison.OrdinalIgnoreCase)))
                            accessor[t, fieldName] = rd.GetValue(i);
                    }
                }
            }            

            return t;
        }
        
        public static IEnumerable<T> Enumerate<T>(this IDataReader rd)
        {
            while (rd.Read())
                yield return rd.ConvertTo<T>();

            rd.NextResult();
        }
    }
}
