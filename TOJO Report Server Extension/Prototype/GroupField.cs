namespace TOJO.ReportServerExtension.Prototype
{
	internal class GroupField
	{
		public string Reference { get; protected set; }
		public string ColumnName { get; protected set; }

		public GroupField(string reference, string columnName)
		{
			Reference = reference;
			ColumnName = columnName;
		}
	}
}
