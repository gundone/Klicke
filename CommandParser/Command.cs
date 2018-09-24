using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;

namespace CommandProcessor
{
	public class Command
	{
		public readonly string Name;
		public readonly Dictionary<string, string> Attr;

		Command(string name, Dictionary<string, string> attr)
			: this()
		{
			Name = name;
			Attr = attr;
		}

		Command()
		{
			Name = string.Empty;
			Attr = new Dictionary<string, string>();
		}

		public static Command Parse(string input)
		{
			var regName = new Regex(@"^[^:\n]*(?=:.*$)");
			var name = regName.Match(input).Value;
			var regParam = new Regex(@"\w+=[^;]+");

			Dictionary<string, string> attrib = new Dictionary<string, string>();
			var attribStr = regParam
				.Matches(input);
			foreach (Match match in attribStr)
			{
				var par = match.Value;
				var pair = par.Split('=');
				var key = pair.First();
				var val = pair.Skip(1).First();
				attrib.Add(key, val);
			}
			return new Command(name, attrib);
		}
	}
}