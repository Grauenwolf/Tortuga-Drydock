using System.Data;
using Tortuga.Anchor.Modeling;
using Tortuga.Chain.Metadata;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class SqlServerColumnModel : ColumnModel<SqlDbType>
    {
        public SqlServerColumnModel(ColumnMetadata<SqlDbType> column) : base(column)
        {

        }

        public override bool ContainsText
        {
            get
            {
                switch (NormalizedTypeName)
                {
                    case "varchar": return true;
                    case "char": return true;
                    case "nvarchar": return true;
                    case "nchar": return true;
                    case "text": return true;
                    case "ntext": return true;
                }
                return base.ContainsText;
            }
        }


        public bool IsSparse { get => Get<bool>(); set => Set(value); }

        public override string ObsoleteMessage
        {
            get
            {
                switch (NormalizedTypeName)
                {
                    case "datetime": return "Use dateTime2";
                    case "smalldatetime": return "Use dateTime2";
                    case "text": return "Use varChar(max)";
                    case "ntext": return "Use nVarChar(max)";
                    case "image": return "Use varBinary(max)";
                }
                return null;
            }
        }

        [CalculatedField("NullRate")]
        public bool? ShouldBeSparse
        {
            get
            {
                if (NullRate == null)
                    return null;
                if (!Column.IsNullable)
                    return null;

                //reference: https://technet.microsoft.com/en-us/library/cc280604(v=sql.120).aspx

                switch (NormalizedTypeName)
                {
                    case "bit": return NullRate >= .98;
                    case "tinyint": return NullRate >= .86;
                    case "smallint": return NullRate >= .76;
                    case "int": return NullRate >= .64;
                    case "bigint": return NullRate >= .52;
                    case "real": return NullRate >= .64;
                    case "float": return NullRate >= .52;
                    case "smallmoney": return NullRate >= .64;
                    case "money": return NullRate >= .52;
                    case "smalldatetime": return NullRate >= .64;
                    case "datetime": return NullRate >= .52;
                    case "uniqueidentifier": return NullRate >= .43;
                    case "date": return NullRate >= .69;
                    case "datetime2":
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .57;
                            else
                                return NullRate >= .52;
                        }
                    case "time":
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .69;
                            else
                                return NullRate >= .60;
                        }
                    case "datetimetoffset":
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .52;
                            else
                                return NullRate >= .49;
                        }
                    case "decimal":
                    case "numeric":
                    case "vardecimal":
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .60;
                            else
                                return NullRate >= .42;
                        }
                    case "char":
                    case "varchar":
                    case "nchar":
                    case "nvarchar":
                    case "xml":
                    case "hierarchyid":
                        return NullRate >= .60;

                }

                return null;


            }
        }

        [CalculatedField("NullRate,IsSparse")]
        public bool SparseCandidate { get => !IsSparse && (ShouldBeSparse == true); }

        [CalculatedField("NullRate,IsSparse")]
        public bool SparseWarning { get => IsSparse && (ShouldBeSparse == false); }

        public override bool SupportsDistinct
        {
            get
            {
                switch (NormalizedTypeName)
                {
                    case "xml":
                    case "text":
                    case "ntext":
                    case "geometry":
                    case "geography":
                        return false;
                }
                return true; //All SQL Server types, other than listed above, support distinct.
                //Is that true for user defined types? Probably not. Rewrite this to assume that they are not distinct compatible.
            }
        }


        //Probably don't need these checks.
        //public override bool UseMaxLength
        //{
        //    get
        //    {
        //        switch (NormalizedTypeName)
        //        {
        //            case "varchar": return true;
        //            case "char": return true;
        //            case "nvarchar": return true;
        //            case "nchar": return true;
        //            case "text": return true;
        //            case "ntext": return true;
        //            case "binary": return true;
        //        }
        //        return base.UseMaxLength;
        //    }
        //}

        //
        //public override bool UsePrecision
        //{
        //    get
        //    {
        //        switch (NormalizedTypeName)
        //        {
        //            case "decimal": return true;
        //            case "numeric": return true;
        //        }
        //        return base.UsePrecision;
        //    }
        //}

        //
        //public override bool UseScale
        //{
        //    get
        //    {
        //        switch (NormalizedTypeName)
        //        {
        //            case "decimal": return true;
        //            case "numeric": return true;
        //            case "datetime2": return true;
        //            case "datetimeoffset": return true;
        //        }
        //        return base.UseScale;
        //    }
        //}





    }
}


