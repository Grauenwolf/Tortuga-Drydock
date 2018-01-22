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
                switch (Column.DbType)
                {
                    case SqlDbType.VarChar:
                    case SqlDbType.Char:
                    case SqlDbType.NVarChar:
                    case SqlDbType.NChar:
                    case SqlDbType.Text:
                    case SqlDbType.NText:
                        return true;
                    default:
                        return base.ContainsText;
                }
            }
        }


        public bool IsSparse { get => Get<bool>(); set => Set(value); }

        public override string ObsoleteMessage
        {
            get
            {
                switch (Column.DbType)
                {
                    case SqlDbType.DateTime: return "Use dateTime2";
                    case SqlDbType.SmallDateTime: return "Use dateTime2";
                    case SqlDbType.Text: return "Use varChar(max)";
                    case SqlDbType.NText: return "Use nVarChar(max)";
                    case SqlDbType.Image: return "Use varBinary(max)";
                    case SqlDbType.VarChar when Column.MaxLength == 1 || Column.MaxLength == 2:
                        return "Use Char instead of VarChar if the max length is 1 or 2";
                    case SqlDbType.NVarChar when Column.MaxLength == 1 || Column.MaxLength == 2:
                        return "Use NChar instead of NVarChar if the max length is 1 or 2";
                }
                return null;
            }
        }

        public override string ObsoleteReplaceType
        {
            get
            {
                switch (Column.DbType)
                {
                    case SqlDbType.DateTime: return "dateTime2(3)";
                    case SqlDbType.SmallDateTime: return "dateTime2(0)";
                    case SqlDbType.Text: return "varChar(max)";
                    case SqlDbType.NText: return "nVarChar(max)";
                    case SqlDbType.Image: return "varBinary(max)";
                    case SqlDbType.VarChar when Column.MaxLength == 1:
                        return "char(1)";
                    case SqlDbType.VarChar when Column.MaxLength == 2:
                        return "char(2)";
                    case SqlDbType.NVarChar when Column.MaxLength == 1:
                        return "NChar(1)";
                    case SqlDbType.NVarChar when Column.MaxLength == 2:
                        return "NChar(2)";
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

                switch (Column.DbType)
                {
                    case SqlDbType.Bit: return NullRate >= .98;
                    case SqlDbType.TinyInt: return NullRate >= .86;
                    case SqlDbType.SmallInt: return NullRate >= .76;
                    case SqlDbType.Int: return NullRate >= .64;
                    case SqlDbType.BigInt: return NullRate >= .52;
                    case SqlDbType.Real: return NullRate >= .64;
                    case SqlDbType.Float: return NullRate >= .52;
                    case SqlDbType.SmallMoney: return NullRate >= .64;
                    case SqlDbType.Money: return NullRate >= .52;
                    case SqlDbType.SmallDateTime: return NullRate >= .64;
                    case SqlDbType.DateTime: return NullRate >= .52;
                    case SqlDbType.UniqueIdentifier: return NullRate >= .43;
                    case SqlDbType.Date: return NullRate >= .69;
                    case SqlDbType.DateTime2:
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .57;
                            else
                                return NullRate >= .52;
                        }
                    case SqlDbType.Time:
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .69;
                            else
                                return NullRate >= .60;
                        }
                    case SqlDbType.DateTimeOffset:
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .52;
                            else
                                return NullRate >= .49;
                        }
                    case SqlDbType.Decimal:
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .60;
                            else
                                return NullRate >= .42;
                        }
                    case SqlDbType.Char:
                    case SqlDbType.VarChar:
                    case SqlDbType.NChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Xml:
                        return NullRate >= .60;

                }

                //Special cases
                switch (NormalizedTypeName)
                {
                    case "numeric":
                    case "vardecimal":
                        {
                            if (Column.Precision == 0)
                                return NullRate >= .60;
                            else
                                return NullRate >= .42;
                        }
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
                switch (Column.DbType)
                {
                    case SqlDbType.BigInt:
                    case SqlDbType.Bit:
                    case SqlDbType.Char:
                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Int:
                    case SqlDbType.Money:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Real:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.SmallInt:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Time:
                    case SqlDbType.TinyInt:
                    case SqlDbType.UniqueIdentifier:
                        return true;
                }
                return false;
            }
        }
    }
}


