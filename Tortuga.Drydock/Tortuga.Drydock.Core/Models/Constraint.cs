using Tortuga.Anchor.Modeling;

namespace Tortuga.Drydock.Models
{
    public class Constraint : ModelBase
    {
        public string ReferencedSchemaName { get => Get<string>(); set => Set(value); }
        public string ReferencedTableName { get => Get<string>(); set => Set(value); }
        public string ReferencedColumnName { get => Get<string>(); set => Set(value); }

        public string ConstraintName { get => Get<string>(); set => Set(value); }
        public string Definition { get => Get<string>(); set => Set(value); }
        public ConstraintType ConstraintType { get => Get<ConstraintType>(); set => Set(value); }

        public string AnnotatedDefinition
        {
            get
            {
                switch (ConstraintType)
                {
                    case ConstraintType.Default: return "DEFAULT " + Definition;
                    case ConstraintType.Check: return "CHECK " + Definition;
                    case ConstraintType.ForiegnKey: return "REFERENCES " + Definition;
                }
                return ConstraintType + " " + Definition;
            }
        }
    }
}


