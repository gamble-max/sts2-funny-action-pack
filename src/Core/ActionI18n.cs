using System;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2ActionPack.Core;

internal static class ActionI18n
{
    public static bool IsZh
    {
        get
        {
            var lang = LocManager.Instance?.Language;
            if (string.IsNullOrWhiteSpace(lang))
                return true;
            return lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static string T(string zh, string en) => IsZh ? zh : en;
}
