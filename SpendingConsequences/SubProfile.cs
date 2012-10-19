using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using ETFLib;
using ETFLib.Composition;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	/// <summary>
	/// Represent an XML document that configures the application's behavior
	/// </summary>
	public class SubProfile : ACompositionTree
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
			XDocument definition = new XDocument(new XElement(NS.Composition + "Calculators"));
			definition.Root.Add(new XElement(NS.Composition + "Consequences"));

			String filePath = Path.Combine(LibraryFolder,String.Format("{0}.sbprofile", name));
			definition.Save(filePath);

			return ACompositionTree.Load<SubProfile>(filePath);
		}
		
		public SubProfile (XDocument profileDoc) : base(profileDoc)
		{
		}

		public ACalculator AddConsequenceFromDefinition(XElement definition)
		{
			XElement calcContainer = this.Definition.Root.Element(NS.Composition + "Consequences");
			if (calcContainer == null)
			{
				calcContainer = new XElement(NS.Composition + "Consequences");
				this.Definition.Root.Add(calcContainer);
			}

			calcContainer.Add(definition);
			ACalculator calc = AComposable.GetInstance<ACalculator>(definition);

			return calc;
		}

		public IEnumerable<ACalculator> Calculators {
			get {
				if (_calculatorContainer == null)
					_calculatorContainer = this.Root.GetChild<AComposable>("Consequences");

				if (_calculatorContainer != null)
					return _calculatorContainer.Children.OfType<ACalculator>();
				else
					return null;
			}
		}
		private AComposable _calculatorContainer;

		public IEnumerable<Template> ResultTemplates {
			get {
				if (_resultTemplateContainer == null && this.Root != null)
					_resultTemplateContainer = this.Root.GetChild<AComposable>("ResultTemplates");
				if (_resultTemplateContainer != null)
					return _resultTemplateContainer.Children.OfType<Template>();
				else
					return null;
			}
		}
		private AComposable _resultTemplateContainer;
		
		public IEnumerable<Template> ConsequenceTemplates { 
			get {
				if (_consequenceTemplateContainer == null && this.Root != null)
					_consequenceTemplateContainer = this.Root.GetChild<AComposable>("ConsequenceTemplates");
				if (_consequenceTemplateContainer != null)
					return _consequenceTemplateContainer.Children.OfType<Template>();
				else
					return null;
			}
		}
		private AComposable _consequenceTemplateContainer;
	}
}

