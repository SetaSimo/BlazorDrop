using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorDrop.Components
{
	public partial class BlazorDropSelect<T> : IAsyncDisposable
	{
		[Parameter]
		public T Value { get; set; }

		[Parameter]
		public EventCallback<T> ValueChanged { get; set; }

		protected override async Task OnInitializedAsync()
		{
			CreateDotNetRef();
			await LoadPageAsync(CurrentPage);

			if (Value != null)
			{
				UpdateSearchTextAfterSelect(Value);
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender && Disabled is false)
			{
				await RegisterInputAsync(_inputSelectorId);
			}

			if (_isDropdownOpen && _isScrollHandlerAttached is false && Disabled is false)
			{
				await RegisterScrollAsync(_scrollContainerId, DotNetRef);
			}
		}

		protected override void OnParametersSet()
		{
			UpdateSearchTextAfterSelect(Value);
		}

		protected override async Task HandleItemSelectedAsync(T value)
		{
			await SetLoadingStateAsync(true);

			if (OnItemClickAsync == null)
			{
				Value = value;
			}
			else
			{
				Value = await OnItemClickAsync.Invoke(value);
			}

			UpdateSearchTextAfterSelect(Value);
			await ValueChanged.InvokeAsync(Value);

			await SetLoadingStateAsync(false);
		}

		private void UpdateSearchTextAfterSelect(T value)
		{
			if (value == null)
			{
				_searchText = string.Empty;
				return;
			}

			_searchText = GetDisplayValue(value);
		}

		protected override bool IsItemSelected(T item)
		{
			return EqualityComparer<T>.Default.Equals(item, Value);
		}

		public async ValueTask DisposeAsync()
		{
			if (DotNetRef != null)
			{
				await InteropService.UnregisterClickOutsideAsync(_inputSelectorId);
				await UnregisterScrollAsync(_scrollContainerId);

				DotNetRef.Dispose();
				_dotNetRefCreated = false;
			}
		}
	}
}