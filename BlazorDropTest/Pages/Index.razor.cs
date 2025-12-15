using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDropTest.Pages
{
	public partial class Index
	{

		private List<SelectItem> _items = new();

		private SelectItem? _singleSelected;
		private SelectItem? _nullSelected;
		private SelectItem? _disabledSelected;
		private SelectItem? _templatedSelected;

		private IEnumerable<SelectItem> _multiSelected = new List<SelectItem>();

		protected override void OnInitialized()
		{
			for (int i = 1; i <= 100; i++)
			{
				_items.Add(new SelectItem(Guid.NewGuid(), $"Item {i}"));
			}

			_singleSelected = _items.First();
			_nullSelected = null;
			_disabledSelected = null;
			_templatedSelected = null;
		}

		private Task<IEnumerable<SelectItem>> LoadItemsPagedAsync(int page, int pageSize)
			=> Task.FromResult(_items.Skip(page * pageSize).Take(pageSize));

		private Task<IEnumerable<SelectItem>> LoadEmptyAsync(int _, int __)
			=> Task.FromResult(Enumerable.Empty<SelectItem>());

		private Task<IEnumerable<SelectItem>> SearchAsync(string text)
			=> Task.FromResult(
				_items.Where(x =>
					x.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
			);

		private Task<SelectItem> OnSingleSelected(SelectItem item)
		{
			_singleSelected = item;
			return Task.FromResult(item);
		}

		private Task<SelectItem> OnNullSelected(SelectItem item)
		{
			_nullSelected = item;
			return Task.FromResult(item);
		}

		private Task<SelectItem> OnTemplateSelected(SelectItem item)
		{
			_templatedSelected = item;
			return Task.FromResult(item);
		}

		private Task<SelectItem> OnMultiSelected(SelectItem item)
		{
			return Task.FromResult(item);
		}

	}

	public sealed record SelectItem(
	Guid Id,
	string Text
);
}
