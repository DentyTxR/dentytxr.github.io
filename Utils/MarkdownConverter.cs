using Markdig;
using Microsoft.AspNetCore.Components;

namespace ghp_app.Utils
{
    public static class MarkdownConverter
    {
        private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoLinks()
            .Build();

        public static MarkupString Parse(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return new MarkupString(string.Empty);

            // Escape angle brackets for custom game tags so the browser's
            // innerHTML parser doesn't choke on them in WASM
            // (Optional: only do this if your logs contain custom tags like <size> or <color>)

            var html = Markdown.ToHtml(markdown, _pipeline);

            return new MarkupString(html);
        }
    }
}