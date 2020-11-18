namespace Hearthstone_Collection_Tracker.Properties
{
    using HearthDb.Enums;
    using WPFLocalizeExtension.Engine;

    public class Strings
    {
        public static string Get(string key) => LocalizeDictionary.Instance.GetLocalizedObject(LibraryInfo.Name, "Resources", key, LocalizeDictionary.Instance.Culture)?.ToString();

        public static string GetCardSetName(CardSet cardSet) => Get(cardSet.ToString());
    }

}
