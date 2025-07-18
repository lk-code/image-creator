namespace ImageCreator.Extensions;

public static class ListExtensions
{
    public static string ToDisplayableList(this List<string> items, string? separator = null)
    {
        separator ??= Environment.NewLine;
        
        StringWriter writer = new();

        List<string> itemList = items.ToList();
        for (int i = 0; i < itemList.Count; i++)
        {
            writer.Write(itemList[i]);
            if (i < itemList.Count - 1)
            {
                writer.Write(separator);
            }
        }

        return writer.ToString();
    }
}