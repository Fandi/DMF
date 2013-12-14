using Microsoft.ReportingServices.DataProcessing;
using TOJO.ReportServerExtension.Prototype;

namespace TOJO.ReportServerExtension
{
	public class Connection : IDbConnection, IDbConnectionExtension
	{
		public string ConnectionString { get; set; }
		public int ConnectionTimeout { get; protected set; }
		public string LocalizedName { get; protected set; }
		public IDataSourceConfiguration Configuration { get; protected set; }

		public Connection()
		{
			ConnectionString = string.Empty;
			ConnectionTimeout = 5;
			LocalizedName = typeof(Connection).FullName;
		}

		public IDbTransaction BeginTransaction()
		{
			return Configuration.CreateTransaction();
		}

		public IDbCommand CreateCommand()
		{
			return Configuration.CreateCommand();
		}

		public void SetConfiguration(string configuration)
		{
			// TODO: provide ConfigurationSection based on report server extension configuration, using System.Reflection
			Configuration = new DataSourceConfiguration();
			Configuration.Configure(this as IDbConnection, configuration);
		}

		public void Open() { }
		public void Close() { }
		public void Dispose() { }

		public string Impersonate
		{
			protected get;
			set;
		}

		public bool IntegratedSecurity
		{
			get;
			set;
		}

		public string Password
		{
			protected get;
			set;
		}

		public string UserName
		{
			protected get;
			set;
		}
	}
}
