using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazorDrop.Components;
using BlazorDrop.Data;

namespace BlazorDropTest.Pages
{
    public partial class Index
    {
        private static readonly string[] Cities = { "Berlin", "Madrid", "Oslo", "Prague", "Riga", "Tallinn", "Vienna" };

        private readonly List<Customer> _customers = new();

        private Customer? _customer;
        private Customer? _listValue;
        private Customer? _templated;
        private IList<Customer> _multi = new List<Customer>();
        private BlazorDropList<Customer>? _list;
        private IBlazorDropDataSource<Customer> _queryableSource = default!;

        private bool _dark;
        private int _queryCount;
        private string? _lastSearch;
        private Exception? _lastError;

        private readonly DemoModel _model = new();
        private string? _submitResult;

        private readonly BlazorDropTexts _customTexts = new()
        {
            NoItems = "Nothing matched your search",
            LoadFailed = "Could not load the data.",
            Retry = "Try again",
            Loading = "Fetching...",
            ClearSelection = "Clear the selection",
            RemoveItemFormat = "Remove {0}",
            MoreSelectedFormat = "+{0} more",
            Done = "Apply",
        };

        protected override void OnInitialized()
        {
            for (var i = 1; i <= 500; i++)
            {
                _customers.Add(new Customer(i, $"Customer {i}", Cities[i % Cities.Length]));
            }

            _customer = _customers[0];

            _queryableSource = BlazorDropDataSource.FromQueryable<Customer>(q =>
            {
                var query = _customers.AsQueryable();
                if (q.HasSearch)
                {
                    query = query.Where(x => x.Name.Contains(q.SearchText, StringComparison.OrdinalIgnoreCase));
                }

                return query.OrderBy(x => x.Name);
            });
        }

        private async Task<IEnumerable<Customer>> QueryCustomersAsync(BlazorDropQuery query, CancellationToken cancellationToken)
        {
            _queryCount++;
            await Task.Delay(150, cancellationToken);
            return _customers
                .Where(x => !query.HasSearch || x.Name.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Id)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .ToList();
        }

        private IEnumerable<Customer> LoadCustomersPage(int page, int pageSize)
            => _customers.Skip(page * pageSize).Take(pageSize).ToList();

        private async Task AddCustomerAndReload()
        {
            var id = _customers.Count + 1;
            _customers.Insert(0, new Customer(id, $"New customer {id}", Cities[id % Cities.Length]));
            if (_list != null)
            {
                await _list.ReloadAsync();
            }
        }

        private Task<IEnumerable<Customer>> FailingLoadAsync(int page, int pageSize)
            => throw new InvalidOperationException("Simulated load failure");

        private void OnLoadError(Exception ex) => _lastError = ex;

        private void Submit() => _submitResult = $"Submitted: {_model.Customer?.Name ?? "—"} + {_model.Tags.Count} tag(s)";

        public sealed record Customer(int Id, string Name, string City);

        public sealed class DemoModel
        {
            [Required(ErrorMessage = "Please pick a customer")]
            public Customer? Customer { get; set; }

            [MinLength(1, ErrorMessage = "Pick at least one tag")]
            public IList<Customer> Tags { get; set; } = new List<Customer>();
        }
    }
}
