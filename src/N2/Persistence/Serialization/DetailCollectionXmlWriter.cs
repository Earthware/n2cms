using System.Xml;
using N2.Details;

namespace N2.Persistence.Serialization
{
	public class DetailCollectionXmlWriter : DetailXmlWriter
	{
		public override void Write(ContentItem item, XmlTextWriter writer)
		{
			using (new ElementWriter("detailCollections", writer))
			{
				foreach (DetailCollection collection in item.DetailCollections.Values)
				{
					WriteDetailCollection(writer, collection);
				}
			}
		}

		protected virtual void WriteDetailCollection(XmlTextWriter writer, DetailCollection collection)
		{
			using (ElementWriter collectionElement = new ElementWriter("collection", writer))
			{
				collectionElement.WriteAttribute("name", collection.Name);
				foreach (ContentDetail detail in collection.Details)
				{
					WriteDetail(detail, writer);
				}
			}
		}
	}
}
