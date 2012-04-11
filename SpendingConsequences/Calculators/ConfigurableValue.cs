using System;
using System.Linq;
using System.Xml.Linq;

namespace SpendingConsequences.Calculators
{
	public class ConfigurableValue
	{
		public ConfigurableValue (XElement definition)
		{
			this.Definition = definition;
		}
		
		public XElement Definition { get; private set; }
		
		public String Name {
			get {
				return Definition.Attribute ("Name") != null ? Definition.Attribute ("Name").Value : "Value";
			}
		}
		
		public String Label {
			get {
				return Definition.Attribute ("Label") != null ? Definition.Attribute ("Label").Value : "Value";
			}
		}
		
		public ConfigurableValueType ValueType {
			get {
				if (_ValueType == ConfigurableValueType.Undefined && Definition.Attribute ("Type") != null)
					_ValueType = ((ConfigurableValueType)Enum.Parse (typeof(ConfigurableValueType), Definition.Attribute ("Type").Value, true));
				
				return _ValueType;
			}
		}
		private ConfigurableValueType _ValueType = ConfigurableValueType.Undefined;
		
		public object Value {
			get {
				switch (ValueType) {
				case ConfigurableValueType.Integer:
					return int.Parse (Definition.Attribute ("Value").Value);
				case ConfigurableValueType.Money:
					return decimal.Parse (Definition.Attribute ("Value").Value);
				case ConfigurableValueType.Percentage:
					return double.Parse (Definition.Attribute ("Value").Value);
				case ConfigurableValueType.Year:
					return int.Parse (Definition.Attribute ("Value").Value);
				case ConfigurableValueType.Months:
					return int.Parse (Definition.Attribute ("Value").Value);
				default:
					return null;
				}
			}
			set {
				Definition.Attribute("Value").SetValue(value);
			}
		}
	}
	
	public enum ConfigurableValueType
	{
		Undefined,
		Integer,
		Money,
		Year,
		Percentage,
		Months
	}
}

