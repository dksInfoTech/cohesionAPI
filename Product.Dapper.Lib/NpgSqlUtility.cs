using System.Text;
using Product.Dapper.Lib.Enums;

namespace Product.Dapper.Lib
{
    public class NpgSqlUtility
    {
        /// <summary>
        /// Build NpgSql Query for Dapper ORM
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <param name="outerConditionType"></param>
        /// <param name="innerConditionType"></param>
        /// <returns></returns>
        public static string BuildNpgSqlQuery(string query, IList<Dictionary<string, object>> args
                                                    , NpgSqlConditionType? outerConditionType = null
                                                    , NpgSqlConditionType innerConditionType = NpgSqlConditionType.AND)
        {
            var builder = new StringBuilder(query);
            if (args.Any())
                builder.Append(' ').Append("WHERE").Append(' ');
            for (int i = 0; i < args.Count(); i++)
            {
                if (i > 0)
                    builder.Append(outerConditionType.ToString()).Append(' ');

                builder.Append("(");
                BuildNpgSqlQueryConditions(builder, args[i], innerConditionType);
                builder.Append(")").Append(' ');
            }
            return builder.ToString();
        }

        /// <summary>
        /// Build NpgSql Query Conditions
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="args"></param>
        /// <param name="innerConditionType"></param>
        private static void BuildNpgSqlQueryConditions(StringBuilder builder, Dictionary<string, object> args
                                            , NpgSqlConditionType innerConditionType)
        {
            for (int i = 0; i < args.Keys.Count; i++)
            {
                var key = args.Keys.ToList()[i];
                builder.Append($"{key} = '{args[key]}'");
                if (i < args.Keys.Count - 1)
                    builder.Append(' ').Append(innerConditionType.ToString()).Append(' ');
            }
        }
    }
}
