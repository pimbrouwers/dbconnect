using System.Collections.Generic;
using System.Reflection;

namespace Cinch
{
    internal static class DbConnectCache
    {
        internal static Dictionary<string, PropertyInfo[]> _objectPropertyCache = new Dictionary<string, PropertyInfo[]>();
        internal static Dictionary<string, ConstructorInfo> _objectConstructorCache = new Dictionary<string, ConstructorInfo>();
    }
}
