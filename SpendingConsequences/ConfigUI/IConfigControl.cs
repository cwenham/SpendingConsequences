using System;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public interface IConfigControl
	{
		event EventHandler<ConfigurableValueChanged> ValueChanged;
	}
	
	public class ConfigurableValueChanged : EventArgs
	{
		public ConfigurableValue ConfigValue { get; private set; }
		
		public ConfigurableValueChanged(ConfigurableValue val)
		{
			this.ConfigValue = val;
		}
	}
}

