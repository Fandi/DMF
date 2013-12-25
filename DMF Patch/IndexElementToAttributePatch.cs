using System.Xml;
using TOJO.Patch;

namespace DMF.Patch
{
	public class IndexElementToAttributePatch : IPatch
	{
		double IPatch.Revision
		{
			get
			{
				return 1.2;
			}
		}

		XmlDocument IPatch.Patch(XmlDocument source)
		{
			foreach (XmlNode groupIndex in source.DocumentElement.SelectNodes("./Field/Group/Index"))
			{
				int? index = null;
				int tmpIndex;

				if (int.TryParse(groupIndex.InnerText, out tmpIndex))
				{
					index = tmpIndex;
				}

				XmlNode groupNode = groupIndex.ParentNode;
				groupNode.RemoveChild(groupIndex);

				if (groupNode.Attributes["Index"] == null)
				{
					groupNode.Attributes.Append(source.CreateAttribute("Index"));
				}

				if (!int.TryParse(groupNode.Attributes["Index"].Value, out tmpIndex))
				{
					groupNode.Attributes["Index"].Value = index.ToString();
				}

				if (groupNode.ChildNodes.Count == 0)
				{
					(groupNode as XmlElement).IsEmpty = true;
				}
			}

			return source;
		}
	}
}
