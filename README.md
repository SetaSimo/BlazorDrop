![.NET Core 3.1](https://img.shields.io/badge/.NET%20Core-3.1-blue)
![.NET 6](https://img.shields.io/badge/.NET-6-blue)
![.NET 8](https://img.shields.io/badge/.NET-8-blueviolet)
![.NET 10](https://img.shields.io/badge/.NET-10-blueviolet)
[![NuGet](https://img.shields.io/nuget/v/BlazorDrop.svg)](https://www.nuget.org/packages/BlazorDrop)

# BlazorDrop

**BlazorDrop** is a small Material-styled Blazor component library: a searchable **dropdown**, a **multi-select**
with chips and a **list**, all loading data **page by page** (infinite scroll) from memory, a delegate or a
database, synchronously or asynchronously. One NuGet package serves **.NET Core 3.1** and **.NET 6 / 8 / 10**.

> ## ⚠️ Version 3.0 is **not compatible** with 2.x
> v3 fixes broken two-way binding and reworks the data-loading API. See **[Migrating from 2.x](#migrating-from-2x)**.
> To stay on the old API, pin `2.0.3`.

## Features

- **Paged loading + infinite scroll** (also on .NET Core 3.1, which has no `<Virtualize>`)
- **Four ways to supply data**: in-memory `Items`, page loaders, a paged+searchable query (`OnQueryAsync`) or an `IBlazorDropDataSource<T>`
- **Sync and async** variants of every loader; `CancellationToken` passed to async queries; stale results discarded
- **Server-side search that pages** (`Skip`/`Take` on the filtered query) or a built-in client-side filter
- **Two-way binding**: `@bind-Value` (Select/List), `@bind-SelectedValues` (MultiSelect)
- **Database entities**: `KeySelector="@(x => x.Id)"` matches rows loaded by different queries
- **EditForm / DataAnnotations** integration for all three components
- **Material Design**: Filled/Outlined, elevation, state layers, chips, dark theme, RTL, dense mode
- **Responsive**: flip near the viewport edge, **mobile bottom sheet** with scrim and Done button
- **Keyboard + WAI-ARIA combobox/listbox** semantics; **localizable** UI strings
- Templates (item / empty / loading / no-more-data), error state with retry, `@ref` methods (`ReloadAsync`, `OpenAsync`, ...)

## Installation

```bash
dotnet add package BlazorDrop
```

Register the service (`Program.cs` or `Startup.ConfigureServices`):

```csharp
using BlazorDrop.Extensions;

services.AddBlazorDrop();
```

Add the namespaces to `_Imports.razor`:

```razor
@using BlazorDrop.Components
@using BlazorDrop.Data
```

Reference the JS and CSS once in your host page (`_Host.cshtml`, `App.razor` or `index.html`):

```html
<link rel="stylesheet" href="_content/BlazorDrop/BlazorDropSelect.css" />
<script src="_content/BlazorDrop/BlazorDropSelect.js"></script>
```

## Quick start

```razor
<BlazorDropSelect T="Country"
                  Label="Country"
                  Placeholder="Search…"
                  Clearable="true"
                  @bind-Value="_country"
                  DisplaySelector="@(x => x.Name)"
                  Items="_countries" />

@code {
    private Country? _country;
    private List<Country> _countries = /* any IEnumerable<T> */;
    public record Country(int Id, string Name);
}
```

`Items` is paged and filtered on the client, so even a 50 000-row list stays fast.

## Loading data

Pick **one** of the following on any component. All of them feed the same paging / search / error pipeline.

| Level | Parameters | Use when |
| ----- | ---------- | -------- |
| 1. In memory | `Items` | You already have the list. Paged + filtered client-side. |
| 2. Page loader | `OnLoadItems` / `OnLoadItemsAsync` `(page, pageSize)` + optional `Search` / `SearchAsync` `(text)` | Simple REST endpoints. `Search` returns *all* matches at once; without it the loaded pages are filtered on the client. |
| 3. Query | `OnQuery` / `OnQueryAsync` `(query[, ct])` | **Databases**: one delegate receives page, page size and search text. |
| 4. Data source | `DataSource` (`IBlazorDropDataSource<T>`) | Reusable / testable sources, precise "has more", `IQueryable` helper. |

Pages are **zero-based**. "Has more" is inferred from a full page (`count >= PageSize`) unless the source says otherwise.

### Database (EF Core) with `OnQueryAsync`

```razor
<BlazorDropSelect T="Customer"
                  Label="Customer"
                  @bind-Value="_customer"
                  DisplaySelector="@(x => x.Name)"
                  KeySelector="@(x => x.Id)"
                  OnQueryAsync="LoadCustomersAsync" />

@code {
    private async Task<IEnumerable<Customer>> LoadCustomersAsync(BlazorDropQuery q, CancellationToken ct)
    {
        IQueryable<Customer> query = _db.Customers;
        if (q.HasSearch)
            query = query.Where(c => c.Name.Contains(q.SearchText));

        return await query.OrderBy(c => c.Name)
                          .Skip(q.Skip)            // q.Page * q.PageSize
                          .Take(q.PageSize)
                          .ToListAsync(ct);
    }
}
```

The same delegate serves browsing (`q.SearchText == ""`) and typed searches, and both page. Use `OnQuery`
for a synchronous version (`query => IEnumerable<T>`).

### `IQueryable` helper (no EF dependency)

```csharp
DataSource = BlazorDropDataSource.FromQueryable<Customer>(
    q => q.HasSearch ? _db.Customers.Where(c => c.Name.Contains(q.SearchText)).OrderBy(c => c.Name)
                     : _db.Customers.OrderBy(c => c.Name),
    (query, ct) => query.ToListAsync(ct));   // omit for a synchronous ToList()
```

The helper applies `Skip`/`Take` and fetches one extra row to know exactly whether a next page exists.
Other factories: `FromEnumerable(items)`, `FromDelegate(query => ...)`, `FromDelegate(async (query, ct) => ...)`.
Implement `IBlazorDropDataSource<T>` yourself for full control (return a `BlazorDropPage<T>` with `HasMore`).

### Page loader + search (classic API)

```razor
<BlazorDropSelect T="Item" @bind-Value="_item"
                  DisplaySelector="@(x => x.Text)"
                  OnLoadItemsAsync="@((page, size) => Api.GetPageAsync(page, size))"
                  SearchAsync="@(text => Api.SearchAsync(text))" />
```

### Entities loaded from a database

Rows loaded by different queries are different object instances, so set **`KeySelector`** (or `ItemComparer`)
to tell the component which items are "the same": `KeySelector="@(x => x.Id)"`.

### Refreshing

```razor
<BlazorDropList @ref="_list" T="Customer" ... />
<button @onclick="() => _list!.ReloadAsync()">Reload</button>
```

Assigning a new `Items` reference also reloads.

## Components

### BlazorDropSelect (single select)

```razor
<BlazorDropSelect T="Customer"
                  Label="Customer" Placeholder="Search…"
                  Clearable="true" FullWidth="true"
                  Variant="DropdownVariant.Outlined"
                  @bind-Value="_customer"
                  DisplaySelector="@(x => x.Name)"
                  KeySelector="@(x => x.Id)"
                  OnQueryAsync="LoadCustomersAsync"
                  ValueNotFoundMessageText="Nothing found" />
```

### BlazorDropMultiSelect

Selections render as Material chips with per-chip remove and a `+N` overflow. The component never mutates
the list you pass in; it emits a fresh list.

```razor
<BlazorDropMultiSelect T="Tag"
                       Placeholder="Select tags…"
                       Clearable="true"
                       MaxDisplayedChips="3"
                       MaxSelectedItems="5"
                       @bind-SelectedValues="_tags"
                       DisplaySelector="@(x => x.Name)"
                       KeySelector="@(x => x.Id)"
                       Items="_allTags" />
```

### BlazorDropList

```razor
<BlazorDropList T="Customer"
                AriaLabel="Customers"
                @bind-Value="_customer"
                DisplaySelector="@(x => x.Name)"
                OnLoadItems="@((page, size) => _repo.GetPage(page, size))"
                PageSize="20" />
```

### Inside an EditForm

```razor
<EditForm Model="_model" OnValidSubmit="Submit">
    <DataAnnotationsValidator />
    <BlazorDropSelect T="Customer" Label="Required" @bind-Value="_model.Customer" DisplaySelector="@(x => x.Name)" Items="_customers" />
    <ValidationMessage For="@(() => _model.Customer)" />
    <BlazorDropMultiSelect T="Tag" @bind-SelectedValues="_model.Tags" DisplaySelector="@(x => x.Name)" Items="_tags" />
    <ValidationMessage For="@(() => _model.Tags)" />
    <button type="submit">Submit</button>
</EditForm>
```

`@bind-Value` / `@bind-SelectedValues` supply the field expression automatically; the root element gets the
`modified` / `valid` / `invalid` classes.

### Templates

```razor
<BlazorDropSelect T="Customer" @bind-Value="_customer" DisplaySelector="@(x => x.Name)" Items="_customers">
    <ItemTemplate Context="item">
        <strong>@item.Name</strong> <small>@item.City</small>
    </ItemTemplate>
    <EmptyTemplate>Nothing matches.</EmptyTemplate>
    <LoadingTemplate>Loading…</LoadingTemplate>
    <NoMoreDataTemplate><em>That's all.</em></NoMoreDataTemplate>
</BlazorDropSelect>
```

## Keyboard and accessibility

| Key | Select / MultiSelect | List |
| --- | -------------------- | ---- |
| `ArrowDown` / `ArrowUp` | open, move the active option (loads the next page at the end) | move |
| `Enter` | pick the active option (retries after an error) | pick |
| `Escape` / `Tab` | close | — |
| `Backspace` on empty input | MultiSelect: remove the last chip | — |
| `Home` / `End` | move the caret | first / last option |

The input is a WAI-ARIA `combobox` controlling a `listbox`; the list is a focusable `listbox`. Status rows
(loading, empty, error) are live regions. Enter never submits a surrounding form while the list is open.

## Localization

```razor
<CascadingValue Value="_texts">   @* localizes every BlazorDrop component below *@
    ...
</CascadingValue>

@code {
    private readonly BlazorDropTexts _texts = new()
    {
        NoItems = "Nothing matched your search", LoadFailed = "Could not load the data.", Retry = "Try again",
        Loading = "Fetching...", ClearSelection = "Clear the selection", RemoveItemFormat = "Remove {0}",
        MoreSelectedFormat = "+{0} more", Done = "Apply",
    };
}
```

Or pass `Texts="_texts"` to one component, or set `BlazorDropTexts.Default` at startup for a single-culture app.

## Methods (`@ref`)

| Method | Components | Description |
| ------ | ---------- | ----------- |
| `ReloadAsync()` | all | Discard loaded pages and load the first page again |
| `SelectItemAsync(item)` | all | Select programmatically |
| `FocusAsync()` | all | Focus the input / list |
| `OpenAsync()` / `CloseAsync()` | Select, MultiSelect | Open / close the dropdown |
| `ClearAsync()` | Select, MultiSelect | Clear the selection |

## Parameters

### Common (all components)

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `T` | generic | Item type |
| `Items` | `IEnumerable<T>` | In-memory items (paged + filtered client-side) |
| `OnLoadItems` / `OnLoadItemsAsync` | `Func<int,int,IEnumerable<T>>` / `…Task<IEnumerable<T>>` | Page loader `(page, pageSize)` |
| `OnQuery` / `OnQueryAsync` | `Func<BlazorDropQuery,IEnumerable<T>>` / `Func<BlazorDropQuery,CancellationToken,Task<IEnumerable<T>>>` | Paged + searchable query |
| `DataSource` | `IBlazorDropDataSource<T>` | Full-control data source |
| `PageSize` | int | Items per page (default 20) |
| `CurrentPage` | int | Zero-based page loaded first when browsing (default 0) |
| `DisplaySelector` | `Func<T,string>` | Item text (default `ToString()`) |
| `KeySelector` | `Func<T,object?>` | Item identity (recommended for entities) |
| `ItemComparer` | `IEqualityComparer<T>` | Custom equality (ignored when `KeySelector` is set) |
| `SearchComparison` | `StringComparison` | Client-side filter comparison (default `OrdinalIgnoreCase`) |
| `ItemTemplate` | `RenderFragment<T>` | Custom item markup |
| `EmptyTemplate` / `LoadingTemplate` / `NoMoreDataTemplate` | `RenderFragment` | State templates |
| `ValueNotFoundMessageText` | string | Empty-state text (default `Texts.NoItems`) |
| `Texts` | `BlazorDropTexts` | UI strings (or cascade one) |
| `ShowLoadingIndicator` | bool | Show progress indicators (default true) |
| `Dense` / `FullWidth` | bool | Compact layout / stretch to the container |
| `MaxDropdownHeight` | string | e.g. `"300px"` |
| `OnItemClickAsync` | `Func<T,Task<T>>` | Intercept a pick and return the value to store |
| `OnReachedEnd` | `EventCallback` | Next page loaded after scrolling to the end |
| `OnError` | `EventCallback<Exception>` | Data source threw (error state + retry shown) |
| `Id` / `Class` / `Style` / `AriaLabel` | string | Root element attributes |
| *(any other attribute)* | | Rendered on the root element (`data-*`, `title`, …) |

### Select / MultiSelect

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `Placeholder` / `Label` | string | Input placeholder / floating label |
| `Variant` | `DropdownVariant` | `Filled` (default) or `Outlined` |
| `Clearable` | bool | Show a clear button |
| `Disabled` | bool | Disable interaction (closes an open dropdown) |
| `UpdateSearchDelayInMilliseconds` | int | Typing debounce (default 500) |
| `MinSearchLength` | int | Characters required before searching (default 0) |
| `Search` / `SearchAsync` | `Func<string,IEnumerable<T>>` / `…Task<IEnumerable<T>>` | Search for the page-loader API (returns all matches) |
| `OnOpen` / `OnClose` / `OnCleared` | `EventCallback` | Lifecycle events |
| `OnSearchChanged` | `EventCallback<string>` | Debounced input text |

### BlazorDropSelect

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `Value` / `ValueChanged` / `ValueExpression` | `T` / `EventCallback<T>` / `Expression<Func<T>>` | `@bind-Value` |

### BlazorDropMultiSelect

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `SelectedValues` / `SelectedValuesChanged` / `SelectedValuesExpression` | `IList<T>` / `EventCallback<IList<T>>` / `Expression<Func<IList<T>>>` | `@bind-SelectedValues` |
| `MaxDisplayedChips` | int | Chips before the `+N` summary (default 3) |
| `MaxSelectedItems` | int | Selection limit (0 = unlimited) |
| `CloseOnSelect` | bool | Close after each pick (default false) |

### BlazorDropList

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `Value` / `ValueChanged` / `ValueExpression` | `T` / `EventCallback<T>` / `Expression<Func<T>>` | `@bind-Value` |

## Theming

Override the `--bzd-*` custom properties on `:root` or any ancestor. The default theme is light; add
`bzd-theme-dark` on an ancestor to force dark mode or `bzd-theme-auto` to follow the OS. Layout uses CSS
logical properties, so `dir="rtl"` works.

```css
:root {
  --bzd-primary: #3f51b5;
  --bzd-surface: #fff;           --bzd-on-surface: #1c1b1f;
  --bzd-surface-variant: #f3f3f3; --bzd-on-surface-variant: #49454f;
  --bzd-outline: #ccc;            --bzd-placeholder: #605d66;   --bzd-error: #b3261e;
  --bzd-border-radius: 4px;       --bzd-font-family: "Roboto", sans-serif;
  --bzd-dropdown-max-height: 280px; --bzd-list-max-height: 320px;
  --bzd-dropdown-z-index: 100;    --bzd-sheet-z-index: 1000;
}
```

**Layout notes.** The dropdown is rendered inside the component (no portal): an ancestor with `overflow: hidden`
clips it, and inside dialogs raise `--bzd-dropdown-z-index`. The mobile sheet uses `position: fixed`, which an
ancestor with `transform`/`filter` turns into local positioning.

## .NET compatibility

| BlazorDrop | Runs on | Package asset |
| ---------- | ------- | ------------- |
| 3.x | .NET Core 3.1, Blazor WebAssembly 3.2 | `netstandard2.1` (Microsoft.AspNetCore.Components 3.1.x) |
| 3.x | .NET 6, 7 | `net6.0` |
| 3.x | .NET 8, 9 | `net8.0` |
| 3.x | .NET 10+ | `net10.0` |
| 2.x | .NET 6, .NET 8 | |
| 1.x | .NET Core 3.1 | |

One code base, one package: the sources stay within C# 8 / Razor 3.0 and anything newer is `#if`-guarded,
so a bug fix ships to every line at once. Infinite scroll is JS-driven (no `<Virtualize>`), which is why it
behaves identically on 3.1. On 3.1 the renderer disposes components synchronously; the components handle
both `IDisposable` and `IAsyncDisposable`.

## Migrating from 2.x

| 2.x | 3.0 |
| --- | --- |
| MultiSelect `SelectedValuesChanged` is `EventCallback<T>` (never fired) | `EventCallback<IList<T>>`; use `@bind-SelectedValues` |
| MultiSelect `GetDisplayTextAsync` | Removed; selections render as chips via `DisplaySelector` |
| `BlazorDropList` `SelectedValuesChanged` | `Value` + `ValueChanged` (`@bind-Value`) |
| `BaseLazy…<T, R>` | `BaseLazy…<T>` |
| `SearchAsync` result replaces the loaded list | Search is non-destructive; server search pages via `OnQueryAsync` |
| `Items` copied once, unpaged | `Items` is paged, filtered and reactive |
| `IBlazorDropInteropService` / `IBlazorDropInvokable` public | Internal (use bUnit `JSInterop.Mode = Loose` in tests) |
| "Failed to load." etc. hard-coded | `BlazorDropTexts` |

## Development

```bash
dotnet build BlazorDrop.sln          # netstandard2.1 + net6.0 + net8.0 + net10.0
dotnet test                          # bUnit suite on net8.0 and net10.0
dotnet run --project BlazorDropTest  # demo app
```

Publishing (both lines in one package): `.\scripts\publish-nuget.ps1 -Bump patch -DryRun`, then without
`-DryRun` and with `-ApiKey` (or `$env:NUGET_API_KEY`). Pushing a `vX.Y.Z` tag runs the same steps in
GitHub Actions (`.github/workflows/release.yml`, secret `NUGET_API_KEY`).

## License

MIT — see [LICENSE](LICENSE).
