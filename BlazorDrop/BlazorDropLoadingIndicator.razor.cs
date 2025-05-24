using System;

namespace BlazorDrop
{
    public partial class BlazorDropLoadingIndicator
    {
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public string Class { get; set; }
    }
}
