using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;

using ETFLib.Composition;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public class AppProfile
	{
		public AppProfile (String masterProfilePath)
		{
			SubProfiles = new Dictionary<string, SubProfile>();

			SubProfile master = ACompositionTree.Load<SubProfile>(masterProfilePath);
			if (master != null)
				AddSubProfile("main", master);
		}

		public Dictionary<String,SubProfile> SubProfiles { get; private set; }

		public void AddSubProfile(String name, SubProfile subProfile)
		{
			SubProfiles.Add(name, subProfile);
		}

		public SubProfile GetSubProfile(String name)
		{
			if (SubProfiles.ContainsKey(name))
				return SubProfiles[name];
			else
				return null;
		}

		public IEnumerable<ACalculator> AllCalculators {
			get {
				return SubProfiles.Values.Where(x => x.Calculators != null).SelectMany(x => x.Calculators);
			}
		}

		public IDictionary<String,XElement> AllConsequenceTemplates {
			get {
				return SubProfiles.Values.Where(x => x.ConsequenceTemplates != null).SelectMany(x => x.ConsequenceTemplates).ToDictionary(x => x.Key, y => y.Value);
			}
		}

		public XElement GetResultTemplate (String name)
		{
			var matches = from sp in SubProfiles.Values
				where sp.ResultTemplates != null
				from t in sp.ResultTemplates
					where t.Key.Equals(name, StringComparison.OrdinalIgnoreCase)
					select t.Value;

			return matches.FirstOrDefault();
		}
	}
}

