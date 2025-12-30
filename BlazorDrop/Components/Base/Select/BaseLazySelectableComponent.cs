using BlazorDrop.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
	public abstract class BaseLazySelectableComponent<ListItemType, R> : BaseLazyComponent, IBlazorDropInvokable
		where R : class, IBlazorDropInvokable
	{
		[Parameter]
		public int PageSize { get; set; } = 20;

		[Parameter]
		public int CurrentPage { get; set; } = 0;

		[Parameter]
		public string ValueNotFoundMessageText { get; set; }

		[Parameter]
		public bool ShowLoadingIndicator { get; set; } = true;

		/// <summary>
		/// pageNumber, pageSize
		/// </summary>
		[Parameter]
		public Func<int, int, Task<IEnumerable<ListItemType>>> OnLoadItemsAsync { get; set; }

		[Parameter]
		public Func<ListItemType, string> DisplaySelector { get; set; }

		[Parameter]
		public IEnumerable<ListItemType> Items { get; set; } = new List<ListItemType>();

		[Parameter]
		public RenderFragment<ListItemType> ItemTemplate { get; set; }

		[Parameter]
		public Func<ListItemType, Task<ListItemType>> OnItemClickAsync { get; set; }

		[Inject]
		protected IBlazorDropInteropService InteropService { get; set; }

		protected DotNetObjectReference<IBlazorDropInvokable> DotNetRef { get; private set; }

		protected const string DefaultSelectableItemClass = "bzd-item";

		protected bool _isLoading;
		protected bool _hasLoadedAllItems;
		protected bool _isScrollHandlerAttached;
		protected bool _dotNetRefCreated;

		[JSInvokable]
		public async Task OnScrollToEndAsync()
		{
			if (_isLoading || _hasLoadedAllItems)
			{
				return;
			}

			await LoadNextPageAsync();
			await InvokeAsync(StateHasChanged);
		}

		protected async Task LoadNextPageAsync()
		{
			var nextPage = CurrentPage + 1;
			await LoadPageAsync(nextPage);
			CurrentPage = nextPage;
		}

		protected virtual async Task LoadPageAsync(int pageNumber, bool ignoreLoadingState = false)
		{
			if (OnLoadItemsAsync == null ||
				_hasLoadedAllItems ||
				(_isLoading && !ignoreLoadingState))
			{
				return;
			}

			await SetLoadingStateAsync(true);

			var newItems = (await OnLoadItemsAsync(pageNumber, PageSize))?.ToList()
						   ?? new List<ListItemType>();

			Items = Items.Concat(newItems);

			_hasLoadedAllItems = newItems.Count < PageSize;

			await SetLoadingStateAsync(false);
		}

		protected async Task SetLoadingStateAsync(bool isLoading)
		{
			_isLoading = isLoading;
			await InvokeAsync(StateHasChanged);
		}

		protected string GetDisplayValue(ListItemType item)
			=> DisplaySelector?.Invoke(item) ?? item?.ToString();

		protected string GetSelectableItemClass(ListItemType item)
			=> IsItemSelected(item)
				? $"{DefaultSelectableItemClass} bzd-item-selected"
				: DefaultSelectableItemClass;

		protected async Task RegisterScrollAsync(
			string containerId,
			DotNetObjectReference<IBlazorDropInvokable> dotNetRef)
		{
			if (_isScrollHandlerAttached)
				return;

			await InteropService.RegisterScrollAsync(
				containerId,
				nameof(OnScrollToEndAsync),
				dotNetRef);

			_isScrollHandlerAttached = true;
		}

		protected async Task UnregisterScrollAsync(string containerId)
		{
			if (!_isScrollHandlerAttached)
				return;

			await InteropService.UnregisterScrollAsync(containerId);
			_isScrollHandlerAttached = false;
		}

		protected void CreateDotNetRef()
		{
			if (_dotNetRefCreated)
			{
				return;
			}

			DotNetRef = DotNetObjectReference.Create<IBlazorDropInvokable>(this);
			_dotNetRefCreated = true;
		}

		protected abstract Task HandleItemSelectedAsync(ListItemType item);

		protected abstract bool IsItemSelected(ListItemType item);
	}
}
