using System.Windows.Media;

namespace BrukerLC.Utils.FontIcon;

public struct FontIcon
{
	public string FontString { get; set; }

	public FontFamily FontFamily { get; set; }

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is FontIcon)
		{
			return Equals((FontIcon)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((FontString != null) ? FontString.GetHashCode() : 0) * 397) ^ ((FontFamily != null) ? FontFamily.GetHashCode() : 0);
	}

	public bool Equals(FontIcon other)
	{
		if (string.Equals(FontString, other.FontString))
		{
			return object.Equals(FontFamily, other.FontFamily);
		}
		return false;
	}

	public static bool operator ==(FontIcon a, FontIcon b)
	{
		if (a.FontString == b.FontString)
		{
			return a.FontFamily == b.FontFamily;
		}
		return false;
	}

	public static bool operator !=(FontIcon a, FontIcon b)
	{
		return !(a == b);
	}
}
