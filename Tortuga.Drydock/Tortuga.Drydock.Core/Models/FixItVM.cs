using System.Windows;
using System.Windows.Input;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public class FixItVM : ViewModelBase
    {
        public string ChangeSql { get => Get<string>(); set => Set(value); }
        public string CreateSql { get => Get<string>(); set => Set(value); }
        public ICommand CopyChangeSqlCommand => GetCommand(CopyChangeSql);
        public ICommand CopyRollBackSqlCommand => GetCommand(CopyRollBackSql);
        public ICommand CopyVerificationSqlCommand => GetCommand(CopyVerificationSql);
        public ICommand CopyCreateSqlCommand => GetCommand(CopyCreateSql);
        public string RollBackSql { get => Get<string>(); set => Set(value); }
        public string VerificationSql { get => Get<string>(); set => Set(value); }
        public string WindowTitle { get => Get<string>(); set => Set(value); }

        private void CopyChangeSql() => Clipboard.SetText(ChangeSql);

        private void CopyCreateSql() => Clipboard.SetText(CreateSql);

        private void CopyRollBackSql() => Clipboard.SetText(RollBackSql);

        private void CopyVerificationSql() => Clipboard.SetText(VerificationSql);
    }
}