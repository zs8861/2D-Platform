using UnityEngine;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Localization
{
	/// <summary>This contains data about each phrase, which is then translated into different languages.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[HelpURL(LeanLocalization.HelpUrlPrefix + "LeanPhrase")]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Phrase")]
	public class LeanPhrase : LeanSource
	{
		public enum DataType
		{
			Text,
			Object,
			Sprite
		}

		[System.Serializable]
		public class Entry
		{
			/// <summary>The language of this translation.</summary>
			public string Language;

			/// <summary>The translated text.</summary>
			public string Text;

			/// <summary>The translated object (e.g. language specific texture).</summary>
			public Object Object;
		}

		public DataType Data { set { data = value; } get { return data; } } [SerializeField] private DataType data;

		/// <summary>This list stores all translations of this phrase in each language.</summary>
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("translations")]
		private List<Entry> entries;

		public List<Entry> Entries
		{
			get
			{
				if (entries == null)
				{
					entries = new List<Entry>();
				}

				return entries;
			}
		}

		public void Clear()
		{
			if (entries != null)
			{
				entries.Clear();
			}
		}

		public override void Compile(string primaryLanguage, string secondaryLanguage)
		{
			var translation = LeanLocalization.RegisterTranslation(name);

			if (entries != null)
			{
				for (var i = entries.Count - 1; i >= 0; i--)
				{
					var entry = entries[i];

					translation.Register(entry.Language, this);

					if (entry.Language == primaryLanguage)
					{
						Compile(translation, entry, true);
					}
					else if (entry.Language == secondaryLanguage && translation.Primary == false)
					{
						Compile(translation, entry, false);
					}
				}
			}
		}

		private void Compile(LeanTranslation translation, Entry entry, bool primary)
		{
			switch (data)
			{
				case DataType.Text:
				{
					Compile(translation, entry.Text, primary);
				}
				break;
				case DataType.Object:
				case DataType.Sprite:
				{
					Compile(translation, entry.Object, primary);
				}
				break;
			}
		}

		private void Compile(LeanTranslation translation, object data, bool primary)
		{
			translation.Data = data;

			if (primary == true)
			{
				translation.Primary = true;
			}
		}

		/// <summary>This will return the translation of this phrase for the specified language.</summary>
		public bool TryFindTranslation(string languageName, ref Entry entry)
		{
			if (entries != null)
			{
				for (var i = entries.Count - 1; i >= 0; i--)
				{
					entry = entries[i];

					if (entry.Language == languageName)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void RemoveTranslation(string languageName)
		{
			if (entries != null)
			{
				for (var i = entries.Count - 1; i >= 0; i--)
				{
					if (entries[i].Language == languageName)
					{
						entries.RemoveAt(i);

						return;
					}
				}
			}
		}

		/// <summary>Add a new translation to this phrase for the specified language, or return the current one.</summary>
		public Entry AddEntry(string languageName, string text = null, Object obj = null)
		{
			var translation = default(Entry);

			if (TryFindTranslation(languageName, ref translation) == false)
			{
				translation = new Entry();

				translation.Language = languageName;

				if (entries == null)
				{
					entries = new List<Entry>();
				}

				entries.Add(translation);
			}

			translation.Text   = text;
			translation.Object = obj;

			return translation;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Localization
{
	[CustomEditor(typeof(LeanPhrase))]
	public class LeanPhrase_Inspector : LeanInspector<LeanPhrase>
	{
		private static List<string> languageNames = new List<string>();

		private static List<LeanPhrase.Entry> entries = new List<LeanPhrase.Entry>();

		protected override void DrawInspector()
		{
			entries.Clear();
			entries.AddRange(Target.Entries);

			languageNames.Clear();
			languageNames.AddRange(LeanLocalization.CurrentLanguages.Keys);

			Target.Data = (LeanPhrase.DataType)GUILayout.Toolbar((int)Target.Data, new string[] { "Text", "Object", "Sprite" });

			EditorGUILayout.Separator();

			foreach (var languageName in languageNames)
			{
				var entry = default(LeanPhrase.Entry);

				if (Target.TryFindTranslation(languageName, ref entry) == true)
				{
					DrawEntry(entry, false);

					entries.Remove(entry);
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(languageName, EditorStyles.boldLabel);
						if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.Width(45.0f)) == true)
						{
							Undo.RecordObject(Target, "Create Translation");

							Target.AddEntry(languageName);

							Dirty();
						}
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.Separator();
			}

			if (entries.Count > 0)
			{
				foreach (var entry in entries)
				{
					DrawEntry(entry, true);
				}
			}
		}

		private void DrawEntry(LeanPhrase.Entry entry, bool unexpected)
		{
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(entry.Language, EditorStyles.boldLabel);
				if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(55.0f)) == true)
				{
					Undo.RecordObject(Target, "Remove Translation");

					Target.RemoveTranslation(entry.Language);

					Dirty();
				}
			EditorGUILayout.EndHorizontal();

			if (unexpected == true)
			{
				EditorGUILayout.HelpBox("Your LeanLocalization component doesn't define the " + entry.Language + " language.", MessageType.Warning);
			}

			Undo.RecordObject(Target, "Modified Translation");

			EditorGUI.BeginChangeCheck();
			
			switch (Target.Data)
			{
				case LeanPhrase.DataType.Text:
					entry.Text = EditorGUILayout.TextArea(entry.Text ?? "", GUILayout.MinHeight(40.0f));
				break;
				case LeanPhrase.DataType.Object:
					entry.Object = EditorGUILayout.ObjectField(entry.Object, typeof(Object), true);
				break;
				case LeanPhrase.DataType.Sprite:
					entry.Object = EditorGUILayout.ObjectField(entry.Object, typeof(Sprite), true);
				break;
			}

			if (EditorGUI.EndChangeCheck() == true)
			{
				Dirty(); LeanLocalization.UpdateTranslations();
			}

			EditorGUILayout.Separator();
		}

		[MenuItem("Assets/Create/Lean/Localization/Lean Phrase")]
		private static void CreatePhrase()
		{
			LeanHelper.CreateAsset("New Phrase").AddComponent<LeanPhrase>();
		}
	}
}
#endif