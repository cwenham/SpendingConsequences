using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SpendingConsequences.Calculators
{
	/// <summary>
	/// Abstract base for supporting calculator elements such as Commentary and Image.
	/// </summary>
	public abstract class ASupportElement
	{
		public ASupportElement (XElement definition)
		{
			this.Definition = definition;
		}
		
		public XElement Definition { get; private set; }
		
		public String Name {
			get {
				return Definition.Attribute ("Name") != null ? Definition.Attribute ("Name").Value : Definition.Name.LocalName;
			}
		}
		
		#region Static methods
		public static Dictionary<string,Type> SupportElementTypes ()
		{
			if (_supportElementTypes == null)
				_supportElementTypes = (from t in Assembly.GetCallingAssembly ().GetTypes ()
				              where t.IsSubclassOf (typeof(ASupportElement))
				              select new {
					name = t.Name,
					typ = t
				}).ToDictionary (x => x.name, y => y.typ);
			
			return _supportElementTypes;
		}
		private static Dictionary<string, Type> _supportElementTypes = null;
		
		public static string[] KnownElementNames ()
		{
			if (_knownElementNames == null)
				_knownElementNames = SupportElementTypes ().Keys.ToArray ();
			
			return _knownElementNames;
		}
		private static string[] _knownElementNames = null;
		
		public static Type ElementType (XElement definition)
		{
			if (SupportElementTypes().ContainsKey (definition.Name.LocalName))
				return _supportElementTypes [definition.Name.LocalName];
			else
				return null;
		}
		
		public static ASupportElement GetInstance (XElement definition)
		{
			Type eType = ElementType (definition);
			if (eType != null) {
				if (definition.Annotation (eType) != null)
					return definition.Annotation (eType) as ASupportElement;
				else {
					var instance = Activator.CreateInstance (eType, new object[] { definition });
					definition.AddAnnotation (instance);
					return instance as ASupportElement;
				}
			} else
				return null;
		}
		#endregion
	}
}

