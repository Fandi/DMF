using Microsoft.ReportingServices.DataProcessing;
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace TOJO.ReportServerExtension.Prototype
{
	public class DataSourceConfiguration : IDataSourceConfiguration
	{
		private static XmlSchema _validatorSchema = null;
		internal const string ReportDesignerSchemaURI = "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner";

		public DirectoryInfo SpecificationFolder { get; protected set; }
		public string FileExtension { get; protected set; }

		public static XmlSchema ValidatorSchema
		{
			get
			{
				if (_validatorSchema == null)
				{
					using (Stream stream = typeof(Connection).Assembly.GetManifestResourceStream("TOJO.ReportServerExtension.DataModelFormat.xsd"))
					{
						_validatorSchema = XmlSchema.Read(stream, null);
					}
				}

				return _validatorSchema;
			}
		}

		public void Configure(IDbConnection connection, string configuration)
		{
			XmlDocument configurationXML = new XmlDocument();
			configurationXML.LoadXml(configuration);

			SpecificationFolder = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "PrivateAssemblies"));
			FileExtension = ".dmf";

			if (configurationXML != null &&
				configurationXML.DocumentElement != null)
			{
				XmlNode specificationFolder = configurationXML.DocumentElement.SelectSingleNode("./SpecificationFolder");

				if (specificationFolder != null &&
					Directory.Exists(specificationFolder.InnerText))
				{
					SpecificationFolder = new DirectoryInfo(specificationFolder.InnerText);
				}

				XmlNode fileExtFolder = configurationXML.DocumentElement.SelectSingleNode("./FileExtension");

				if (fileExtFolder != null)
				{
					FileExtension = fileExtFolder.InnerText;
				}
			}
		}

		public IDbCommand CreateCommand()
		{
			return new Prototype.Command(this) as IDbCommand;
		}

		public IDbTransaction CreateTransaction()
		{
			return new Transaction() as IDbTransaction;
		}
	}
}
