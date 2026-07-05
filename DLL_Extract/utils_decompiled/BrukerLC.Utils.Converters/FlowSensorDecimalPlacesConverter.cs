using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class FlowSensorDecimalPlacesConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] == null || values[1] == null)
		{
			return null;
		}
		double num = (double)System.Convert.ChangeType(values[0], typeof(double));
		string text;
		if (values.Length > 1 && values[1].ToString().Contains("nL/min"))
		{
			text = ((Math.Abs((int)num) < 10) ? " 0.0;-0.0" : " 0;-0");
		}
		else
		{
			double num2 = Math.Abs(num);
			string text2 = ((num2 >= 1000.0) ? " 0;-0" : ((num2 >= 100.0) ? " 0.0;-0.0" : ((!(num2 >= 10.0)) ? " 0.000;-0.000" : " 0.00;-0.00")));
			text = text2;
		}
		return num.ToString(text, CultureInfo.InvariantCulture);
	}

	public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
