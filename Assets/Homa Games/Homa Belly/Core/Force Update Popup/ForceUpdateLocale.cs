using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    internal class ForceUpdateLocale
    {
        private ForceUpdateLocale()
        { }

        public string PopupTitle;
        public string PopupText;

        public string PopupOkayButton;
        public string PopupDismissButton;

        public static ForceUpdateLocale GetLocaleFor(SystemLanguage language)
        {
            if (LanguageToLocale.TryGetValue(language, out var output))
                return output;

            return DefaultLocale;
        }

        private static readonly ForceUpdateLocale DefaultLocale = new ForceUpdateLocale
        {
            PopupTitle = "New Version",
            PopupText = "New version is available on the store",
            PopupOkayButton = "Update",
            PopupDismissButton = "Not now",
        };

        private static readonly Dictionary<SystemLanguage, ForceUpdateLocale> LanguageToLocale 
            = new Dictionary<SystemLanguage, ForceUpdateLocale>
        {
            [SystemLanguage.English] = DefaultLocale,
            [SystemLanguage.German] = new ForceUpdateLocale
            {
                PopupTitle = "Neue Version",
                PopupText = "Eine neue version ist im Store verfügbar",
                PopupOkayButton = "ktualisieren",
                PopupDismissButton = "Nicht jetzt",
            },
            [SystemLanguage.Spanish] = new ForceUpdateLocale
            {
                PopupTitle = "Nueva versión",
                PopupText = "Una nueva versión está disponible en la tienda",
                PopupOkayButton = "Actualizar",
                PopupDismissButton = "no ahora",
            },
            [SystemLanguage.French] = new ForceUpdateLocale
            {
                PopupTitle = "Nouvelle Version",
                PopupText = "Une nouvelle version est disponible",
                PopupOkayButton = "Mettre à jour",
                PopupDismissButton = "Plus tard",
            },
            [SystemLanguage.Italian] = new ForceUpdateLocale
            {
                PopupTitle = "Nuova versione",
                PopupText = "Una nuova versione è disponibile sullo store",
                PopupOkayButton = "Aggiornare",
                PopupDismissButton = "Non adesso",
            },
        };
    }
}