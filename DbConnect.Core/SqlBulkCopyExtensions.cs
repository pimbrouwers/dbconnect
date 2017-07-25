using FastMember;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Cinch
{
    public static class SqlBulkCopyExtensions
    {
        public static void MapColumns<T>(this SqlBulkCopy bcp, IEnumerable<string> ignoreCols)
        {
            Type type = typeof(T);
            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();

            foreach (var member in members)
            {
                if (ignoreCols != null && ignoreCols.Contains(member.Name))
                    continue;

                bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping(member.Name, member.Name));
            }
        }
    }
}
