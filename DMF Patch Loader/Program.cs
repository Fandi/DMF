using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using TOJO.ReportServerExtension.Prototype;

namespace TOJO.Patch
{
	class Program
	{
		const string CONFIG_FILENAME = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\PrivateAssemblies\RSReportDesigner.config";

		static IEnumerable<IPatch> patches = default(IEnumerable<IPatch>);

		static void Main(string[] args)
		{
			PatchSpecificationFiles();
		}

		static IEnumerable<IPatch> Patches
		{
			get
			{
				if (patches == default(IEnumerable<IPatch>))
				{
					patches = GetPatches();
				}

				return patches;
			}
		}

		private static IEnumerable<IPatch> GetPatches()
		{
			List<IPatch> patches = new List<IPatch>();

			foreach (Assembly assembly in GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (!(typeof(IPatch)).IsAssignableFrom(type))
					{
						continue;
					}

					patches.Add((IPatch)Activator.CreateInstance(type));
				}
			}

			return patches.OrderBy((patch) =>
			{
				return patch.Revision;
			}, Comparer<double>.Default);
		}

		private static IEnumerable<Assembly> GetAssemblies()
		{
			DirectoryInfo curDir = new DirectoryInfo(Directory.GetCurrentDirectory());

			// no *.exe
			foreach (FileInfo assemblyFile in curDir.GetFiles("*.dll"))
			{
				yield return Assembly.LoadFrom(assemblyFile.FullName);
			}
		}

		private static void PatchSpecificationFiles()
		{
			if (Patches.Count() == 0)
			{
				return;
			}

			DataSourceConfiguration dsc = GetDataSourceConfiguration(CONFIG_FILENAME);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.Indent = true;
			settings.IndentChars = "    ";

			foreach (FileInfo dmf in dsc.SpecificationFolder.GetFiles("*" + dsc.FileExtension, SearchOption.AllDirectories))
			{
				XmlDocument dmfXML = new XmlDocument();

				using (StreamReader reader = new StreamReader(dmf.FullName))
				{
					try
					{
						dmfXML.LoadXml(reader.ReadToEnd());
					}
					catch
					{
					}
				}

				if (dmfXML == null ||
					dmfXML.DocumentElement == null)
				{
					continue;
				}

				foreach (IPatch patch in Patches)
				{
					dmfXML = patch.Patch(dmfXML);
				}

				dmfXML.Normalize();

				using (XmlWriter writer = XmlWriter.Create(dmf.FullName, settings))
				{
					dmfXML.Save(writer);
				}
			}
		}

		private static DataSourceConfiguration GetDataSourceConfiguration(string rsReportDesignerConfigFileName)
		{
			DataSourceConfiguration dsc = default(DataSourceConfiguration);

			if (File.Exists(rsReportDesignerConfigFileName))
			{
				XmlDocument config = new XmlDocument();

				using (StreamReader reader = new StreamReader(rsReportDesignerConfigFileName))
				{
					config.LoadXml(reader.ReadToEnd());
				}

				if (config != null &&
					config.DocumentElement != null)
				{
					XmlNode configNode = config.DocumentElement.SelectSingleNode("./Extensions/Data/Extension[@Name='TOJO Data Source']/Configuration/ExtensionConfiguration");

					if (configNode != null)
					{
						dsc = new DataSourceConfiguration();
						dsc.Configure(null, configNode.OuterXml);
					}
				}
			}

			return dsc;
		}
	}
}
