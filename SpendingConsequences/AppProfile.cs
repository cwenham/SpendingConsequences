using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;

using SpendingConsequences.Calculators;

namespace SpendingConsequences
{
	public class AppProfile
	{
		public AppProfile (String masterProfilePath)
		{
			SubProfiles = new Dictionary<string, SubProfile>();

			SubProfile master = SubProfile.Load(masterProfilePath);
			if (master != null)
				AddSubProfile("main", master);
		}

		public Dictionary<String,SubProfile> SubProfiles { get; private set; }

		public void AddSubProfile(String name, SubProfile subProfile)
		{
			SubProfiles.Add(name, subProfile);
		}

		public IEnumerable<ACalculator> AllCalculators {
			get {
				return SubProfiles.Values.SelectMany(x => x.Calculators);
			}
		}

		public IDictionary<String,XElement> AllConsequenceTemplates {
			get {
				return SubProfiles.Values.SelectMany(x => x.ConsequenceTemplates).ToDictionary(x => x.Key, y => y.Value);
			}
		}

		public XElement GetResultTemplate (String name)
		{
			var matches = from sp in SubProfiles.Values
				from t in sp.ResultTemplates
					where t.Key.Equals(name, StringComparison.OrdinalIgnoreCase)
					select t.Value;

			return matches.FirstOrDefault();
		}
	}
}

