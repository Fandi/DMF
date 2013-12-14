using Microsoft.ReportingServices.DataProcessing;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class Transaction : IDbTransaction
	{
		public void Commit() { }
		public void Rollback() { }
		public void Dispose() { }
	}
}
