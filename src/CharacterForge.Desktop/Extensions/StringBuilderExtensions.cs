using System.Text;

namespace CharacterForge.Desktop.Extensions;

public static class StringBuilderExtensions
{
    extension(StringBuilder @this)
    {
        public void AppendTextBlockWithHeader(string name, string value)
        {
            if (value.Length != 0) @this.AppendLine($"{name}:\n```plaintext\n{value}\n```\n");
        }
    }
}
