using System.Collections.Generic;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class Group
	{
		public int Index { get; protected set; }
		public string ColumnName { get; protected set; }

		public Group(int index, string columnName)
		{
			Index = index;
			ColumnName = columnName;
		}
	}
}
