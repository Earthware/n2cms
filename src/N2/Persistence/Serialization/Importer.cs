using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using N2.Details;
using N2.Persistence;
using System.Diagnostics;

namespace N2.Persistence.Serialization
{
	public class Importer
	{
		private readonly IPersister persister;
		private readonly ItemXmlReader reader;

		public Importer(IPersister persister, ItemXmlReader reader)
		{
			this.persister = persister;
			this.reader = reader;
		}

		public virtual IImportRecord Read(string path)
		{
			using (Stream input = File.OpenRead(path))
			{
				return Read(input, Path.GetFileName(path));
			}
		}

		public virtual IImportRecord Read(Stream input, string filename)
		{
			return Read(new StreamReader(input));
		}

		public virtual IImportRecord Read(TextReader input)
		{
			XPathNavigator navigator = CreateNavigator(input);

			navigator.MoveToRoot();
			if (!navigator.MoveToFirstChild())
				throw new DeserializationException("Expected root node 'n2' not found");

			int version = ReadExportVersion(navigator);
			if (2 != version)
				throw new WrongVersionException("Invalid export version, expected 2 but was " + version);

			return reader.Read(navigator);
		}

		protected virtual XPathNavigator CreateNavigator(TextReader input)
		{
			return new XPathDocument(input).CreateNavigator();
		}

		protected virtual int ReadExportVersion(XPathNavigator navigator)
		{
			return int.Parse(navigator.GetAttribute("exportVersion", string.Empty));
		}

		public virtual void Import(IImportRecord record, ContentItem destination, ImportOption options)
		{
			ResetIDs(record.ReadItems);
			if ((options & ImportOption.AllItems) == ImportOption.AllItems)
			{
				record.RootItem.AddTo(destination);
				persister.Save(record.RootItem);
			}
			else if ((options & ImportOption.Children) == ImportOption.Children)
			{
				RemoveReferences(record.ReadItems, record.RootItem);
				while (record.RootItem.Children.Count > 0)
				{
					ContentItem child = record.RootItem.Children[0];
					child.AddTo(destination);
					persister.Save(child);
				}
			}
			else
			{
				throw new NotImplementedException("This option isn't implemented, sorry.");
			}
			if ((options & ImportOption.Attachments) == ImportOption.Attachments)
			{
				foreach(Attachment a in record.Attachments)
				{
                    try
                    {
                        a.Import();
                    }
                    catch (Exception ex)
                    {
                        Trace.Write(ex);
                        record.FailedAttachments.Add(a);
                    }
				}
			}
		}

		protected virtual void RemoveReferences(IEnumerable<ContentItem> items, ContentItem referenceToRemove)
		{
			foreach (ContentItem item in items)
			{
				RemoveDetailReferences(referenceToRemove, item);
				RemoveReferencesInCollections(referenceToRemove, item);
			}
		}

		protected virtual void RemoveDetailReferences(ContentItem referenceToRemove, ContentItem item)
		{
			List<string> keys = new List<string>(item.Details.Keys);
			foreach (string key in keys)
			{
				ContentDetail detail = item.Details[key];
				if (detail.ValueType == typeof(ContentItem))
				{
					if (((LinkDetail)detail).LinkedItem == referenceToRemove)
					{
						item.Details.Remove(key);
					}
				}
			}
		}

		protected virtual void RemoveReferencesInCollections(ContentItem referenceToRemove, ContentItem item)
		{
			foreach (DetailCollection collection in item.DetailCollections.Values)
			{
				for (int i = collection.Details.Count - 1; i >= 0; --i)
				{
					ContentDetail detail = collection.Details[i];
					if (detail.ValueType == typeof(ContentItem))
					{
						if (((LinkDetail)detail).LinkedItem == referenceToRemove)
						{
							collection.Remove(referenceToRemove);
						}
					}
				}
			}
		}

		protected virtual void ResetIDs(IEnumerable<ContentItem> items)
		{
			foreach(ContentItem item in items)
			{
				item.ID = 0;
			}
		}
	}
}
