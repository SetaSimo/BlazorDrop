using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDropTest.Pages
{
	public partial class Index
	{
		private List<KeyValuePair<Guid, string>> _testData = new List<KeyValuePair<Guid, string>>();
		private IEnumerable<KeyValuePair<Guid, string>> _multiSelected = new List<KeyValuePair<Guid, string>>();
		private KeyValuePair<Guid, string> _singleSelected;
		private KeyValuePair<Guid, string> _templatedSelect;
		private KeyValuePair<Guid, string> _listSelected;

		protected override void OnInitialized()
		{
			for (int i = 1; i <= 100; i++)
			{
				var key = Guid.NewGuid();
				_testData.Add(new KeyValuePair<Guid, string>(key, $"Item {i}"));
			}

			_singleSelected = _testData.FirstOrDefault();
			_templatedSelect = _testData.FirstOrDefault();
			_listSelected = _testData.FirstOrDefault();
		}

		private Task<IEnumerable<KeyValuePair<Guid, string>>> LoadItemsPagedAsync(int page, int pageSize)
		{
			var result = _testData.Skip(page * pageSize).Take(pageSize);
			return Task.FromResult(result);
		}

		private async Task<IEnumerable<KeyValuePair<Guid, string>>> LoadItemsPagedWithDelayAsync(int page, int pageSize)
		{
			await Task.Delay(600);
			return _testData.Skip(page * pageSize).Take(pageSize);
		}

		private Task<IEnumerable<KeyValuePair<Guid, string>>> SearchByTextAsync(string text)
		{
			var result = _testData.Where(x => x.Value.Contains(text, StringComparison.OrdinalIgnoreCase));
			return Task.FromResult(result);
		}

		private Task<KeyValuePair<Guid, string>> OnItemSelected(KeyValuePair<Guid, string> item)
		{
			_singleSelected = item;
			StateHasChanged();
			return Task.FromResult(item);
		}

		private Task<KeyValuePair<Guid, string>> OnTemplateItemSelected(KeyValuePair<Guid, string> item)
		{
			_templatedSelect = item;
			StateHasChanged();
			return Task.FromResult(item);
		}

		private Task<KeyValuePair<Guid, string>> OnListItemSelected(KeyValuePair<Guid, string> item)
		{
			_listSelected = item;
			StateHasChanged();
			return Task.FromResult(item);
		}

		private Task<KeyValuePair<Guid, string>> OnMultiItemSelected(KeyValuePair<Guid, string> item)
		{
			return Task.FromResult(item);
		}
	}
}
