using System.Text;

namespace DifBot.Helpers;

public class EmojiHelper
{
    public static string GetExternalEmojiImageUrl(string code)
    {
        // eggplant = 0001F346
        // https://emoji.aranja.com/static/emoji-data/img-twitter-72/1f346.png

        // flag =  0001F1E6 0001F1E9
        // https://emoji.aranja.com/static/emoji-data/img-twitter-72/1f1e6-1f1e9.png
        var utf32Encoding = new UTF32Encoding(true, false);

        byte[] bytes = utf32Encoding.GetBytes(code);

        var sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.AppendFormat("{0:X2}", bytes[i]);
        }

        string bytesAsString = sb.ToString();

        var fileNameBuilder = new StringBuilder();

        for (int i = 0; i < bytesAsString.Length; i += 8)
        {
            string unicodePart = bytesAsString.Substring(i, 8)
                .TrimStart('0')
                .ToLower();

            fileNameBuilder.Append(i == 0 ? unicodePart : $"-{unicodePart}");
        }

        string fileName = fileNameBuilder.ToString();

        return $"https://emoji.aranja.com/static/emoji-data/img-twitter-72/{fileName}.png";
    }
}
