using UnityEngine;
using System.Collections.Generic;

namespace Lean.Localization
{
	/// <summary>This contains the translated value for the current language, and other associated data.</summary>
	public class LeanTranslation
	{
		public struct Entry
		{
			public string Language;

			public Object Owner;
		}

		/// <summary>The name of this translation.</summary>
		public string Name { get { return name; } } [SerializeField] private string name;

		/// <summary>The data of this translation (e.g. string or Object).
		/// NOTE: This is a System.Object, so you must correctly cast it back before use.</summary>
		public object Data;

		/// <summary>If Data has been filled with data for the primary language, then this will be set to true.</summary>
		public bool Primary;

		/// <summary>This stores a list of all LeanSource instances that are currently managing the current value of this translation in the current language.
		/// NOTE: If this is empty then no LeanSource of this name is localized for the current language.</summary>
		public List<Entry> Entries { get { return entries; } } private List<Entry> entries = new List<Entry>();

		private static bool buffering;

		private static System.Text.StringBuilder current = new System.Text.StringBuilder();

		private static System.Text.StringBuilder buffer = new System.Text.StringBuilder();

		private static List<LeanToken> tokens = new List<LeanToken>();

		public LeanTranslation(string newName)
		{
			name = newName;
		}

		public void Register(string language, Object owner)
		{
			var entry = new Entry();

			entry.Language = language;
			entry.Owner    = owner;

			entries.Add(entry);
		}

		public void Clear()
		{
			Data    = null;
			Primary = false;

			entries.Clear();
		}

		public int LanguageCount(string language)
		{
			var total = 0;

			for (var i = entries.Count - 1; i >= 0; i--)
			{
				if (entries[i].Language == language)
				{
					total += 1;
				}
			}

			return total;
		}

		/// <summary>This returns Text with all tokens substituted using the LeanLocalization.Tokens list.</summary>
		public static string FormatText(string rawText, string currentText = null, ILocalizationHandler handler = null)
		{
			if (string.IsNullOrEmpty(currentText) == true)
			{
				currentText = rawText;
			}

			if (rawText != null)
			{
				current.Length = 0;
				buffer.Length = 0;
				tokens.Clear();

				for (var i = 0; i < rawText.Length; i++)
				{
					var rawChar = rawText[i];

					if (rawChar == '{')
					{
						if (buffering == true)
						{
							buffering = false;

							buffer.Length = 0;
						}
						else
						{
							buffering = true;
						}
					}
					else if (rawChar == '}')
					{
						if (buffering == true)
						{
							if (buffer.Length > 0)
							{
								var token = default(LeanToken);

								if (buffer.Length > 0 && LeanLocalization.CurrentTokens.TryGetValue(buffer.ToString(), out token) == true) // TODO: Avoid ToString here?
								{
									current.Append(token.Value);

									tokens.Add(token);
								}
								else
								{
									current.Append('{').Append(buffer).Append('}');
								}

								buffer.Length = 0;
							}

							buffering = false;
						}
					}
					else
					{
						if (buffering == true)
						{
							buffer.Append(rawChar);
						}
						else
						{
							current.Append(rawChar);
						}
					}
				}

				if (Match(currentText, current) == false)
				{
					if (handler != null)
					{
						handler.UnregisterAll();

						for (var i = tokens.Count - 1; i >= 0; i--)
						{
							var token = tokens[i];

							token.Register(handler);

							handler.Register(token);
						}
					}

					return current.ToString();
				}
			}

			return currentText;
		}

		private static bool Match(string a, System.Text.StringBuilder b)
		{
			if (a == null && b.Length > 0)
			{
				return false;
			}

			if (a.Length != b.Length)
			{
				return false;
			}

			for (var i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}