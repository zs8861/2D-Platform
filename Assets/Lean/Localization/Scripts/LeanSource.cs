using UnityEngine;
using System.Collections.Generic;

namespace Lean.Localization
{
	/// <summary>This is the base class used for all translation sources. When a translation source is built, it will populate the <b>LeanLocalization</b> class with its translation data.</summary>
	public abstract class LeanSource : MonoBehaviour
	{
		public static LinkedList<LeanSource> Instances = new LinkedList<LeanSource>();

		[System.NonSerialized]
		private LinkedListNode<LeanSource> node;

		public abstract void Compile(string primaryLanguage, string secondaryLanguage);

		public void Register()
		{
			if (node == null)
			{
				node = Instances.AddLast(this);

				LeanLocalization.DelayUpdateTranslations();
			}
		}

		public void Unregister()
		{
			if (node != null)
			{
				Instances.Remove(node);

				node = null;

				LeanLocalization.DelayUpdateTranslations();
			}
		}

		protected virtual void OnEnable()
		{
			Register();
		}

		protected virtual void OnDisable()
		{
			Unregister();
		}
	}
}