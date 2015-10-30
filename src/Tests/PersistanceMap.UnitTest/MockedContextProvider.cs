﻿using System;
using System.Collections.Generic;

namespace PersistanceMap.UnitTest
{
    public class MockedContextProvider : IDatabaseContext
    {
        public IConnectionProvider ConnectionProvider
        {
            get { throw new NotImplementedException(); }
        }

        public void Commit()
        {
        }

        public void AddQuery(IQueryCommand command)
        {
        }

        public IEnumerable<IQueryCommand> QueryStore
        {
            get { throw new NotImplementedException(); }
        }

        public InterceptorCollection Interceptors
        {
            get { throw new NotImplementedException(); }
        }

        public QueryKernel Kernel
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }
    }
}
