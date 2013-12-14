using Microsoft.ReportingServices.DataProcessing;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class DataParameter : IDataParameter
	{
		public string ParameterName
		{
			get;
			set;
		}

		public object Value
		{
			get;
			set;
		}
	}
}
