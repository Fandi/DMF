using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using TOJO.Patch;

namespace DMF.Patch
{
	public class GroupReferenceFromFieldNamePatch : IPatch
	{
		double IPatch.Revision
		{
			get
			{
				return 1.3;
			}
		}

		XmlDocument IPatch.Patch(XmlDocument dmf)
		{
			foreach (XmlNode groupRef in dmf.DocumentElement.SelectNodes("./Field/Group[@Ref]/@Ref"))
			{
				string groupRefName = (groupRef as XmlAttribute).Value;
				XmlNode node = dmf.DocumentElement.SelectSingleNode("./Field/DataField[text() = '" + groupRefName + "']/../@Name");

				if (node == null)
				{
					node = dmf.DocumentElement.SelectSingleNode("./Field/Value[text() = '" + groupRefName + "']/../@Name");
				}

				if (node != null)
				{
					groupRefName = node.Value;
				}

				(groupRef as XmlAttribute).Value = groupRefName;
			}

			return dmf;
		}
	}
}
