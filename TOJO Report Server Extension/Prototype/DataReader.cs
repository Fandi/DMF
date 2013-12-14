using Microsoft.ReportingServices.DataProcessing;
using System;
using sd = System.Data;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class DataReader : IDataReader
	{
		protected int RowPosition { get; set; }
		protected sd.DataTable DataTable { get; set; }

		public DataReader(sd.DataTable dataTable)
		{
			RowPosition = -1;
			DataTable = dataTable;
		}

		public int FieldCount
		{
			get
			{
				return DataTable.Columns.Count;
			}
		}

		public Type GetFieldType(int fieldIndex)
		{
			return DataTable.Columns[fieldIndex].DataType;
		}

		public string GetName(int fieldIndex)
		{
			return DataTable.Columns[fieldIndex].ColumnName;
		}

		public int GetOrdinal(string fieldName)
		{
			return DataTable.Columns.IndexOf(fieldName);
		}

		public object GetValue(int fieldIndex)
		{
			return DataTable.Rows[RowPosition][fieldIndex];
		}

		public bool Read()
		{
			return ++RowPosition < DataTable.Rows.Count;
		}

		public void Dispose()
		{
			DataTable.Dispose();
		}
	}
}
