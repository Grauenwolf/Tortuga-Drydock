using System;
using System.Collections.Generic;
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


        public TableVM(IClass1DataSource dataSource, TableOrViewMetadata<TName, TDbType> table) : base(dataSource)
        {
            if (!table.IsTable)
                throw new ArgumentException($"{table.Name} is not a table.", nameof(table));
            Table = table;

            PopulateColumns();

            UpdateSuggestions();
        }

        public ICommand AnalyzeColumnsCommand
        {
            get { return GetCommand(async () => await AnalyzeColumnAsync()); }
        }

        public ColumnCollection<TDbType> Columns { get => GetNew<ColumnCollection<TDbType>>(); }
        public bool? IsHeap
        {
            get => GetDefault<bool?>(null);
            set
            {
                if (Set(value))
                    UpdateSuggestions();
            }
        }



        public bool SuggestPrimaryKeyButton { get => Get<bool>(); private set => Set(value); }

        public ICommand SuggestPrimaryKeyCommand => GetCommand(async () => await SuggestPrimaryKeyAsync());

        public TableOrViewMetadata<TName, TDbType> Table { get; }


        protected abstract Task AnalyzeColumnAsync(ColumnModel<TDbType> column);

        protected virtual void PopulateColumns()
        {
            Columns.AddRange(Table.Columns.Select(c => new ColumnModel<TDbType>(c)));
        }



        void UpdateSuggestions()
        {
            SuggestPrimaryKeyButton = (IsHeap == true) && !Columns.Any(c => c.IsPrimaryKey || c.IsPrimaryKeyCandidate);
        }

        public virtual string WindowTitle => "Table: " + Table.Name.ToString();

        async Task AnalyzeColumnAsync()
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

        }

        public async Task SuggestPrimaryKeyAsync()
        {
            StartWork();

            await AnalyzeColumnAsync();

            foreach (var column in Columns.Where(c => c.IsUnique == true && (c.NullCount ?? 0) == 0))
                column.IsPrimaryKeyCandidate = true;

            SuggestPrimaryKeyButton = false;

            StopWork();

        }

        public override string FullName => Table.Name.ToString();
    }
}


