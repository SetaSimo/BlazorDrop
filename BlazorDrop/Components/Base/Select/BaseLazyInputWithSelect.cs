using BlazorDrop.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDrop.Components.Base.Select
{
	public abstract class BaseLazyInputWithSelect<T, R>
		: BaseLazySelectableComponent<T, R>
		where R : class, IBlazorDropInvokable
	{
		[Parameter]
		public string Placeholder { get; set; }

		[Parameter]
		public int UpdateSearchDelayInMilliseconds { get; set; } = 500;

		[Parameter]
		public bool Disabled { get; set; }

		[Parameter]
		public Func<string, Task<IEnumerable<T>>> SearchAsync { get; set; }

		[Inject]
		protected IBlazorDropInteropService InteropService { get; set; }

		protected string _searchText = string.Empty;

		protected bool _isDropdownOpen;

		protected readonly string _inputSelectorId = Guid.NewGuid().ToString();
		protected readonly string _scrollContainerId = Guid.NewGuid().ToString();

		[JSInvokable]
		public async Task OnClickOutsideAsync(string containerId)
		{
			_isDropdownOpen = false;
			_isScrollHandlerAttached = false;

			await UnregisterScrollAsync(containerId);
			await InteropService.UnregisterClickOutsideAsync(containerId);

			StateHasChanged();
		}

		[JSInvokable]
		public async Task UpdateSearchListAfterInputAsync(string containerId)
		{
			_hasLoadedAllItems = false;

			if (_isDropdownOpen is false)
			{
				await OpenDropdownAsync(containerId);
			}

			if (string.IsNullOrWhiteSpace(_searchText))
			{
				await ResetSearchAsync();
			}
			else
			{
				await SearchWithFilterAsync();
			}
		}

		protected async Task OpenDropdownAsync(string containerId)
		{
			if (Disabled || _isDropdownOpen)
			{
				return;
			}

			_isDropdownOpen = true;
			StateHasChanged();

			await InteropService.RegisterClickOutsideAsync(containerId, DotNetRef);
			await RegisterScrollAsync(containerId, DotNetRef);
		}

		private async Task ResetSearchAsync()
		{
			CurrentPage = 0;
			Items = new List<T>();

			await LoadPageAsync(CurrentPage, ignoreLoadingState: true);
			StateHasChanged();
		}

		private async Task SearchWithFilterAsync()
		{
			await UnregisterScrollAsync(_scrollContainerId);
			if (SearchAsync == null)
			{
				Items = Items.Where(x => GetDisplayValue(x).Contains(_searchText));
			}
			else
			{
				var items = await SearchAsync(_searchText);
				Items = items?.ToList() ?? new List<T>();
			}
		}

		protected async Task RegisterInputAsync(string clickOutsideContainerId)
		{
			if (Disabled)
			{
				return;
			}

			CreateDotNetRef();

			await InteropService.RegisterInputAsync(
				_inputSelectorId,
				UpdateSearchDelayInMilliseconds,
				clickOutsideContainerId,
				DotNetRef);
		}
	}
}
