using Microsoft.ReportingServices.DataProcessing;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class DataMultiValueParameter : IDataMultiValueParameter
	{
		public object[] Values
		{
			get;
			set;
		}

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
