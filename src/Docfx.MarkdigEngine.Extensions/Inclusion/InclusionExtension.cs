// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Microsoft.DocAsCode.MarkdigEngine.Extensions;

namespace Docfx.MarkdigEngine.Extensions;

/// <summary>
/// Extension to enable extension IncludeFile.
/// </summary>
public class InclusionExtension : IMarkdownExtension
{
    private readonly MarkdownContext _context;
    private MarkdownPipeline _inlinePipeline;

    public InclusionExtension(MarkdownContext context)
    {
        _context = context;
    }

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.BlockParsers.AddIfNotAlready<InclusionBlockParser>();
        pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new InclusionInlineParser());

        var inlineShortcut = new InclusionInlineShortcutParser();
        pipeline.InlineParsers.InsertBefore<XrefInlineShortParser>(inlineShortcut);
        pipeline.InlineParsers.AddIfNotAlready(inlineShortcut);

        pipeline.BlockParsers.AddIfNotAlready<InclusionShortcutBlockParser>();
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            if (!htmlRenderer.ObjectRenderers.Contains<HtmlInclusionInlineRenderer>())
            {
                var inlinePipeline = LazyInitializer.EnsureInitialized(ref _inlinePipeline, () => CreateInlineOnlyPipeline(pipeline));

                htmlRenderer.ObjectRenderers.Insert(0, new HtmlInclusionInlineRenderer(_context, inlinePipeline));
            }

            if (!htmlRenderer.ObjectRenderers.Contains<HtmlInclusionInlineShortcutRenderer>())
            {
                var inlinePipeline = LazyInitializer.EnsureInitialized(ref _inlinePipeline, () => CreateInlineOnlyPipeline(pipeline));

                htmlRenderer.ObjectRenderers.Insert(0, new HtmlInclusionInlineShortcutRenderer(_context, inlinePipeline));
            }

            if (!htmlRenderer.ObjectRenderers.Contains<HtmlInclusionBlockRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, new HtmlInclusionBlockRenderer(_context, pipeline));
            }

            if (!htmlRenderer.ObjectRenderers.Contains<HtmlInclusionShortcutBlocktRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, new HtmlInclusionShortcutBlocktRenderer(_context, pipeline));
            }
        }
    }

    private static MarkdownPipeline CreateInlineOnlyPipeline(MarkdownPipeline pipeline)
    {
        var builder = new MarkdownPipelineBuilder();

        foreach (var extension in pipeline.Extensions)
        {
            builder.Extensions.Add(extension);
        }

        builder.UseInlineOnly();

        return builder.Build();
    }
}
