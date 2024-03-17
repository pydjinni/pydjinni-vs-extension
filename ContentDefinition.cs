using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace PyDjinni
{
    public class ContentDefinition
    {
#pragma warning disable 649
        [Export]
        [Name("pydjinni")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition PyDjinniContentTypeDefinition;

        [Export]
        [FileExtension(".pydjinni")]
        [ContentType("pydjinni")]
        internal static FileExtensionToContentTypeDefinition PyDjinniFileExtensionDefinition;
#pragma warning restore 649
    }
}
