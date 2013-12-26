namespace TOJO.ReportServerExtension.Prototype
{
	internal class Group
	{
		public int Index { get; protected set; }
		public string Name { get; protected set; }
		public string ColumnName { get; protected set; }
		public bool IsEnumeration { get; set; }

		public Group(string name, int index, string columnName, bool isEnumeration = false)
		{
			Name = name;
			Index = index;
			ColumnName = columnName;
			IsEnumeration = isEnumeration;
		}
	}
}
