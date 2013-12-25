using Microsoft.ReportingServices.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using sd = System.Data;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class Command : IDbCommand
	{
		private Random rnd;
		private sd.DataTable dataTable;
		private List<Group> groups;
		private List<GroupField> groupFields;

		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public IDataParameterCollection Parameters { get; protected set; }
		public IDbTransaction Transaction { get; set; }
		protected DataSourceConfiguration Configuration { get; set; }

		public Command(DataSourceConfiguration configuration)
		{
			rnd = new Random(DateTime.Now.GetHashCode());
			dataTable = new sd.DataTable();
			groups = new List<Group>();
			groupFields = new List<GroupField>();

			Configuration = configuration;
			CommandText = string.Empty;
			CommandTimeout = 5;
			CommandType = CommandType.Text;
			Parameters = new Prototype.DataParameterCollection();
		}

		public IDataParameter CreateParameter()
		{
			return new Prototype.DataMultiValueParameter();
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			sd.DataTable dataTable = GenerateDataTable(behavior);
			return new Prototype.DataReader(dataTable) as IDataReader;
		}

		protected sd.DataTable GenerateDataTable(CommandBehavior behavior)
		{
			#region DMF Specification
			if (CommandType != CommandType.Text ||
				string.IsNullOrEmpty(CommandText.Trim()))
			{
				return dataTable;
			}

			XmlDocument specXML = null;

			using (StringReader reader = new StringReader(CommandText))
			{
				string line = reader.ReadLine();

				while (line != null)
				{
					Regex regex = new Regex(@"^\s*\-\-\s*Name\s*:", RegexOptions.CultureInvariant | RegexOptions.Singleline);

					if (regex.IsMatch(line))
					{
						string specification = line.Substring(line.IndexOf(':') + 1, line.Length - line.IndexOf(':') - 1).Trim();

						if (specification.Length > 1 &&
							specification[0] == '"' &&
							specification[specification.Length - 1] == '"')
						{
							specification = specification.Substring(1, specification.Length - 2);
							specification = Regex.Unescape(specification);
						}

						FileInfo specFileInfo = new FileInfo(
							Path.Combine(
								Configuration.SpecificationFolder.FullName,
								specification + ".dmf"
							)
						);

						if (specFileInfo != null)
						{
							specXML = new XmlDocument();
							specXML.Load(specFileInfo.FullName);
						}

						break;
					}

					line = reader.ReadLine();
				}
			}
			#endregion

			if (specXML == null ||
				specXML.DocumentElement == null)
			{
				return dataTable;
			}

			#region Min and Max Values
			int min = -1;
			int max = -1;

			if (specXML.DocumentElement.Attributes["Max"] == null ||
				!int.TryParse(specXML.DocumentElement.Attributes["Max"].Value, out max))
			{
				max = 50;
			}

			if (specXML.DocumentElement.Attributes["Min"] == null ||
				!int.TryParse(specXML.DocumentElement.Attributes["Min"].Value, out min) ||
				min > max)
			{
				min = max;
			}
			#endregion

			#region Data schema
			foreach (XmlNode field in specXML.SelectNodes("/Fields/Field"))
			{
				if (field.Attributes["Name"] == null)
				{
					continue;
				}

				string fieldName = field.Attributes["Name"].Value; ;
				string columnName;
				string dataType;

				#region Column name and data type
				XmlNode fieldNode = field.SelectSingleNode("./DataField");

				if (fieldNode == null)
				{
					fieldNode = field.SelectSingleNode("./Value");

					if (fieldNode == null)
					{
						continue;
					}

					columnName = fieldNode.InnerText;

					XmlAttribute dataTypeAttribute = fieldNode.Attributes["DataType"];

					if (dataTypeAttribute == null)
					{
						dataType = "System.String";
					}
					else
					{
						switch (dataTypeAttribute.Value)
						{
							case "Boolean":
								dataType = "System.Boolean";
								break;
							case "DateTime":
								dataType = "System.DateTime";
								break;
							case "Integer":
								dataType = "System.Int32";
								break;
							case "Float":
								dataType = "System.Decimal";
								break;
							case "String":
							default:
								dataType = "System.String";
								break;
						}
					}
				}
				else
				{
					columnName = fieldNode.InnerText;

					XmlNamespaceManager namespaceManager = new XmlNamespaceManager(specXML.NameTable);
					namespaceManager.AddNamespace("rd", DataSourceConfiguration.ReportDesignerSchemaURI);
					XmlNode dataTypeNode = field.SelectSingleNode("./rd:TypeName", namespaceManager);

					if (dataTypeNode == null)
					{
						dataType = "System.String";
					}
					else
					{
						dataType = dataTypeNode.InnerText;
					}
				}
				#endregion

				sd.DataColumn column = new sd.DataColumn(columnName);

				#region Group
				XmlNode group = field.SelectSingleNode("./Group");

				if (group != null)
				{
					if (group.Attributes["Index"] != null)
					{
						groups.Add(
							new Group(
								fieldName,
								int.Parse(group.Attributes["Index"].Value),
								columnName
							)
						);
					}
					else if (group.Attributes["Ref"] != null)
					{
						groupFields.Add(
							new GroupField(
								group.Attributes["Ref"].Value,
								columnName
							)
						);
					}
					else
					{
						// Backward compatibility
						groups.Add(
							new Group(
								fieldName,
								int.Parse(group.SelectSingleNode("./Index").InnerText),
								columnName
							)
						);
					}
				}
				#endregion

				XmlNodeList predefinedValues = field.SelectNodes("./Values/Value");
				List<object> columnValues = new List<object>();
				List<bool> optional = new List<bool>();

				switch (dataType)
				{
					case "System.Int64":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Int64.Parse(value.InnerText));
						}

						column.DataType = typeof(Int64);
						column.DefaultValue = (Int64)9999999;

						break;
					case "System.Boolean":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Boolean.Parse(value.InnerText));
						}

						column.DataType = typeof(Boolean);
						column.DefaultValue = true;

						break;
					case "System.Decimal":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Decimal.Parse(value.InnerText));
						}

						column.DataType = typeof(Decimal);
						column.DefaultValue = (Decimal)9999999.5959;

						break;
					case "System.Int32":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Int32.Parse(value.InnerText));
						}

						column.DataType = typeof(Int32);
						column.DefaultValue = (Int32)9999999;

						break;
					case "System.Int16":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Int16.Parse(value.InnerText));
						}

						column.DataType = typeof(Int16);
						column.DefaultValue = Int16.MaxValue;

						break;
					case "System.Byte":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Byte.Parse(value.InnerText));
						}

						column.DataType = typeof(Byte);
						column.DefaultValue = Byte.MaxValue;

						break;
					case "System.Double":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Double.Parse(value.InnerText));
						}

						column.DataType = typeof(Double);
						column.DefaultValue = (Double)9999999.5959;

						break;
					case "System.Single":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(Single.Parse(value.InnerText));
						}

						column.DataType = typeof(Single);
						column.DefaultValue = (Single)9999999.5959;

						break;
					case "System.DateTime":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(DateTime.Parse(value.InnerText));
						}

						column.DataType = typeof(DateTime);
						column.DefaultValue = DateTime.Now;

						break;
					case "System.DateTimeOffset":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(DateTimeOffset.Parse(value.InnerText));
						}

						column.DataType = typeof(DateTimeOffset);
						column.DefaultValue = DateTimeOffset.Now;

						break;
					case "System.TimeSpan":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(TimeSpan.Parse(value.InnerText));
						}

						column.DataType = typeof(TimeSpan);
						column.DefaultValue = TimeSpan.FromDays(1);

						break;
					//case "System.Byte[]":
					//	column.DataType = typeof(Double);
					//	column.DefaultValue = new Byte[] { };
					//	break;
					//case "Microsoft.SqlServer.Types.SqlHierarchyId":
					//    column.DataType = typeof(Microsoft.SqlServer.Types.SqlHierarchyId);
					//    break;
					//case "System.Object":
					//	column.DataType = typeof(Object);
					//	column.DefaultValue = new Object();
					//	break;
					case "System.Guid":
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(new Guid(value.InnerText));
						}

						column.DataType = typeof(Guid);
						column.DefaultValue = Guid.NewGuid();

						break;
					case "System.String":
					default:
						foreach (XmlNode value in predefinedValues)
						{
							optional.Add(
								value.Attributes["Optional"] != null && (
									value.Attributes["Optional"].Value.Equals("true") ||
									value.Attributes["Optional"].Value.Equals("1")
								)
							);
							columnValues.Add(value.InnerText);
						}

						column.DataType = typeof(String);
						column.DefaultValue = "Lorem ipsum dolor si amet";

						break;
				}

				column.ExtendedProperties.Add("Values", columnValues);
				column.ExtendedProperties.Add("Optional", optional);
				column.AllowDBNull = field.Attributes["Nullable"] != null && (
					field.Attributes["Nullable"].Value.Equals("true") ||
					field.Attributes["Nullable"].Value.Equals("1")
				);

				dataTable.Columns.Add(column);
			}
			#endregion

			if (behavior == CommandBehavior.SingleResult &&
				dataTable.Columns.Count > 0)
			{
				groups = groups.OrderBy((group) =>
				{
					return group.Index;
				}, Comparer<int>.Default).ToList();

				if (groups.Count > 0)
				{
					FillDataRowByGroup(0, min, max, dataTable.NewRow());
				}
				else
				{
					FillDataRowByQuantity(min, max, dataTable.NewRow());
				}
			}

			dataTable.AcceptChanges();

			return dataTable;
		}

		protected void FillDataRowByGroup(int groupByIndex, int min, int max, sd.DataRow row)
		{
			List<object> availableValues = new List<object>();
			sd.DataColumn groupByColumn = dataTable.Columns[groups[groupByIndex].ColumnName];

			for (int i = 0; i < (groupByColumn.ExtendedProperties["Values"] as List<object>).Count; i++)
			{
				if ((bool)(groupByColumn.ExtendedProperties["Optional"] as List<bool>)[i] &&
					rnd.Next(2).Equals(0))
				{
					continue;
				}

				availableValues.Add((groupByColumn.ExtendedProperties["Values"] as List<object>)[i]);
			}

			if (groupByColumn.AllowDBNull)
			{
				availableValues.Add(DBNull.Value);
			}

			if (availableValues.Count > 0)
			{
				foreach (object availableValue in availableValues)
				{
					row[groups[groupByIndex].ColumnName] = availableValue;
					FillDataColumnByGroup(groups[groupByIndex].Name, ref row);

					if (groupByIndex == groups.Count - 1)
					{
						FillDataRowByQuantity(min, max, row);
					}
					else
					{
						FillDataRowByGroup(groupByIndex + 1, min, max, row);
					}
				}
			}
			else
			{
				row[groups[groupByIndex].ColumnName] = groupByColumn.DefaultValue;
				FillDataColumnByGroup(groups[groupByIndex].Name, ref row);

				if (groupByIndex == groups.Count - 1)
				{
					FillDataRowByQuantity(min, max, row);
				}
				else
				{
					FillDataRowByGroup(groupByIndex + 1, min, max, row);
				}
			}
		}

		protected void FillDataRowByQuantity(int min, int max, sd.DataRow row)
		{
			int qty = rnd.Next(min, max);

			for (int i = 0; i < qty; i++)
			{
				FillDataRow(row);
			}
		}

		protected void FillDataRow(sd.DataRow row)
		{
			sd.DataRow rowToBeAdded = dataTable.NewRow();

			foreach (sd.DataColumn column in dataTable.Columns)
			{
				if (
					// Is a group
					!object.Equals(groups.FirstOrDefault((group) =>
					{
						return group.ColumnName == column.ColumnName;
					}), default(Group)) ||

					// Is a group column
					!object.Equals(groupFields.FirstOrDefault((groupColumn) =>
					{
						return groupColumn.ColumnName == column.ColumnName;
					}), default(GroupField)))
				{
					rowToBeAdded[column.ColumnName] = row[column.ColumnName];
				}
				else
				{
					rowToBeAdded[column.ColumnName] = GenerateDataColumnValue(column);
				}
			}

			dataTable.Rows.Add(rowToBeAdded);
		}

		protected void FillDataColumnByGroup(string groupName, ref sd.DataRow row)
		{
			foreach (GroupField groupField in groupFields.Where((groupField) =>
			{
				return groupField.Reference == groupName;
			}))
			{
				row[groupField.ColumnName] = GenerateDataColumnValue(dataTable.Columns[groupField.ColumnName]);
			}
		}

		protected object GenerateDataColumnValue(sd.DataColumn column)
		{
			List<object> availableValues = new List<object>();

			for (int i = 0; i < (column.ExtendedProperties["Values"] as List<object>).Count; i++)
			{
				if ((bool)(column.ExtendedProperties["Optional"] as List<bool>)[i] &&
					rnd.Next(2).Equals(0))
				{
					continue;
				}

				availableValues.Add((column.ExtendedProperties["Values"] as List<object>)[i]);
			}

			if (column.AllowDBNull)
			{
				availableValues.Add(DBNull.Value);
			}

			if (availableValues.Count > 0)
			{
				return availableValues[rnd.Next(availableValues.Count)];
			}
			else
			{
				return column.DefaultValue;
			}
		}

		public void Cancel() { }
		public void Dispose()
		{
			dataTable.Dispose();
		}
	}
}
