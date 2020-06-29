using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Localization
{
	/// <summary>This attribute allows you to modify a normal string field into one that has a dropdown list that allows you to pick a language.</summary>
	public class LeanLanguageNameAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace Lean.Localization
{
	[CustomPropertyDrawer(typeof(LeanLanguageNameAttribute))]
	public class LeanLanguageNameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var rectA = position; rectA.xMax -= 37.0f;
			var rectB = position; rectB.xMin = rectB.xMax - 35.0f;

			EditorGUI.PropertyField(rectA, property);

			if (GUI.Button(rectB, "List") == true)
			{
				var menu = new GenericMenu();

				foreach (var languageName in LeanLocalization.CurrentLanguages.Keys)
				{
					menu.AddItem(new GUIContent(languageName), property.stringValue == languageName, () => { property.stringValue = languageName; property.serializedObject.ApplyModifiedProperties(); });
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
	}
}
#endif