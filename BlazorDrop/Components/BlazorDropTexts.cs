namespace BlazorDrop.Components
{
    public sealed class BlazorDropTexts
    {
        public static BlazorDropTexts Default { get; set; } = new BlazorDropTexts();

        public string NoItems { get; set; } = "No items found";

        public string LoadFailed { get; set; } = "Failed to load.";

        public string Retry { get; set; } = "Retry";

        public string Loading { get; set; } = "Loading…";

        public string ClearSelection { get; set; } = "Clear selection";

        public string RemoveItemFormat { get; set; } = "Remove {0}";

        public string MoreSelectedFormat { get; set; } = "{0} more selected";

        public string Done { get; set; } = "Done";
    }
}
