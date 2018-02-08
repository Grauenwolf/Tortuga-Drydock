using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tortuga.Chain;
using Tortuga.Chain.Metadata;

namespace Tortuga.Drydock.Models
{
    public abstract class TableVM<TName, TDbType> : TableVM
        where TDbType : struct
    {
        public abstract string QuotedTableName { get; }

        public TableVM(IClass1DataSource dataSource, TableOrViewMetadata<TName, TDbType> table) : base(dataSource)
        {
            if (!table.IsTable)
                throw new ArgumentException($"{table.Name} is not a table.", nameof(table));
            Table = table;

            PopulateColumns();

            UpdateSuggestions();

            FixItOperations.Add(new FixDropTable<TName, TDbType>(this));

        }

        public ICommand AnalyzeColumnsCommand
        {
            get { return GetCommand(async () => await AnalyzeColumnsAsync()); }
        }

        public ColumnCollection<TDbType> Columns { get => GetNew<ColumnCollection<TDbType>>(); }
        public override string FullName => Table.Name.ToString();

        public bool? IsHeap
        {
            get => GetDefault<bool?>(null);
            set
            {
                if (Set(value))
                    UpdateSuggestions();
            }
        }



        public ICommand ShowTopTenCommand => GetCommand<ColumnModel<TDbType>>(async t => await ShowTopTenAsync(t));
        public bool SuggestPrimaryKeyButton { get => Get<bool>(); private set => Set(value); }

        public ICommand SuggestPrimaryKeyCommand => GetCommand(async () => await SuggestPrimaryKeyAsync());

        public TableOrViewMetadata<TName, TDbType> Table { get; }


        public virtual string WindowTitle => "Table: " + Table.Name.ToString();

        public async Task SuggestPrimaryKeyAsync()
        {
            StartWork();

            await AnalyzeColumnsAsync();

            foreach (var column in Columns.Where(c => c.IsUnique == true && (c.NullCount ?? 0) == 0))
                column.IsPrimaryKeyCandidate = true;

            SuggestPrimaryKeyButton = false;

            StopWork();

        }

        protected abstract Task AnalyzeColumnAsync(ColumnModel<TDbType> column);

        protected abstract Task<DataTable> OnShowTopTenAsync(ColumnModel<TDbType> column);

        protected virtual void PopulateColumns()
        {
            Columns.AddRange(Table.Columns.Select(c => new ColumnModel<TDbType>(c)));
        }



        async Task AnalyzeColumnsAsync()
        {
            var exceptions = new List<Exception>();

            try
            {
                StartWork();

                foreach (var column in Columns.Where(c => !c.StatsLoaded))
                    try
                    {
                        await AnalyzeColumnAsync(column);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }

                if (exceptions.Count > 0)
                    throw new AggregateException("Error analyzing one or more columns.", exceptions);
            }
            finally
            {
                StopWork();

            }
            FixItOperations.RefreshAll();

        }

        async Task ShowTopTenAsync(ColumnModel<TDbType> column)
        {
            //The database-specific OnShowTopTenAsync will eventually be replaced by Chain's Aggregate functionality
            //https://github.com/docevaad/Chain/milestone/9

            var model = new DataVM()
            {
                Data = await OnShowTopTenAsync(column),
                WindowTitle = $"Top 10 values for {Table.Name}.{column.Column.SqlName}"
            };
            RequestDialog(model);
        }

        void UpdateSuggestions()
        {
            SuggestPrimaryKeyButton = (IsHeap == true) && !Columns.Any(c => c.IsPrimaryKey || c.IsPrimaryKeyCandidate);
        }
    }
}


