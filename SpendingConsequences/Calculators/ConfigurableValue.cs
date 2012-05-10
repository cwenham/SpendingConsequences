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
				return Definition.Attribute ("Label") != null ? Definition.Attribute ("Label").Value : "Label";
			}
		}
		
		public String ID {
			get {
				return Definition.Attribute("ID") != null ? Definition.Attribute("ID").Value : null;
			}
		}
		
		/// <summary>
		/// Returns true if a UI should not be presented for changing the value
		/// </summary>
		/// <remarks>Used for values that may be computed from other sources, fetched from web services, etc.</remarks>
		public bool SuppressUI {
			get {
				bool _suppress = false;
				if (Definition.Attribute ("SuppressUI") != null)
					bool.TryParse (Definition.Attribute ("SuppressUI").Value, out _suppress);
				return _suppress;
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
		
		private static UserCalculatorSettings UserSettings = new UserCalculatorSettings();
		
		public object Value {
			get {
				if (_value == null) {
					_value = UserSettings.GetCustomValue (this);
					if (_value == null)
						_value = Definition.Attribute ("Value").Value;
				}
				
				switch (ValueType) {
				case ConfigurableValueType.Integer:
					return int.Parse (_value);
				case ConfigurableValueType.Money:
					return decimal.Parse (_value);
				case ConfigurableValueType.Percentage:
					return double.Parse (_value);
				case ConfigurableValueType.Year:
					return int.Parse (_value);
				case ConfigurableValueType.Months:
					return int.Parse (_value);
				case ConfigurableValueType.String:
					return _value;
				default:
					return _value;
				}
			}
			set {
				_value = value.ToString();
				if (value != null)
					UserSettings.StoreCustomValue (this, value.ToString());
			}
		}
		private string _value = null;
	}
	
	public enum ConfigurableValueType
	{
		Undefined,
		Integer,
		Money,
		Year,
		Percentage,
		Months,
		String
	}
}

