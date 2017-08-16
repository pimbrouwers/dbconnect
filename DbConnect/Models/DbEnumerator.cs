using System;
using System.Collections;
using System.Collections.Generic;

namespace Cinch.DbConnect
{
    public interface IDbEnumerator<T> : IEnumerable<T>, IDisposable
    {

    }
    
    public class DbEnumerator<T> : IDbEnumerator<T>
    {
        public IDbReader dbReader;
        public IEnumerable<T> lst;

        public DbEnumerator(IDbReader dbReader)
        {
            this.dbReader = dbReader;
            lst = dbReader.Enumerate<T>();
        }

        public void Dispose()
        {
            dbReader.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var i in lst)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
}
