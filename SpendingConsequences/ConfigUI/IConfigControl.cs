using System;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public interface IConfigControl
	{
		event EventHandler<ConfigurableValueChanged> ValueChanged;

		event EventHandler<CurrencyChangeEventArgs> CurrencyButtonClicked;
	}
	
	public class ConfigurableValueChanged : EventArgs
	{
		public ConfigurableValue ConfigValue { get; private set; }
		
		public ConfigurableValueChanged(ConfigurableValue val)
		{
			this.ConfigValue = val;
		}
	}

	public class CurrencyChangeEventArgs : EventArgs
	{
		public ConfigurableValue ConfigValue { get; private set; }

		public CurrencyChangeEventArgs(ConfigurableValue val)
		{
			this.ConfigValue = val;
		}
	}
}

