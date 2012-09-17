using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent an XML document that configures the application's behavior
	/// </summary>
	public class SubProfile
	{
		public static String LibraryFolder
		{
			get {
				if (String.IsNullOrWhiteSpace(_libraryFolder))
					_libraryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

				return _libraryFolder;
			}
		}
		private static String _libraryFolder;

		public static String InboxFolder
		{
			get {
				if (String.IsNullOrWhiteSpace(_inboxFolder))
					_inboxFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Inbox");

				return _inboxFolder;
			}
		}
		private static String _inboxFolder;

		/// <summary>
		/// Create a new empty profile and save it to the LibraryFolder
		/// </summary>
		/// <param name='name'>
		/// Name of profile. Will be used to name the file
		/// </param>
		public static SubProfile Create (String name)
		{
			XDocument definition = new XDocument(new XElement(NS.Profile + "Calculators"));
			definition.Root.Add(new XElement(NS.Profile + "Consequences"));

			String filePath = Path.Combine(LibraryFolder,String.Format("{0}.sbprofile", name));
			definition.Save(filePath);

			return SubProfile.Load(filePath);
		}

		public static SubProfile Load (String uri)
		{
			try {
				XDocument definition = XDocument.Load (uri, LoadOptions.SetBaseUri);
				if (definition != null && definition.Root.Name.Namespace == NS.Profile)
					return new SubProfile (definition);
				else
				{
					Console.WriteLine("Null or invalid profile: {0}", uri);
					return null;
				}
			} catch (Exception ex) {
				Console.WriteLine("{0} thrown when loading {1}: {2}", ex.GetType().Name, uri, ex.Message);
				return null;
			}
		}
		
		private SubProfile (XDocument profileDoc)
		{
			this.Definition = profileDoc;
			this.Definition.AddAnnotation (this);

			this.Definition.Root.Changed += delegate(object sender, XObjectChangeEventArgs e) {
				Console.WriteLine ("Received XDocument changed event: {0}", this.Definition.BaseUri);
				Uri uriPath = new Uri (this.Definition.BaseUri);
				Console.WriteLine ("Saving to: {0}", uriPath.LocalPath);
				Definition.Save (uriPath.LocalPath);

				XElement subject = sender as XElement;
				if (subject != null)
					switch (e.ObjectChange) {
					case XObjectChange.Remove:
					    ACalculator victim = ACalculator.GetInstance(subject, false);
					    if (victim != null)
						    Calculators.Remove(victim);
					break;
					case XObjectChange.Add:
					    ACalculator calc = ACalculator.GetInstance(subject);
					    if (calc != null && !Calculators.Contains(calc))
						    Calculators.Add(calc);
					break;
					}
			};

			var consequencesElement = Definition.Root.Element (NS.Profile + "Consequences");
			if (consequencesElement != null) {
				var calculators = from e in consequencesElement.Elements ()
					where e.Name.Namespace == NS.Profile
					&& ACalculator.CalcType (e) != null
						select ACalculator.GetInstance (e);
				if (calculators != null)
					this.Calculators = calculators.ToList ();
			}
				
			var resultTemplatesElement = Definition.Root.Element (NS.Profile + "ResultTemplates");

			if (resultTemplatesElement != null)
				ResultTemplates = resultTemplatesElement.Elements ()
						.Where (x => x.Attribute ("Name") != null)
						.ToDictionary (x => x.Attribute ("Name").Value, y => y.Element (NS.XSLT + "stylesheet"));

			var consequenceTemplatesElement = Definition.Root.Element (NS.Profile + "ConsequenceTemplates");
			if (consequenceTemplatesElement != null)
				ConsequenceTemplates = consequenceTemplatesElement.Elements ()
						.Where (x => x.Attribute ("Name") != null)
						.ToDictionary (x => x.Attribute ("Name").Value);
		}

		public ACalculator AddConsequenceFromDefinition(XElement definition)
		{
			XElement calcContainer = this.Definition.Root.Element(NS.Profile + "Consequences");
			if (calcContainer == null)
			{
				calcContainer = new XElement(NS.Profile + "Consequences");
				this.Definition.Root.Add(calcContainer);
			}

			XElement assimilatedDef = XElement.Parse(definition.ToString());
			calcContainer.Add(assimilatedDef);
			ACalculator calc = ACalculator.GetInstance(assimilatedDef);

			return calc;
		}

		public Boolean IsUserEditable {
			get {
				if (!_isUserEditableSet)
				{
					var editAttribute = this.Definition.Root.Attribute("UserEditable");
					if (editAttribute != null)
						Boolean.TryParse(editAttribute.Value, out _isUserEditable);

					_isUserEditableSet = true;
				}

				return _isUserEditable;
			}
		}
		private Boolean _isUserEditable = true;
		private Boolean _isUserEditableSet = false;

		public XDocument Definition { get; private set; }
		
		public List<ACalculator> Calculators { get; private set; }
		
		public Dictionary<String, XElement> ResultTemplates { get; private set; }
		
		public Dictionary<String, XElement> ConsequenceTemplates { get; private set; }
	}
}

