using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BalticWpfControlLib.Controls;

public class IPControl : TextBox
{
	private const string IpRegex = "([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([0-9]|[0-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]|[*])";

	public IPControl()
	{
		base.PreviewTextInput += TextBoxPreviewTextInput;
		base.PreviewKeyDown += HandleHandledKeyDown;
		DataObject.AddPastingHandler(this, IPControl_Pasting);
	}

	public void HandleHandledKeyDown(object sender, RoutedEventArgs e)
	{
		if (e is KeyEventArgs { Key: Key.Space })
		{
			e.Handled = true;
		}
	}

	private static bool ValidCharacter(string enteredValue)
	{
		if (!Regex.Match(enteredValue, "[0-9.]", RegexOptions.IgnoreCase).Success)
		{
			return false;
		}
		return true;
	}

	private static int CountOfDot(string text)
	{
		return text.Count((char c) => c == '.');
	}

	private string AlterValue(TextCompositionEventArgs e, TextBox text, string val, int insertIndex, bool replace, out bool moveNext, int replaceIndex = 0)
	{
		string text2 = text.Text;
		string oldValue = text2;
		if (replace)
		{
			text2 = text2.Remove(replaceIndex, 1);
		}
		text2 = text2.Insert(insertIndex, val);
		if (!ValidIpFragment(text2, out moveNext))
		{
			return Manipulate(e, text, oldValue, val, insertIndex);
		}
		return text2;
	}

	private void Do(TextCompositionEventArgs e, string val, TextBox tx, int ind, int nextIndexOfDot, int indexDiff, int dotCount)
	{
		bool moveNext;
		switch (indexDiff)
		{
		case 0:
		{
			int num = Math.Abs(FindPreviousIndexOf(".", tx.Text, ind - 1) - nextIndexOfDot);
			if (4 > num)
			{
				Do(e, val, tx, ind, nextIndexOfDot, num, dotCount);
			}
			else
			{
				HandleTextInput(e, ind + 1, val, tx);
			}
			break;
		}
		case 4:
			if (tx.Text.Length <= ind)
			{
				if (dotCount >= 3)
				{
					e.Handled = true;
					tx.CaretIndex = ind + 1;
					break;
				}
				if (val != ".")
				{
					tx.Text = AlterValue(e, tx, ".", ind, replace: false, out moveNext);
					tx.Text = AlterValue(e, tx, val, ind + 1, replace: false, out moveNext);
				}
				else
				{
					tx.Text = AlterValue(e, tx, val, ind, replace: false, out moveNext);
				}
				tx.CaretIndex = ind + 2;
			}
			else
			{
				string text2 = AlterValue(e, tx, val, ind, replace: true, out moveNext, ind);
				if (!moveNext)
				{
					tx.Text = text2;
					tx.CaretIndex = ind + 1;
				}
				else
				{
					int caretIndex2 = tx.CaretIndex;
					tx.Text = text2;
					tx.CaretIndex = caretIndex2 + 1;
				}
			}
			e.Handled = true;
			break;
		default:
		{
			string text = AlterValue(e, tx, val, ind, replace: false, out moveNext);
			e.Handled = true;
			if (!moveNext)
			{
				tx.Text = text;
				tx.CaretIndex = ind + 1;
			}
			else
			{
				int caretIndex = tx.CaretIndex;
				tx.Text = text;
				tx.CaretIndex = caretIndex + 1;
			}
			break;
		}
		}
	}

	private int FindNextIndexOf(string p, string text, int ind)
	{
		int num = text.IndexOf(p, ind);
		if (num == -1)
		{
			return text.Length;
		}
		return num;
	}

	private int FindPreviousIndexOf(string p, string text, int ind)
	{
		if (ind < 0)
		{
			return ind;
		}
		return text.LastIndexOf(p, ind);
	}

	private void HandleExcessDot(TextCompositionEventArgs e, int ind, TextBox textBox)
	{
		if (textBox.Text.Length > ind)
		{
			int num = FindNextIndexOf(".", textBox.Text, ind);
			if (num != textBox.Text.Length)
			{
				int count = num - ind;
				textBox.Text = textBox.Text.Remove(ind, count);
				textBox.CaretIndex = ind + 1;
			}
			else
			{
				textBox.CaretIndex = num;
			}
		}
		e.Handled = true;
	}

	private void HandleTextInput(TextCompositionEventArgs e, int ind, string enteredValue, TextBox textBox)
	{
		if (!ValidCharacter(enteredValue))
		{
			e.Handled = true;
			textBox.CaretIndex = ind;
			return;
		}
		int num = CountOfDot(textBox.Text);
		if (enteredValue == "." && num >= 3)
		{
			HandleExcessDot(e, ind, textBox);
			return;
		}
		int num2 = FindPreviousIndexOf(".", textBox.Text, ind);
		int num3 = FindNextIndexOf(".", textBox.Text, ind);
		int num4 = num3 - num2;
		if (4 >= num4)
		{
			Do(e, enteredValue, textBox, ind, num3, num4, num);
		}
		else
		{
			e.Handled = true;
		}
	}

	private void IPControl_Pasting(object sender, DataObjectPastingEventArgs e)
	{
		((TextBox)sender).Clear();
		if (e.DataObject.GetDataPresent(typeof(string)))
		{
			if (!Regex.IsMatch((string)e.DataObject.GetData(typeof(string)), "([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([0-9]|[0-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]|[*])"))
			{
				e.CancelCommand();
			}
		}
		else
		{
			e.CancelCommand();
		}
	}

	private string Manipulate(TextCompositionEventArgs e, TextBox text, string oldValue, string val, int insertIndex)
	{
		int num = FindPreviousIndexOf(".", oldValue, insertIndex);
		int num2 = FindNextIndexOf(".", oldValue, insertIndex);
		int num3 = num2 - num;
		int num4 = CountOfDot(oldValue);
		if (0 < num3)
		{
			if (insertIndex != num2)
			{
				oldValue = oldValue.Remove(num + 1, num3 - 1);
				oldValue = oldValue.Insert(num + 1, val);
				text.CaretIndex = num + 1;
			}
			else if (num4 < 3)
			{
				HandleTextInput(e, text.CaretIndex, ".", text);
				HandleTextInput(e, text.CaretIndex, val, text);
				oldValue = text.Text;
			}
		}
		else
		{
			HandleTextInput(e, num2 + 1, val, text);
			text.CaretIndex--;
			oldValue = text.Text;
		}
		return oldValue;
	}

	private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		string text = e.Text;
		HandleTextInput(e, textBox.CaretIndex, text, textBox);
	}

	private static bool ValidIpFragment(string textVal, out bool moveToNext)
	{
		moveToNext = false;
		string[] array = textVal.Split('.');
		int num = 0;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text) && int.Parse(text) > 255)
			{
				if (num < 3)
				{
					moveToNext = true;
				}
				return false;
			}
			num++;
		}
		return true;
	}
}
