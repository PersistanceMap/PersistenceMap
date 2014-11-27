﻿using System.Linq;
using System.Text;
using PersistanceMap.QueryBuilder;
using System.Collections.Generic;
using PersistanceMap.Sql;

namespace PersistanceMap.QueryParts
{
    public class ProcedureQueryPartsMap : QueryPartsMap, IQueryPartsMap
    {
        public ProcedureQueryPartsMap(string procedure)
        {
            procedure.EnsureArgumentNotNullOrEmpty("procedure");

            ProcedureName = procedure;
        }

        #region IQueryPartsMap Implementation

        public override CompiledQuery Compile()
        {
            /* *Using Output compiles to*                
            declare @p1 datetime
            set @p1='2012-01-01 00:00:00'
            exec procedure @param1=@p1 output,@param2='2014-07-15 00:00:00'
            select @p1                
            */

            var sb = new StringBuilder(100);

            // prepare outputparameters
            foreach (var param in Parts.Where(p => p.OperationType == OperationType.OutParameterPrefix))
            {
                // creates a name for the output parameter
                var value = param.Compile();
                if (!string.IsNullOrEmpty(value))
                    sb.AppendLine(value);
            }

            // create the exec statement
            sb.Append(string.Format("EXEC {0} ", ProcedureName));

            var conv = new LambdaCompiler();
            conv.PrefixFieldWithTableName = false;

            // add parameters
            foreach (var param in Parameters)
            {
                var value = param.Compile();
                sb.Append(string.Format("{0}{1}", value, Parameters.Last() == param ? "" : ", "));
            }

            // add the select for all output parameters
            var selectoutput = string.Empty;
            foreach (var param in Parts.Where(p => p.OperationType == OperationType.OutParameterSufix))
            {
                bool separator = true;
                if (string.IsNullOrEmpty(selectoutput))
                {
                    selectoutput = "SELECT";
                    separator = false;
                }

                var value = param.Compile();
                if (!string.IsNullOrEmpty(value))
                    selectoutput = string.Format("{0} {1}{2}", selectoutput, separator ? ", " : "", value);
            }

            sb.AppendLine();
            sb.AppendLine(selectoutput);

            return new CompiledQuery
            {
                QueryString = sb.ToString(),
                QueryParts = this
            };
        }

        #endregion

        #region Properties

        internal IEnumerable<IParameterQueryPart> Parameters
        {
            get
            {
                return Parts.Where(p => p is IParameterQueryPart).Cast<IParameterQueryPart>();
            }
        }

        public string ProcedureName { get; private set; }

        #endregion
    }
}
