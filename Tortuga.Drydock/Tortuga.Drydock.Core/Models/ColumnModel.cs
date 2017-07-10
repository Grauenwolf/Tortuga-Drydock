using Tortuga.Anchor.Modeling;
using Tortuga.Chain.Metadata;

namespace Tortuga.Drydock.Models
{
    public class ColumnModel<TDbType> : ModelBase
        where TDbType : struct
    {

        public ColumnModel(ColumnMetadata<TDbType> column)
        {
            Column = column;
            NormalizedTypeName = column.TypeName.ToLowerInvariant();
        }

        public string Name { get => Column.SqlName; }
        public string TypeName { get => Column.TypeName; }

        public int? MaxLength { get => Column.MaxLength; }
        public int? Precision { get => Column.Precision; }
        public int? Scale { get => Column.Scale; }
        public bool IsIdentity { get => Column.IsIdentity; }
        public bool IsComputed { get => Column.IsComputed; }
        public bool IsNullable { get => Column.IsNullable; }
        public bool IsPrimaryKey { get => Column.IsPrimaryKey; }


        public int? ActualMaxLength { get => Get<int?>(); set => Set(value); }
        public decimal? AverageLength { get { return Get<decimal?>(); } set { Set(value); } }
        public ColumnMetadata<TDbType> Column { get; }


        /// <summary>
        /// Indicates that this is a text column.
        /// </summary>
        /// <value><c>true</c> if contains text; otherwise, <c>false</c>.</value>
        /// <remarks>This version uses ANSI SQL types.</remarks>
        public virtual bool ContainsText
        {
            get
            {
                switch (NormalizedTypeName)
                {
                    case "character":
                    case "char":
                    case "character varying":
                    case "char varying":
                    case "varchar":
                    case "character large object":
                    case "char large object":
                    case "clob":
                    case "national character":
                    case "national char":
                    case "nchar":
                    case "national character varying":
                    case "national char varying":
                    case "nchar varying":
                    case "national character large object":
                    case "nchar large object":
                    case "nclob":
                        return true;
                }
                return false;
            }
        }

        public int? DistinctCount { get => Get<int?>(); set => Set(value); }
        [CalculatedField("DistinctCount,SampleSize")]
        public double? DistinctRate
        {
            get { return SampleSize > 0 && DistinctCount.HasValue ? (double?)DistinctCount / (double)SampleSize.Value : null; }
        }

        public int? EmptyCount { get => Get<int?>(); set => Set(value); }
        [CalculatedField("EmptyCount,SampleSize")]
        public double? EmptyRate
        {
            get { return SampleSize > 0 && EmptyCount.HasValue ? (double?)EmptyCount / (double)SampleSize.Value : null; }
        }

        public bool? IsIndexed { get => Get<bool?>(); set => Set(value); }
        [CalculatedField("ObsoleteMessage")]
        public bool IsObsolete { get => ObsoleteMessage != null; }

        public bool IsPrimaryKeyCandidate { get => Get<bool>(); set => Set(value); }
        public bool? IsUnique { get => Get<bool?>(); set => Set(value); }
        public bool? IsUniqueIndex { get => Get<bool?>(); set => Set(value); }
        [CalculatedField("DistinctCount,SampleSize")]
        public bool? NoDistinctValues
        {
            get
            {
                if (DistinctCount == null || SampleSize == null) return null;
                if (DistinctCount == 1 && SampleSize > 1) return true;
                return false;
            }
        }

        public int? NullCount { get => Get<int?>(); set => Set(value); }
        [CalculatedField("NullCount,SampleSize")]
        public double? NullRate
        {
            get { return SampleSize > 0 && NullCount.HasValue ? (double?)NullCount / (double)SampleSize.Value : null; }
        }


        public virtual string ObsoleteMessage { get => null; }

        public int? SampleSize { get => Get<int?>(); set => Set(value); }
        public bool StatsLoaded { get => Get<bool>(); set => Set(value); }

        /// <summary>
        /// Gets a value indicating whether this column supports the distinct operator.
        /// </summary>
        /// <value><c>true</c> if [supports distinct]; otherwise, <c>false</c>.</value>
        /// <remarks>This version uses ANSI SQL types.</remarks>
        public virtual bool SupportsDistinct
        {
            get
            {
                switch (NormalizedTypeName)
                {
                    case "character":
                    case "char":
                    case "character varying":
                    case "char varying":
                    case "varchar":
                    case "national character":
                    case "national char":
                    case "nchar":
                    case "national character varying":
                    case "national char varying":
                    case "nchar varying":
                    case "numeric":
                    case "decimal":
                    case "dec":
                    case "smallint":
                    case "integer":
                    case "int":
                    case "bigint":
                    case "float":
                    case "real":
                    case "double precision":
                    case "boolean":
                    case "date":
                    case "time":
                    case "timestamp":
                    case "date with time zone":
                    case "date without time zone":
                    case "time with time zone":
                    case "time without time zone":
                    case "timestamp with time zone":
                    case "timestamp without time zone":
                        return true;
                }
                return false;
            }
        }


        public virtual bool UseMaxLength { get => Column.MaxLength.HasValue; }


        public virtual bool UsePrecision { get => Column.Precision.HasValue; }


        public virtual bool UseScale { get => Column.Scale.HasValue; }

        internal TableVM TableVM
        {
            get { return Get<TableVM>(); }
            set { Set(value); }
        }

        protected string NormalizedTypeName { get; }
    }
}


