using System;

namespace Munk.AspNetCore.Sql
{
    public abstract class SubOrderWhereClauseBuilder
    {
        protected readonly string TableName;
        protected readonly bool IncludeEndDate;
        public readonly bool? IncludeEqualized;
        public readonly string Op = "=";

        protected SubOrderWhereClauseBuilder(string tableName)
        {
            TableName = tableName;
        }

        protected SubOrderWhereClauseBuilder(string tableName, bool includeEndDate = false, bool? includeEqualized = false, string op = "=") : this(tableName)
        {
            IncludeEqualized = includeEqualized;
            IncludeEndDate = includeEndDate;
            Op = op;
        }

        public abstract string WhereClause(string overrideTableName = null, string prefix = "AND");

        protected string GetIncludeSqlEqualizedSnippet()
        {
            string includeSqlEqualizedSnippet = IncludeEqualized != null ? (!IncludeEqualized.Value ? $" AND {TableName}.IsEqualized=0" : "") : "";
            return includeSqlEqualizedSnippet;
        }

        protected string GetIncludeSqlEndDateSnippet()
        {
            string includeSqlEndDateSnippet = IncludeEndDate ? ">=" : ">";
            return includeSqlEndDateSnippet;
        }

    }

    public class SubOrderIdEqualsWhereClauseBuilder : SubOrderWhereClauseBuilder
    {
        private readonly Guid _subOrderId;

        public SubOrderIdEqualsWhereClauseBuilder(string tableName, Guid subOrderId, bool? includeEqualized = false) : base(tableName, false, includeEqualized)
        {
            _subOrderId = subOrderId;
        }

        public override string WhereClause(string overrideTableName = null, string prefix = "AND")
        {
            var tableName = overrideTableName ?? TableName;
            string sqlWhere =
                $@" {prefix} {tableName}.Id='{_subOrderId}' {GetIncludeSqlEqualizedSnippet()}";

            return sqlWhere;
        }
    }

    public class IntervalWithinWhereClauseBuilder : SubOrderWhereClauseBuilder
    {
        public IntervalWithinWhereClauseBuilder(string tableName, bool includeEndDate = false, bool includeEqualized = false) : base(tableName, includeEndDate, includeEqualized)
        {
        }

        public DateTime Start { get; set; } = DateTime.MinValue;
        public DateTime End { get; set; } = DateTime.MaxValue;

        public override string WhereClause(string overrideTableName = null, string prefix = "AND")
        {
            string sqlWhere = $@" {prefix} (({TableName}.[Start]>='{Start.ToSql()}' AND {TableName}.[Start]<'{End.ToSql()}') OR 
                ({TableName}.[End]{GetIncludeSqlEndDateSnippet()}'{Start.ToSql()}' AND {TableName}.[End]<='{End.ToSql()}') OR 
                ({TableName}.[Start]<='{Start.ToSql()}' AND {TableName}.[End]>='{End.ToSql()}')) 
                {GetIncludeSqlEqualizedSnippet()}";

            return sqlWhere;
        }
    }

    public class DateWithinWhereClauseBuilder : SubOrderWhereClauseBuilder
    {
        public string ColumnName { get; set; }
        public DateTime Start { get; set; } = DateTime.MinValue;
        public DateTime End { get; set; } = DateTime.MaxValue;

        public DateWithinWhereClauseBuilder(string tableName, string columnName, bool includeEndDate = false, bool? includeEqualized = false) : base(tableName, includeEndDate, includeEqualized)
        {
            ColumnName = columnName;
        }

        public override string WhereClause(string overrideTableName = null, string prefix = "AND")
        {
            var tableName = overrideTableName ?? TableName;
            string sqlWhere =
                $@" {prefix} {tableName}.[{ColumnName}]>='{Start.ToSql()}' AND '{End.ToSql()}'{GetIncludeSqlEndDateSnippet()}{tableName}.[{ColumnName}]";
                
            return sqlWhere;
        }
    }

    public class StartDateWithinWhereClauseBuilder : DateWithinWhereClauseBuilder
    {
        public StartDateWithinWhereClauseBuilder(string tableName, bool includeEndDate = false, bool? includeEqualized = false) : base(tableName, "Start" ,includeEndDate, includeEqualized)
        {
        }
    }

    public class StartDateStrictWhereClauseBuilder : SubOrderWhereClauseBuilder
    {
        public DateTime Start { get; set; } = DateTime.MinValue;

        public StartDateStrictWhereClauseBuilder(string tableName, bool? includeEqualized = false, string op = "=") : base(tableName, false, includeEqualized, op)
        {
        }

        public override string WhereClause(string overrideTableName = null, string prefix = "AND")
        {
            string sqlWhere = $@" {prefix} {TableName}.[Start]{Op}'{Start.ToSql()}' {GetIncludeSqlEqualizedSnippet()}";
            return sqlWhere;
        }
    }

    public class EndDateStrictWhereClauseBuilder : SubOrderWhereClauseBuilder
    {
        public DateTime End { get; set; } = DateTime.MinValue;

        public EndDateStrictWhereClauseBuilder(string tableName, bool? includeEqualized = false, string op = "=") : base(tableName, false, includeEqualized, op)
        {
        }

        public override string WhereClause(string overrideTableName = null, string prefix = "AND")
        {
            string sqlWhere = $@" {prefix} {TableName}.[End]{Op}'{End.ToSql()}' {GetIncludeSqlEqualizedSnippet()}";
            return sqlWhere;
        }
    }

    public class CombineSubOrderWhereClauseBuilders : SubOrderWhereClauseBuilder
    {
        private readonly SubOrderWhereClauseBuilder _subOrderWhereClauseBuilder1;
        private readonly SubOrderWhereClauseBuilder _subOrderWhereClauseBuilder2;
        private readonly string _comparer;

        public CombineSubOrderWhereClauseBuilders(SubOrderWhereClauseBuilder subOrderWhereClauseBuilder1, SubOrderWhereClauseBuilder subOrderWhereClauseBuilder2, string comparer = "OR") : base(null)
        {
            _subOrderWhereClauseBuilder1 = subOrderWhereClauseBuilder1;
            _subOrderWhereClauseBuilder2 = subOrderWhereClauseBuilder2;
            _comparer = comparer;
        }

        public CombineSubOrderWhereClauseBuilders(string tableName) : base(tableName)
        {
        }

        public CombineSubOrderWhereClauseBuilders(string tableName, bool includeEndDate = false, bool? includeEqualized = false, string op = "=") : base(tableName, includeEndDate, includeEqualized, op)
        {
        }

        public override string WhereClause(string overrideTableName = null, string prefix = "AND")
        {
            var whereClause = "(" + _subOrderWhereClauseBuilder1.WhereClause(null, "") + ") " + _comparer + " (" + _subOrderWhereClauseBuilder2.WhereClause(null, "") + ")";
            return whereClause;
        }
    }
}
