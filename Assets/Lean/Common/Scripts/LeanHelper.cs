using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Common
{
	/// <summary>This class contains useful methods used in almost all <b>LeanTouch</b> code.</summary>
	public static class LeanHelper
	{
		public const string HelpUrlPrefix = "http://carloswilkes.github.io/Documentation/";

		public const string ComponentPathPrefix = "Lean/";
#if UNITY_EDITOR
		/// <summary>This method creates an empty GameObject prefab at the current asset folder</summary>
		public static GameObject CreateAsset(string name)
		{
			var gameObject = new GameObject(name);
			var path       = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (string.IsNullOrEmpty(path) == true)
			{
				path = "Assets";
			}

			path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".prefab");
#if UNITY_2018_3_OR_NEWER
			var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);
#else
			var prefab = PrefabUtility.CreatePrefab(path, gameObject);
#endif
			Object.DestroyImmediate(gameObject);

			Selection.activeObject = prefab;

			return prefab;
		}
#endif
		/// <summary>This method allows you to create a UI element with the specified component and specified parent, with behaviour consistent with Unity's built-in UI element creation.</summary>
		public static T CreateElement<T>(Transform parent)
			where T : Component
		{
			var gameObject = new GameObject(typeof(T).Name);
#if UNITY_EDITOR
			Undo.RegisterCreatedObjectUndo(gameObject, "Create " + typeof(T).Name);
#endif
			var component = gameObject.AddComponent<T>();

			// Auto attach to canvas?
			if (parent == null || parent.GetComponentInParent<Canvas>() == null)
			{
				var canvas = Object.FindObjectOfType<Canvas>();

				if (canvas == null)
				{
					canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();

					canvas.gameObject.layer = LayerMask.NameToLayer("UI");

					canvas.renderMode = RenderMode.ScreenSpaceOverlay;

					// Make event system?
					if (EventSystem.current == null)
					{
						new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
					}
				}

				parent = canvas.transform;
			}

			gameObject.layer = parent.gameObject.layer;

			component.transform.SetParent(parent, false);

			return component;
		}

		/// <summary>This method gives you the time-independent 't' value for lerp when used for dampening. This returns 1 in edit mode, or if dampening is less than 0.</summary>
		public static float DampenFactor(float dampening, float elapsed)
		{
			if (dampening < 0.0f)
			{
				return 1.0f;
			}
#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return 1.0f;
			}
#endif
			return 1.0f - Mathf.Exp(-dampening * elapsed);
		}

		/// <summary>This method allows you to destroy the target object in game and in edit mode, and it returns null.</summary>
		public static T Destroy<T>(T o)
			where T : Object
		{
			if (o != null)
			{
#if UNITY_EDITOR
				if (Application.isPlaying == true)
				{
					Object.Destroy(o);
				}
				else
				{
					Object.DestroyImmediate(o);
				}
#else
				Object.Destroy(o);
#endif
			}

			return null;
		}
#if UNITY_EDITOR
		/// <summary>This method gives you the actual object behind a SerializedProperty given to you by a property drawer.</summary>
		public static T GetObjectFromSerializedProperty<T>(object target, SerializedProperty property)
		{
			var tokens = property.propertyPath.Replace(".Array.data[", ".[").Split('.');

			for (var i = 0; i < tokens.Length; i++)
			{
				var token = tokens[i];
				var type  = target.GetType();

				if (target is IList)
				{
					var list  = (IList)target;
					var index = int.Parse(token.Substring(1, token.Length - 2));

					target = list[index];
				}
				else
				{
					while (type != null)
					{
						var field = type.GetField(token, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

						if (field != null)
						{
							target = field.GetValue(target);

							break;
						}

						type = type.BaseType;
					}
				}
			}

			return (T)target;
		}
#endif
	}
}