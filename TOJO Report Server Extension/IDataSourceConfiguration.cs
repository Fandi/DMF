using Microsoft.ReportingServices.DataProcessing;

namespace TOJO.ReportServerExtension
{
	public interface IDataSourceConfiguration
	{
		void Configure(IDbConnection connection, string configuration);
		IDbCommand CreateCommand();
		IDbTransaction CreateTransaction();
	}
}
