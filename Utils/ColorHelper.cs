namespace ghp_app.Utils
{
    public class ColorHelper
    {
        public static string HexToRgba(string hex)
        {
            hex = hex.Replace("#", "");

            byte r = 0, g = 0, b = 0, a = 255;

            if (hex.Length == 6)
            {
                // #RRGGBB format
                r = Convert.ToByte(hex.Substring(0, 2), 16);
                g = Convert.ToByte(hex.Substring(2, 2), 16);
                b = Convert.ToByte(hex.Substring(4, 2), 16);
            }
            else if (hex.Length == 8)
            {
                // #RRGGBBAA format
                r = Convert.ToByte(hex.Substring(0, 2), 16);
                g = Convert.ToByte(hex.Substring(2, 2), 16);
                b = Convert.ToByte(hex.Substring(4, 2), 16);
                a = Convert.ToByte(hex.Substring(6, 2), 16);
            }
            else
            {
                throw new ArgumentException("Invalid hex color format. Use #RRGGBB or #RRGGBBAA.");
            }

            return $"rgba({r}, {g}, {b}, {a / 255.0:0.##})";
        }
    }
}