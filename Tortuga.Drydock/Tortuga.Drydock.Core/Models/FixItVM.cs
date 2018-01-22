using System;
using System.Windows;
using System.Windows.Input;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public class FixItVM : ViewModelBaseImproved
    {

        public string ChangeSql { get => Get<string>(); set => Set(value); }
        public ICommand CopyChangeSqlCommand => GetCommand(CopyChangeSql);
        public ICommand CopyRollBackSqlCommand => GetCommand(CopyRollBackSql);
        public ICommand CopyVerificationSqlCommand => GetCommand(CopyVerificationSql);
        public string RollBackSql { get => Get<string>(); set => Set(value); }
        public string VerificationSql { get => Get<string>(); set => Set(value); }
        public string WindowTitle { get => Get<string>(); set => Set(value); }
        void CopyChangeSql() => Clipboard.SetText(ChangeSql);

        void CopyRollBackSql() => Clipboard.SetText(RollBackSql);

        void CopyVerificationSql() => Clipboard.SetText(VerificationSql);
    }
}
