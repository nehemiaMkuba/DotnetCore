namespace Core.Management.Infrastructure
{
    public partial class Queries
    {
        public const string GET_ENTITY_BY_COLUMN_NAME = "SELECT * FROM gospel.{EntityName} WHERE {ColumnName} = @value";
        public const string GET_ENTITY_COLUMN = "SELECT {Column} FROM gospel.{EntityName} WHERE {ColumnName} = @value";
 
    }
}
