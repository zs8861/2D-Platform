namespace Lean.Localization
{
	/// <summary>This interface can be implemented by any component that needs to listen for localization changes.</summary>
	public interface ILocalizationHandler
	{
		/// <summary>This method is called when initializing, or changing language.</summary>
		void UpdateLocalization();

		/// <summary>This method allows you to register the specified token.</summary>
		void Register(LeanToken token);

		/// <summary>This method allows you to unregister the specified token.</summary>
		void Unregister(LeanToken token);

		/// <summary>This method allows you to unregister all tokens.</summary>
		void UnregisterAll();
	}
}