using System;
using System.Collections.Generic;

using AruaRoseLoginManager.Data;

namespace AruaRoseLoginManager.Helpers
{
    public interface IPartyDisplay: ILoginDisplay<Party>
    {
        event EventHandler<EventArgs> NewRequest;

        void Prompt(IEnumerable<string> accounts);

        void Prompt(IEnumerable<string> accounts, Party info);

        void PromptWithAccounts(Dictionary<string, Account> accounts);

        void PromptWithAccounts(Dictionary<string, Account> accounts, WindowSize sizeDefaults);

        void PromptWithAccounts(Dictionary<string, Account> accounts, Party info);

        void PromptWithAccounts(Dictionary<string, Account> accounts, Party info, WindowSize sizeDefaults);

        void SetColumnWidths(double[] widths);

        double[] GetColumnWidths();
    }
}
