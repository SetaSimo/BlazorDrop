![.NET 6](https://img.shields.io/badge/.NET-6-blue)
![.NET 8](https://img.shields.io/badge/.NET-8-blueviolet)

# BlazorDrop

**BlazorDrop** is a lightweight Blazor component library that provides customizable dropdowns with async loading, search, and virtualization support.

## Features

- Asynchronous data loading by pages
- Search by input text with debounce
- Scroll-to-end detection for infinite loading
- Multi-select support with dynamic list rendering
- Two-way data binding support (@bind-Value)
- Custom display selector
- Loading indicator while fetching data
- Disabled state support
- Custom item templates
- Optional placeholder text
- Centralized JS Interop Service

## Installation

You can install the library via NuGet:

```bash
dotnet add package BlazorDrop
```

## Required Imports
Starting from this version, **BlazorDrop requires DI registration** for internal JS interop services.

Add the following line in `Program.cs`:

```csharp
using BlazorDrop.Extensions;

builder.Services.AddBlazorDrop();
```
To use templates, make sure you import the following namespaces in your `.razor` file or `_Imports.razor`:

```razor
@using BlazorDrop
@using BlazorDrop.Components
//if loading bars required
@using BlazorDrop.Components.Loading
```

Then reference the JavaScript and CSS in your host page:

- For **Blazor WebAssembly**: `wwwroot/index.html`
- For **Blazor Server**: `_Host.cshtml`

```html
<script src="_content/BlazorDrop/BlazorDropSelect.js"></script>
<link rel="stylesheet" href="_content/BlazorDrop/BlazorDropSelect.css" />
<link
  href="https://fonts.googleapis.com/css2?family=Roboto&display=swap"
  rel="stylesheet"
/>
```

## Usage

## BlazorDropSelect

![Select](https://github.com/user-attachments/assets/e55fec59-044d-4b8b-a4d0-643c7101606e)

```razor
<BlazorDropSelect
        T="SelectItem"
        DisplaySelector="@(x => x.Text)"
        OnLoadItemsAsync="LoadPageAsync"
        SearchAsync="SearchAsync"
        @bind-Value="@_selectedItem"
        ShowLoadingIndicator="true"
        Disabled="false"
        Placeholder="Select value"
        UpdateSearchDelayInMilliseconds="600"
        PageSize="10"
        ValueNotFoundMessageText="Not found" />
```

### Parameters

| Parameter                         | Type                                   | Description                         |
| --------------------------------- | -------------------------------------- | ----------------------------------- |
| T                                 | generic                                | Type of the items                   |
| Id                                | string                                 | Optional unique ID (auto-generated) |
| Class                             | string                                 | Optional CSS class                  |
| Style                             | string                                 | Optional inline CSS                 |
| Placeholder                       | string                                 | Placeholder text                    |
| ValueNotFoundMessageText          | string                                 | Message when no items found         |
| PageSize                          | int                                    | Items per page (default: 20)        |
| CurrentPage                       | int                                    | Initial page index                  |
| UpdateSearchDelayInMilliseconds   | int                                    | Search debounce delay (ms)          |
| Value                             | T                                      | Selected value                      |
| ValueChanged                      | EventCallback<T>                       | Triggered when value changes        |
| OnItemClickAsync                  | Func<T, Task<T>>                       | Optional custom selection logic     |
| ItemTemplate                      | RenderFragment<T>                      | Custom item template                |
| DisplaySelector                   | Func<T, string>                         | Display text selector               |
| OnLoadItemsAsync                  | Func<int, int, Task<IEnumerable<T>>>   | Paged data loader                   |
| SearchAsync                       | Func<string, Task<IEnumerable<T>>>     | Search handler                      |
| ShowLoadingIndicator              | bool                                   | Show loading spinner                |
| Disabled                          | bool                                   | Disable interaction                 |

### Example Code

```csharp
private Dictionary<Guid, string> _testValues = new();

private KeyValuePair<Guid, string> _selectedItem;

protected override async Task OnInitializedAsync()
{
    for (int i = 0; i < 1000; i++)
    {
        var id = Guid.NewGuid();
        _testValues.Add(id, $"Record {i}");
    }
}

private Task<IEnumerable<KeyValuePair<Guid, string>>> LoadPageAsync(int page, int pageSize)
{
    return Task.FromResult(_testValues.Skip(page * pageSize).Take(pageSize));
}

private Task<IEnumerable<KeyValuePair<Guid, string>>> SearchForItemAsync(string text)
{
    return Task.FromResult(_testValues.Where(x => x.Value.Contains(text)));
}

private Task<KeyValuePair<Guid, string>> OnValueChanged(KeyValuePair<Guid, string> item)
{
    _selectedItem = item;
    return Task.FromResult(item);
}
```

### BlazorDropList

`BlazorDropList` is a Blazor component designed to render a virtualized, scrollable dropdown-style list that supports lazy loading and custom item display. It is styled according to Material Design principles.

![List](https://github.com/user-attachments/assets/ab3eb98d-c179-4b7e-af3b-6109ded34b0f)

```razor
<BlazorDropList
        T="SelectItem"
        DisplaySelector="@(x => x.Text)"
        OnLoadItemsAsync="LoadPageAsync"
        @bind-Value="@_selectedItem"
        ShowLoadingIndicator="true"
        PageSize="10" />
```

### Parameters

| Parameter                | Type                                   | Description                         |
| ------------------------ | -------------------------------------- | ----------------------------------- |
| T                        | generic                                | Type of the items                   |
| Id                       | string                                 | Optional unique ID (auto-generated) |
| Class                    | string                                 | Optional CSS class                  |
| Style                    | string                                 | Optional inline CSS                 |
| ValueNotFoundMessageText | string                                 | Message shown when list is empty    |
| PageSize                 | int                                    | Items per page (default: 20)        |
| CurrentPage              | int                                    | Initial page index                  |
| Value                    | T                                      | Currently highlighted item          |
| ValueChanged             | EventCallback<T>                       | Triggered when an item is selected  |
| OnItemClickAsync         | Func<T, Task<T>>                       | Custom logic on item click          |
| ItemTemplate             | RenderFragment<T>                      | Custom item template                |
| DisplaySelector          | Func<T, string>                         | Display text selector               |
| OnLoadItemsAsync         | Func<int, int, Task<IEnumerable<T>>>   | Paged data loader                   |
| ShowLoadingIndicator     | bool                                   | Show loading spinner                |

### Example Code

```csharp
private Dictionary<Guid, string> _testValues = new();

private KeyValuePair<Guid, string> _selectedItem;

protected override async Task OnInitializedAsync()
{
    for (int i = 0; i < 1000; i++)
    {
        var id = Guid.NewGuid();
        _testValues.Add(id, $"Record {i}");
    }
}

private Task<IEnumerable<KeyValuePair<Guid, string>>> LoadPageAsync(int page, int pageSize)
{
    return Task.FromResult(_testValues.Skip(page * pageSize).Take(pageSize));
}

private Task<IEnumerable<KeyValuePair<Guid, string>>> SearchForItemAsync(string text)
{
    return Task.FromResult(_testValues.Where(x => x.Value.Contains(text)));
}

private Task<KeyValuePair<Guid, string>> OnValueChanged(KeyValuePair<Guid, string> item)
{
    _selectedItem = item;
    return Task.FromResult(item);
}
```

### BlazorDropMultiSelect

`BlazorDropMultiSelect` is an extension of `BlazorDropSelect` supporting multi-value selection with the same lazy-loading and searchable capabilities.

```razor
<BlazorDropMultiSelect
        T="KeyValuePair<Guid, string>"
        DisplaySelector="@(x => x.Value)"
        OnLoadItemsAsync="LoadPageAsync"
        OnSearchAsync="SearchForItemAsync"
        OnItemClickAsync="OnMultiSelectChanged"
        ShowLoadingIndicator="true"
        Disabled="false"
        Placeholder="Select values"
        PageSize="10"
        Class="multi-select"
        Id="select-multi"
        CurrentPage="@(0)"
        SelectedValues="@_selectedItems" />
```

Parameters (additional to `BlazorDropSelect`):
| Parameter                | Type                                   | Description                                     |
| ------------------------ | -------------------------------------- | ----------------------------------------------- |
| SelectedValues           | IList<T>                               | List of selected items                          |
| MaxDisplayedTags         | int                                    | Max tags before summary (Default: 3)            |
| SelectedItemsTextFormat  | string                                 | Summary text format (default: "Selected: {0}")  |
| GetDisplayTextAsync      | Func<IEnumerable<T>, Task<string>>     | Custom logic for input text display             |

## Custom Item Template

You can customize how each dropdown item is rendered using the `ItemTemplate` parameter. This allows you to change formatting or apply custom styles.

```razor
<BlazorDropSelect T="KeyValuePair<Guid, string>"
                  Placeholder="With custom template"
                  Value="@_templatedSelect"
                  DisplaySelector="@(x => x.Value)"
                  OnLoadItemsAsync="LoadItemsPagedAsync"
                  OnSearchTextChangedAsync="SearchByTextAsync"
                  OnItemClickAsync="OnTemplateItemSelected"
                  ValueNotFoundMessageText="Not found">

    <ItemTemplate Context="item">
        <span class="text-success fw-semibold">@item.Value</span>
    </ItemTemplate>

</BlazorDropSelect>
```

Notes

- ItemTemplate accepts a RenderFragment<T> where T is the item type.
- If specified, this replaces the default item rendering.
- You can apply Bootstrap or custom CSS styles inside the template.

## Loading Indicator Components

`BlazorDropCircularProgressBar` and `BlazorDropLinearProgressBar` - loading bar to indicate data loading.

### Parameters

| Parameter | Type   | Description                    |
| --------- | ------ | ------------------------------ |
| Id        | string | Optional unique identifier     |
| Class     | string | Optional CSS class             |
| Style     | string | Optional inline CSS style      |

## .NET Compatibility

| BlazorDrop Version | Supported .NET Versions                                 |
| ------------------ | ------------------------------------------------------- |
| < 2.0.0            | .NET Core 3.1                                           |
| ≥ 2.0.0            | .NET 6, .NET 8                                          |
|                    | .NET 7, .NET 9, .NET 10 (not tested, potentially works) |
