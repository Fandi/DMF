using Microsoft.ReportingServices.DataProcessing;
using System.Collections;
using System.Collections.Generic;

namespace TOJO.ReportServerExtension.Prototype
{
	internal class DataParameterCollection : List<IDataParameter>, IDataParameterCollection
	{
		int IDataParameterCollection.Add(IDataParameter parameter)
		{
			(this as List<IDataParameter>).Add(parameter);
			return (this as List<IDataParameter>).IndexOf(parameter);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (this as List<IDataParameter>).GetEnumerator();
		}
	}
}
