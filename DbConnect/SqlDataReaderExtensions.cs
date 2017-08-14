using FastMember;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cinch.DbConnect
{
    
    public static class SqlDataReaderExtensions
    {
        public static T ConvertTo<T>(this SqlDataReader rd)
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
        
        public static Dictionary<string, object> ConvertToDictionary(this SqlDataReader rd)
        {
            var dict = new Dictionary<string, object>();

            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (!rd.IsDBNull(i))
                {
                    dict.Add(rd.GetName(i), rd.GetValue(i));
                }
            }

            return dict;
        }

        public static IEnumerable<T> Enumerate<T>(this SqlDataReader rd)
        {
            while (rd.Read())
                yield return rd.ConvertTo<T>();

            rd.NextResult();
        }

        public static async Task<IEnumerable<T>> EnumerateAsync<T>(this SqlDataReader rd)
        {
            var lst = new List<T>();

            while (await rd.ReadAsync())
                lst.Add(rd.ConvertTo<T>());

            rd.NextResult();

            return lst;
        }
    }
}
