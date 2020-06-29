using UnityEngine;
using System.Collections.Generic;

namespace Lean.Localization
{
	/// <summary>This class stores information about a single language, and any of its optional cultures.</summary>
	[System.Serializable]
	public class LeanLanguage
	{
		[SerializeField]
		private string name;

		[SerializeField]
		private List<string> cultures;

		public string Name
		{
			set
			{
				name = value;
			}

			get
			{
				return name;
			}
		}

		/// <summary>This culture names for this language (e.g. en-GB, en-US).</summary>
		public List<string> Cultures
		{
			get
			{
				if (cultures == null)
				{
					cultures = new List<string>();
				}

				return cultures;
			}
		}
	}
}