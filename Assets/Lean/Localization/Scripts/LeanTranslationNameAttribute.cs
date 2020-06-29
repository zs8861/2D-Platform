using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Localization
{
	/// <summary>This attribute allows you to select a translation from all the localizations in the scene.</summary>
	public class LeanTranslationNameAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace Lean.Localization
{
	[CustomPropertyDrawer(typeof(LeanTranslationNameAttribute))]
	public class LeanTranslationNameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var left   = position; left.xMax -= 40;
			var right  = position; right.xMin = left.xMax + 2;
			var color  = GUI.color;
			var exists = LeanLocalization.CurrentTranslations.ContainsKey(property.stringValue);

			if (exists == false)
			{
				GUI.color = Color.red;
			}

			EditorGUI.PropertyField(left, property);

			GUI.color = color;

			if (GUI.Button(right, "List") == true)
			{
				var menu = new GenericMenu();

				if (string.IsNullOrEmpty(property.stringValue) == false)
				{
					if (exists == true)
					{
						var translation = default(LeanTranslation);

						if (LeanLocalization.CurrentTranslations.TryGetValue(property.stringValue, out translation) == true)
						{
							foreach (var entry in translation.Entries)
							{
								var owner = entry.Owner; menu.AddItem(new GUIContent("Select/" + entry.Language), false, () => { Selection.activeObject = owner; EditorGUIUtility.PingObject(owner); });
							}
						}
					}
					else
					{
						menu.AddItem(new GUIContent("Add: " + property.stringValue.Replace('/', '\\')), false, () => { var phrase = LeanLocalization.AddPhraseToFirst(property.stringValue); LeanLocalization.UpdateTranslations(); Selection.activeObject = phrase; EditorGUIUtility.PingObject(phrase); });
					}

					menu.AddItem(GUIContent.none, false, null);
				}

				foreach (var translationName in LeanLocalization.CurrentTranslations.Keys)
				{
					menu.AddItem(new GUIContent(translationName), property.stringValue == translationName, () => { property.stringValue = translationName; property.serializedObject.ApplyModifiedProperties(); });
				}

				if (menu.GetItemCount() > 0)
				{
					menu.DropDown(right);
				}
				else
				{
					Debug.LogWarning("Your scene doesn't contain any phrases, so the phrase name list couldn't be created.");
				}
			}
		}
	}
}
#endif