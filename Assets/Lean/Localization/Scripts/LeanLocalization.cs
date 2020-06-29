using UnityEngine;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Localization
{
	/// <summary>This component manages a global list of translations for easy access.
	/// Translations are gathered from the <b>prefabs</b> list, as well as from any active and enabled <b>LeanSource</b> components in the scene.</summary>
	[ExecuteInEditMode]
	[HelpURL(HelpUrlPrefix + "LeanLocalization")]
	[AddComponentMenu(ComponentPathPrefix + "Localization")]
	public class LeanLocalization : MonoBehaviour
	{
		public enum DetectType
		{
			None,
			SystemLanguage,
			CurrentCulture,
			CurrentUICulture
		}

		public const string HelpUrlPrefix = LeanHelper.HelpUrlPrefix + "LeanLocalization#";

		public const string ComponentPathPrefix = LeanHelper.ComponentPathPrefix + "Localization/Lean ";

		/// <summary>All active and enabled LeanLocalization components.</summary>
		public static List<LeanLocalization> Instances = new List<LeanLocalization>();

		public static Dictionary<string, LeanToken> CurrentTokens = new Dictionary<string, LeanToken>();

		public static Dictionary<string, LeanLanguage> CurrentLanguages = new Dictionary<string, LeanLanguage>();

		/// <summary>Dictionary of all the phrase names mapped to their current translations.</summary>
		public static Dictionary<string, LeanTranslation> CurrentTranslations = new Dictionary<string, LeanTranslation>();

		/// <summary>If the application is started and no language has been loaded or auto detected, this language will be used.</summary>
		[LeanLanguageName]
		public string DefaultLanguage;

		/// <summary>How should the cultures be used to detect the user's device language?</summary>
		public DetectType DetectLanguage = DetectType.SystemLanguage;

		/// <summary>Automatically save/load the CurrentLanguage selection to PlayerPrefs? (can be cleared with ClearSave context menu option)</summary>
		public bool SaveLanguage = true;

		[SerializeField]
		private List<LeanLanguage> languages;

		[SerializeField]
		private List<LeanPrefab> prefabs;

		/// <summary>Called when the language or translations change.</summary>
		public static event System.Action OnLocalizationChanged;

		/// <summary>The currently set language.</summary>
		private static string currentLanguage;

		private static bool pendingUpdates;

		private static Dictionary<string, LeanTranslation> tempTranslations = new Dictionary<string, LeanTranslation>();

		/// <summary>This stores all languages and their aliases managed by this LeanLocalization instance.</summary>
		public List<LeanLanguage> Languages
		{
			get
			{
				if (languages == null)
				{
					languages = new List<LeanLanguage>();
				}

				return languages;
			}
		}

		/// <summary>This stores all prefabs and folders managed by this LeanLocalization instance.</summary>
		public List<LeanPrefab> Prefabs
		{
			get
			{
				if (prefabs == null)
				{
					prefabs = new List<LeanPrefab>();
				}

				return prefabs;
			}
		}

		/// <summary>Does at least one localization have 'SaveLanguage' set?</summary>
		public static bool CurrentSaveLanguage
		{
			get
			{
				for (var i = 0; i < Instances.Count; i++)
				{
					if (Instances[i].SaveLanguage == true)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>Change the current language of this instance?</summary>
		public static string CurrentLanguage
		{
			set
			{
				if (CurrentLanguage != value)
				{
					currentLanguage = value;

					if (CurrentSaveLanguage == true)
					{
						SaveNow();
					}

					UpdateTranslations();
				}
			}

			get
			{
				return currentLanguage;
			}
		}

		/// <summary>When rebuilding translations this method is called from any <b>LeanSource</b> components that define a token.</summary>
		public static void RegisterToken(string name, LeanToken token)
		{
			if (string.IsNullOrEmpty(name) == false && token != null && CurrentTokens.ContainsKey(name) == false)
			{
				CurrentTokens.Add(name, token);
			}
		}

		/// <summary>When rebuilding translations this method is called from any <b>LeanSource</b> components that define a transition.</summary>
		public static LeanTranslation RegisterTranslation(string name)
		{
			var translation = default(LeanTranslation);

			if (string.IsNullOrEmpty(name) == false && CurrentTranslations.TryGetValue(name, out translation) == false)
			{
				if (tempTranslations.TryGetValue(name, out translation) == true)
				{
					tempTranslations.Remove(name);

					CurrentTranslations.Add(name, translation);
				}
				else
				{
					translation = new LeanTranslation(name);

					CurrentTranslations.Add(name, translation);
				}
			}

			return translation;
		}

		[ContextMenu("Clear Save")]
		public void ClearSave()
		{
			PlayerPrefs.DeleteKey("LeanLocalization.CurrentLanguage");
		}

		private static void SaveNow()
		{
			PlayerPrefs.SetString("LeanLocalization.CurrentLanguage", currentLanguage);
		}

		private static void LoadNow()
		{
			currentLanguage = PlayerPrefs.GetString("LeanLocalization.CurrentLanguage");
		}

		/// <summary>This sets the current language using the specified string.</summary>
		public void SetCurrentLanguage(string newLanguage)
		{
			CurrentLanguage = newLanguage;
		}

		/// <summary>This sets the current language using the specified index based on the Languages list, where 0 is the first language.</summary>
		public void SetCurrentLanguage(int newLanguageIndex)
		{
			if (newLanguageIndex >= 0 && newLanguageIndex < languages.Count)
			{
				SetCurrentLanguage(languages[newLanguageIndex].Name);
			}
		}

		public bool LanguageExists(string languageName)
		{
			var language = default(LeanLanguage);

			return TryGetLanguage(languageName, ref language);
		}

		public bool TryGetLanguage(string languageName, ref LeanLanguage language)
		{
			if (languages != null)
			{
				for (var i = languages.Count - 1; i >= 0; i--)
				{
					language = languages[i];

					if (language.Name == languageName)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>This adds the specified UnityEngine.Object to this LeanLocalization instance, allowing it to be registered as a prefab.</summary>
		public void AddPrefab(Object root)
		{
			for (var i = Prefabs.Count - 1; i >= 0; i--) // NOTE: Property
			{
				if (prefabs[i].Root == root)
				{
					return;
				}
			}

			var prefab = new LeanPrefab();

			prefab.Root = root;

			prefabs.Add(prefab);
		}

		/// <summary>This adds a new language to this LeanLocalization instance, with the specified name and cultures.</summary>
		public LeanLanguage AddLanguage(string languageName, string[] cultures)
		{
			var language = default(LeanLanguage);

			if (TryGetLanguage(languageName, ref language) == false)
			{
				language = new LeanLanguage();

				language.Name = languageName;

				if (languages == null)
				{
					languages = new List<LeanLanguage>();
				}

				languages.Add(language);
			}

			language.Cultures.Clear();
			language.Cultures.AddRange(cultures);

			return language;
		}

		/// <summary>This calls AddToken on the first active and enabled LeanLocalization instance, or creates one first.</summary>
		public static LeanToken AddTokenToFirst(string name)
		{
			if (Instances.Count == 0)
			{
				new GameObject("LeanLocalization").AddComponent<LeanLocalization>();
			}

			return Instances[0].AddToken(name);
		}

		/// <summary>This creates a new token with the specified name, and adds it to the current GameObject.</summary>
		public LeanToken AddToken(string name)
		{
			if (string.IsNullOrEmpty(name) == false)
			{
				var root  = new GameObject(name);
				var token = root.AddComponent<LeanToken>();

				root.transform.SetParent(transform, false);

				return token;
			}

			return null;
		}

		/// <summary>This allows you to set the value of the token with the specified name.
		/// If no token exists and allowCreation is enabled, then one will be created for you.</summary>
		public static void SetToken(string name, string value, bool allowCreation = true)
		{
			if (string.IsNullOrEmpty(name) == false)
			{
				var token = default(LeanToken);

				if (CurrentTokens.TryGetValue(name, out token) == true)
				{
					token.Value = value;
				}
				else if (allowCreation == true)
				{
					token = AddTokenToFirst(name);

					token.Value = value;
				}
			}
		}

		/// <summary>This allows you to get the value of the token with the specified name.
		/// If no token exists, then the defaultValue will be returned.</summary>
		public static string GetToken(string name, string defaultValue = null)
		{
			var token = default(LeanToken);

			if (string.IsNullOrEmpty(name) == false)
			{
				if (CurrentTokens.TryGetValue(name, out token) == true)
				{
					return token.Value;
				}
			}

			return defaultValue;
		}

		/// <summary>This calls AddPhrase on the first active and enabled LeanLocalization instance, or creates one first.</summary>
		public static LeanPhrase AddPhraseToFirst(string name)
		{
			if (Instances.Count == 0)
			{
				new GameObject("LeanLocalization").AddComponent<LeanLocalization>();
			}

			return Instances[0].AddPhrase(name);
		}

		/// <summary>This creates a new phrase with the specified name, and adds it to the current GameObject.</summary>
		public LeanPhrase AddPhrase(string name)
		{
			if (string.IsNullOrEmpty(name) == false)
			{
				var root   = new GameObject(name);
				var phrase = root.AddComponent<LeanPhrase>();

				root.transform.SetParent(transform, false);

				return phrase;
			}

			return null;
		}

		/// <summary>This will return the translation with the specified name, or null if none was found.</summary>
		public static LeanTranslation GetTranslation(string name)
		{
			var translation = default(LeanTranslation);

			if (string.IsNullOrEmpty(name) == false)
			{
				CurrentTranslations.TryGetValue(name, out translation);
			}

			return translation;
		}

		/// <summary>This will return the translated string with the specified name, or the fallback if none is found.</summary>
		public static string GetTranslationText(string name, string fallback = null, bool replaceTokens = true)
		{
			var translation = default(LeanTranslation);

			if (string.IsNullOrEmpty(name) == false && CurrentTranslations.TryGetValue(name, out translation) == true && translation.Data is string)
			{
				fallback = (string)translation.Data;
			}

			if (replaceTokens == true)
			{
				fallback = LeanTranslation.FormatText(fallback);
			}

			return fallback;
		}

		/// <summary>This will return the translated UnityEngine.Object with the specified name, or the fallback if none is found.</summary>
		public static T GetTranslationObject<T>(string name, T fallback = null)
			where T : Object
		{
			var translation = default(LeanTranslation);

			if (string.IsNullOrEmpty(name) == false && CurrentTranslations.TryGetValue(name, out translation) == true && translation.Data is T)
			{
				return (T)translation.Data;
			}

			return fallback;
		}

		/// <summary>This rebuilds the dictionary used to quickly map phrase names to translations for the current language.</summary>
		public static void UpdateTranslations(bool forceUpdate = true)
		{
			if (pendingUpdates == true || forceUpdate == true)
			{
				pendingUpdates = false;

				// Copy previous translations to temp dictionary
				tempTranslations.Clear();

				foreach (var pair in CurrentTranslations)
				{
					var translation = pair.Value;

					translation.Clear();

					tempTranslations.Add(pair.Key, translation);
				}

				// Clear currents
				CurrentTokens.Clear();
				CurrentLanguages.Clear();
				CurrentTranslations.Clear();

				// Rebuild all currents
				for (var i = 0; i < Instances.Count; i++)
				{
					Instances[i].RegisterAndBuild();
				}

				// Notify changes?
				if (OnLocalizationChanged != null)
				{
					OnLocalizationChanged();
				}
			}
		}

		/// <summary>If you call this method, then UpdateTranslations will be called next Update.</summary>
		public static void DelayUpdateTranslations()
		{
			pendingUpdates = true;

#if UNITY_EDITOR
			// Go through all enabled phrases
			for (var i = 0; i < Instances.Count; i++)
			{
				EditorUtility.SetDirty(Instances[i].gameObject);
			}
#endif
		}

		/// <summary>Set the instance, merge old instance, and update translations.</summary>
		protected virtual void OnEnable()
		{
			Instances.Add(this);

			UpdateCurrentLanguage();

			UpdateTranslations();
		}

		/// <summary>Unset instance?</summary>
		protected virtual void OnDisable()
		{
			Instances.Remove(this);

			UpdateTranslations();
		}

		protected virtual void Update()
		{
			UpdateTranslations(false);
		}
#if UNITY_EDITOR
		// Inspector modified?
		protected virtual void OnValidate()
		{
			UpdateTranslations();
		}
#endif
		private void RegisterAndBuild()
		{
			if (languages != null)
			{
				for (var i = 0; i < languages.Count; i++)
				{
					var language = languages[i];

					if (language != null && string.IsNullOrEmpty(language.Name) == false)
					{
						if (CurrentLanguages.ContainsKey(language.Name) == false)
						{
							CurrentLanguages.Add(language.Name, language);
						}
					}
				}
			}

			if (prefabs != null)
			{
				for (var i = 0; i < prefabs.Count; i++)
				{
					var sources = prefabs[i].Sources;

					for (var j = 0; j < sources.Count; j++)
					{
						sources[j].Compile(currentLanguage, DefaultLanguage);
					}
				}
			}

			var source = LeanSource.Instances.First;

			for (var i = LeanSource.Instances.Count - 1; i >= 0; i--)
			{
				source.Value.Compile(currentLanguage, DefaultLanguage);

				source = source.Next;
			}
		}

		private void UpdateCurrentLanguage()
		{
			// Load saved language?
			if (string.IsNullOrEmpty(currentLanguage) == true)
			{
				if (SaveLanguage == true)
				{
					LoadNow();
				}
			}

			// Find language by culture?
			if (string.IsNullOrEmpty(currentLanguage) == true)
			{
				switch (DetectLanguage)
				{
					case DetectType.SystemLanguage:
					{
						currentLanguage = FindLanguageName(Application.systemLanguage.ToString());
					}
					break;

					case DetectType.CurrentCulture:
					{
						var cultureInfo = System.Globalization.CultureInfo.CurrentCulture;

						if (cultureInfo != null)
						{
							currentLanguage = FindLanguageName(cultureInfo.Name);
						}
					}
					break;

					case DetectType.CurrentUICulture:
					{
						var cultureInfo = System.Globalization.CultureInfo.CurrentUICulture;

						if (cultureInfo != null)
						{
							currentLanguage = FindLanguageName(cultureInfo.Name);
						}
					}
					break;
				}
			}

			// Use default language?
			if (string.IsNullOrEmpty(currentLanguage) == true)
			{
				currentLanguage = DefaultLanguage;
			}

			// Attempt to set the new language
			if (SaveLanguage == true)
			{
				SaveNow();
			}
		}

		private string FindLanguageName(string alias)
		{
			for (var i = Languages.Count - 1; i >= 0; i--)
			{
				var language = Languages[i];

				if (language.Name == alias)
				{
					return language.Name;
				}

				if (language.Cultures != null)
				{
					for (var j = language.Cultures.Count - 1; j >= 0; j--)
					{
						if (language.Cultures[j] == alias)
						{
							return language.Name;
						}
					}
				}
			}

			return null;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Localization
{
	[CustomEditor(typeof(LeanLocalization))]
	public class LeanLocalization_Inspector : LeanInspector<LeanLocalization>
	{
		static LeanLocalization_Inspector()
		{
			AddPresetLanguage("Chinese", "ChineseSimplified", "ChineseTraditional", "zh", "zh-TW", "zh-CN", "zh-HK", "zh-SG", "zh-MO");
			AddPresetLanguage("English", "en", "en-GB", "en-US", "en-AU", "en-CA", "en-NZ", "en-IE", "en-ZA", "en-JM", "en-en029", "en-BZ", "en-BZ", "en-TT", "en-ZW", "en-PH");
			AddPresetLanguage("Spanish", "es", "es-ES", "es-MX", "es-GT", "es-CR", "es-PA", "es-DO", "es-VE", "es-CO", "es-PE", "es-AR", "es-EC", "es-CL", "es-UY", "es-PY", "es-BO", "es-SV", "es-SV", "es-HN", "es-NI", "es-PR");
			AddPresetLanguage("Arabic", "ar", "ar-SA", "ar-IQ", "ar-EG", "ar-LY", "ar-DZ", "ar-MA", "ar-TN", "ar-OM", "ar-YE", "ar-SY", "ar-JO", "ar-LB", "ar-KW", "ar-AE", "ar-BH", "ar-QA");
			AddPresetLanguage("German", "de", "de-DE", "de-CH", "de-AT", "de-LU", "de-LI");
			AddPresetLanguage("Korean", "ko", "ko-KR");
			AddPresetLanguage("French", "fr", "fr-FR", "fr-BE", "fr-CA", "fr-CH", "fr-LU", "fr-MC");
			AddPresetLanguage("Russian", "ru", "ru-RU");
			AddPresetLanguage("Japanese", "ja", "ja-JP");
			AddPresetLanguage("Italian", "it", "it-IT", "it-CH");
			AddPresetLanguage("Portuguese", "pt", "pt-BR", "pt-PT");
			AddPresetLanguage("Other...");
		}

		class PresetLanguage
		{
			public string   Name;
			public string[] Cultures;
		}

		private static List<PresetLanguage> presetLanguages = new List<PresetLanguage>();

		protected override void DrawInspector()
		{
			LeanLocalization.UpdateTranslations();
			
			DrawCurrentLanguage();
			Draw("SaveLanguage", "Automatically save/load the CurrentLanguage selection to PlayerPrefs? (can be cleared with ClearSave context menu option)");

			EditorGUILayout.Separator();

			Draw("DefaultLanguage", "If the application is started and no language has been loaded or auto detected, this language will be used.");
			Draw("DetectLanguage", "How should the cultures be used to detect the user's device language?");
			EditorGUI.BeginDisabledGroup(true);
				EditorGUI.indentLevel++;
					switch (Target.DetectLanguage)
					{
						case LeanLocalization.DetectType.SystemLanguage:
							EditorGUILayout.TextField("SystemLanguage", Application.systemLanguage.ToString());
						break;
						case LeanLocalization.DetectType.CurrentCulture:
							EditorGUILayout.TextField("CurrentCulture", System.Globalization.CultureInfo.CurrentCulture.ToString());
						break;
						case LeanLocalization.DetectType.CurrentUICulture:
							EditorGUILayout.TextField("CurrentUICulture", System.Globalization.CultureInfo.CurrentUICulture.ToString());
						break;
					}
				EditorGUI.indentLevel--;
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Separator();

			DrawLanguages();

			EditorGUILayout.Separator();

			DrawPrefabs();

			EditorGUILayout.Separator();

			DrawTokens();

			EditorGUILayout.Separator();

			DrawTranslations();
		}

		private void DrawCurrentLanguage()
		{
			var rect  = Reserve();
			var rectA = rect; rectA.xMax -= 37.0f;
			var rectB = rect; rectB.xMin = rectB.xMax - 35.0f;

			LeanLocalization.CurrentLanguage = EditorGUI.TextField(rectA, "Current Language", LeanLocalization.CurrentLanguage);

			if (GUI.Button(rectB, "List") == true)
			{
				var menu = new GenericMenu();

				foreach (var pair in LeanLocalization.CurrentLanguages)
				{
					var languageName = pair.Key;

					menu.AddItem(new GUIContent(languageName), LeanLocalization.CurrentLanguage == languageName, () => { LeanLocalization.CurrentLanguage = languageName; });
				}

				if (menu.GetItemCount() > 0)
				{
					menu.DropDown(rectB);
				}
				else
				{
					Debug.LogWarning("Your scene doesn't contain any languages, so the language name list couldn't be created.");
				}
			}
		}

		private void DrawLanguages()
		{
			var languagesProperty = serializedObject.FindProperty("languages");

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);
				if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(35.0f)) == true)
				{
					var menu = new GenericMenu();

					foreach (var presetLanguage in presetLanguages)
					{
						var preset = presetLanguage; menu.AddItem(new GUIContent(presetLanguage.Name), Target.LanguageExists(presetLanguage.Name), () => AddLanguage(preset));
					}

					menu.ShowAsContext();
				}
			EditorGUILayout.EndHorizontal();

			if (languagesProperty.arraySize == 0)
			{
				EditorGUILayout.HelpBox("Click the 'Add' button, and select a language.", MessageType.Info);
			}

			EditorGUI.indentLevel++;
				for (var i = 0; i < languagesProperty.arraySize; i++)
				{
					EditorGUILayout.PropertyField(languagesProperty.GetArrayElementAtIndex(i), true);
				}
			EditorGUI.indentLevel--;
		}

		private void DrawPrefabs()
		{
			var rectA = Reserve();
			var rectB = rectA; rectB.xMin += EditorGUIUtility.labelWidth;
			EditorGUI.LabelField(rectA, "Prefabs", EditorStyles.boldLabel);
			var newPrefab = EditorGUI.ObjectField(rectB, "", default(Object), typeof(Object), false);
			if (newPrefab != null)
			{
				Undo.RecordObject(Target, "Add Source");

				Target.AddPrefab(newPrefab);

				Dirty();
			}

			EditorGUI.indentLevel++;
				for (var i = 0; i < Target.Prefabs.Count; i++)
				{
					DrawPrefabs(i);
				}
			EditorGUI.indentLevel--;
		}

		private int expandPrefab = -1;

		private void DrawPrefabs(int index)
		{
			var rectA   = Reserve();
			var rectB   = rectA; rectB.xMax -= 22.0f;
			var rectC   = rectA; rectC.xMin = rectC.xMax - 20.0f;
			var prefab  = Target.Prefabs[index];
			var rebuilt = false;
			var expand  = EditorGUI.Foldout(new Rect(rectA.x, rectA.y, 20, rectA.height), expandPrefab == index, "");

			if (expand == true)
			{
				expandPrefab = index;
			}
			else if (expandPrefab == index)
			{
				expandPrefab = -1;
			}

			EditorGUI.BeginDisabledGroup(true);
				BeginError(prefab.Root == null);
					EditorGUI.ObjectField(rectB, prefab.Root, typeof(Object), false);
				EndError();
				if (prefab.Root != null)
				{
					Undo.RecordObject(Target, "Rebuild Sources");

					rebuilt |= prefab.RebuildSources();

					if (expand == true)
					{
						var sources = prefab.Sources;

						EditorGUI.indentLevel++;
							foreach (var source in sources)
							{
								EditorGUI.ObjectField(Reserve(), source, typeof(LeanSource), false);
							}
						EditorGUI.indentLevel--;
					}
				}
			EditorGUI.EndDisabledGroup();
			if (rebuilt == true)
			{
				Dirty();
			}
			if (GUI.Button(rectC, "X", EditorStyles.miniButton) == true)
			{
				Undo.RecordObject(Target, "Remove Prefab");

				Target.Prefabs.RemoveAt(index);

				Dirty();

				if (expand == true)
				{
					expandPrefab = -1;
				}
			}
		}

		private static string translationFilter;

		private LeanTranslation expandTranslation;

		private void DrawTranslations()
		{
			var rectA = Reserve();
			var rectB = rectA; rectB.xMin += EditorGUIUtility.labelWidth; rectB.xMax -= 37.0f;
			var rectC = rectA; rectC.xMin = rectC.xMax - 35.0f;
			EditorGUI.LabelField(rectA, "Translations", EditorStyles.boldLabel);
			translationFilter = EditorGUI.TextField(rectB, "", translationFilter);
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(translationFilter) == true || LeanLocalization.CurrentTranslations.ContainsKey(translationFilter) == true);
				if (GUI.Button(rectC, "Add", EditorStyles.miniButton) == true)
				{
					var phrase = LeanLocalization.AddPhraseToFirst(translationFilter);

					LeanLocalization.UpdateTranslations();

					Selection.activeObject = phrase;

					EditorGUIUtility.PingObject(phrase);
				}
			EditorGUI.EndDisabledGroup();

			if (LeanLocalization.CurrentTranslations.Count == 0 && string.IsNullOrEmpty(translationFilter) == true)
			{
				EditorGUILayout.HelpBox("Type in the name of a translation, and click the 'Add' button. Or, drag and drop a prefab that contains some.", MessageType.Info);
			}
			else
			{
				var total = 0;

				EditorGUI.indentLevel++;
					foreach (var pair in LeanLocalization.CurrentTranslations)
					{
						var name = pair.Key;

						if (string.IsNullOrEmpty(translationFilter) == true || name.IndexOf(translationFilter, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
						{
							var translation = pair.Value;
							var rectT       = Reserve();
							var expand      = EditorGUI.Foldout(new Rect(rectT.x, rectT.y, 20, rectT.height), expandTranslation == translation, "");

							if (expand == true)
							{
								expandTranslation = translation;
							}
							else if (expandTranslation == translation)
							{
								expandTranslation = null;
							}

							CalculateTranslation(pair.Value);

							var data = translation.Data;

							total++;

							EditorGUI.BeginDisabledGroup(true);
								BeginError(missing.Count > 0 || clashes.Count > 0);
									if (data is Object)
									{
										EditorGUI.ObjectField(rectT, name, (Object)data, typeof(Object), true);
									}
									else
									{
										EditorGUI.TextField(rectT, name, data != null ? data.ToString() : "");
									}
								EndError();

								if (expand == true)
								{
									EditorGUI.indentLevel++;
										foreach (var entry in translation.Entries)
										{
											BeginError(clashes.Contains(entry.Language) == true);
												EditorGUILayout.ObjectField(entry.Language, entry.Owner, typeof(Object), true);
											EndError();
										}
									EditorGUI.indentLevel--;
								}
							EditorGUI.EndDisabledGroup();

							if (expand == true)
							{
								foreach (var language in missing)
								{
									EditorGUILayout.HelpBox("This translation isn't defined for the " + language + " language.", MessageType.Warning);
								}

								foreach (var language in clashes)
								{
									EditorGUILayout.HelpBox("This translation is defined multiple times for the " + language + " language.", MessageType.Warning);
								}
							}
						}
					}
				EditorGUI.indentLevel--;

				if (total == 0)
				{
					EditorGUILayout.HelpBox("No translation with this name exists, click the 'Add' button to create it.", MessageType.Info);
				}
			}
		}
		
		private static List<string> missing = new List<string>();

		private static List<string> clashes = new List<string>();

		private static void CalculateTranslation(LeanTranslation translation)
		{
			missing.Clear();
			clashes.Clear();

			foreach (var language in LeanLocalization.CurrentLanguages.Keys)
			{
				if (translation.Entries.Exists(e => e.Language == language) == false)
				{
					missing.Add(language);
				}
			}

			foreach (var entry in translation.Entries)
			{
				var language = entry.Language;

				if (clashes.Contains(language) == false)
				{
					if (translation.LanguageCount(language) > 1)
					{
						clashes.Add(language);
					}
				}
			}
		}

		private static string tokensFilter;

		private void DrawTokens()
		{
			var rectA = Reserve();
			var rectB = rectA; rectB.xMin += EditorGUIUtility.labelWidth; rectB.xMax -= 37.0f;
			var rectC = rectA; rectC.xMin = rectC.xMax - 35.0f;
			EditorGUI.LabelField(rectA, "Tokens", EditorStyles.boldLabel);
			tokensFilter = EditorGUI.TextField(rectB, "", tokensFilter);
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(tokensFilter) == true || LeanLocalization.CurrentTokens.ContainsKey(tokensFilter) == true);
				if (GUI.Button(rectC, "Add", EditorStyles.miniButton) == true)
				{
					var token = LeanLocalization.AddTokenToFirst(tokensFilter);

					LeanLocalization.UpdateTranslations();

					Selection.activeObject = token;

					EditorGUIUtility.PingObject(token);
				}
			EditorGUI.EndDisabledGroup();

			if (LeanLocalization.CurrentTokens.Count > 0 || string.IsNullOrEmpty(tokensFilter) == false)
			{
				var total = 0;

				EditorGUI.indentLevel++;
					EditorGUI.BeginDisabledGroup(true);
						foreach (var pair in LeanLocalization.CurrentTokens)
						{
							if (string.IsNullOrEmpty(tokensFilter) == true || pair.Key.IndexOf(tokensFilter, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
							{
								EditorGUILayout.ObjectField(pair.Key, pair.Value, typeof(Object), true); total++;
							}
						}
					EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel--;

				if (total == 0)
				{
					EditorGUILayout.HelpBox("No token with this name exists, click the 'Add' button to create it.", MessageType.Info);
				}
			}
		}

		private void AddLanguage(PresetLanguage presetLanguage)
		{
			Undo.RecordObject(Target, "Add Language");

			Target.AddLanguage(presetLanguage.Name, presetLanguage.Cultures);

			Dirty();
		}

		private static void AddPresetLanguage(string name, params string[] cultures)
		{
			var presetLanguage = new PresetLanguage();

			presetLanguage.Name     = name;
			presetLanguage.Cultures = cultures;

			presetLanguages.Add(presetLanguage);
		}

		[MenuItem("GameObject/Lean/Localization", false, 1)]
		private static void CreateLocalization()
		{
			var gameObject = new GameObject(typeof(LeanLocalization).Name);

			Undo.RegisterCreatedObjectUndo(gameObject, "Create LeanLocalization");

			gameObject.AddComponent<LeanLocalization>();

			Selection.activeGameObject = gameObject;
		}
	}
}
#endif