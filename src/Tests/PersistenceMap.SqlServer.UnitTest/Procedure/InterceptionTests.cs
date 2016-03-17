﻿using Moq;
using NUnit.Framework;
using PersistenceMap.Interception;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersistenceMap.SqlServer.UnitTest.Procedure
{
    [TestFixture]
    public class InterceptionTests
    {
        [Test]
        public void PersistenceMap_SqlServer_Procedure_Interception_Mock_Test()
        {
            var lst = new List<SalesByYear>
            {
                new SalesByYear
                {
                    OrdersID = 1,
                    Subtotal = 50
                }
            };

            var dataReader = new MockedDataReader<SalesByYear>(lst);

            var connectionProvider = new Mock<IConnectionProvider>();
            connectionProvider.Setup(exp => exp.QueryCompiler).Returns(() => new QueryCompiler());
            connectionProvider.Setup(exp => exp.Execute(It.IsAny<string>())).Returns(() => new DataReaderContext(dataReader));

            var provider = new SqlContextProvider(connectionProvider.Object);
            //ovider.Interceptor<SalesByYear>().Returns(() => lst);

            using (var context = provider.Open())
            {
                // proc with resultset without parameter names
                var proc = context.Procedure("SalesByYear")
                    .AddParameter(() => new DateTime(1970, 1, 1))
                    .AddParameter(() => DateTime.Today)
                    .Execute<SalesByYear>();

                Assert.IsTrue(proc.Any());
            }
        }

        private class SalesByYear
        {
            public DateTime ShippedDate { get; set; }

            public int OrdersID { get; set; }

            public double Subtotal { get; set; }

            public double SpecialSubtotal { get; set; }

            public int Year { get; set; }
        }
    }
}
