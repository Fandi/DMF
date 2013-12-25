using System.Xml;

namespace TOJO.Patch
{
	public interface IPatch
	{
		// This property is most likely to be revised
		double Revision { get; }
		XmlDocument Patch(XmlDocument dmf);
	}
}
