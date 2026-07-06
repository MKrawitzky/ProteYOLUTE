// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000086 RID: 134
	public class IPControl : TextBox
	{
		// Token: 0x06000636 RID: 1590 RVA: 0x0003AAE5 File Offset: 0x00038CE5
		public IPControl()
		{
			base.PreviewTextInput += this.TextBoxPreviewTextInput;
			base.PreviewKeyDown += new KeyEventHandler(this.HandleHandledKeyDown);
			DataObject.AddPastingHandler(this, new DataObjectPastingEventHandler(this.IPControl_Pasting));
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0003AB24 File Offset: 0x00038D24
		public void HandleHandledKeyDown(object sender, RoutedEventArgs e)
		{
			KeyEventArgs ke = e as KeyEventArgs;
			if (ke != null && ke.Key == Key.Space)
			{
				e.Handled = true;
			}
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0003AB4C File Offset: 0x00038D4C
		private static bool ValidCharacter(string enteredValue)
		{
			return Regex.Match(enteredValue, "[0-9.]", RegexOptions.IgnoreCase).Success;
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0003AB64 File Offset: 0x00038D64
		private static int CountOfDot(string text)
		{
			return text.Count((char c) => c == '.');
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0003AB8C File Offset: 0x00038D8C
		private string AlterValue(TextCompositionEventArgs e, TextBox text, string val, int insertIndex, bool replace, out bool moveNext, int replaceIndex = 0)
		{
			string textVal = text.Text;
			string oldValue = textVal;
			if (replace)
			{
				textVal = textVal.Remove(replaceIndex, 1);
			}
			textVal = textVal.Insert(insertIndex, val);
			if (!IPControl.ValidIpFragment(textVal, out moveNext))
			{
				return this.Manipulate(e, text, oldValue, val, insertIndex);
			}
			return textVal;
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x0003ABD4 File Offset: 0x00038DD4
		private void Do(TextCompositionEventArgs e, string val, TextBox tx, int ind, int nextIndexOfDot, int indexDiff, int dotCount)
		{
			if (indexDiff != 0)
			{
				bool moveNext;
				if (indexDiff == 4)
				{
					if (tx.Text.Length <= ind)
					{
						if (dotCount >= 3)
						{
							e.Handled = true;
							tx.CaretIndex = ind + 1;
							return;
						}
						if (val != ".")
						{
							tx.Text = this.AlterValue(e, tx, ".", ind, false, out moveNext, 0);
							tx.Text = this.AlterValue(e, tx, val, ind + 1, false, out moveNext, 0);
						}
						else
						{
							tx.Text = this.AlterValue(e, tx, val, ind, false, out moveNext, 0);
						}
						tx.CaretIndex = ind + 2;
					}
					else
					{
						string ret = this.AlterValue(e, tx, val, ind, true, out moveNext, ind);
						if (!moveNext)
						{
							tx.Text = ret;
							tx.CaretIndex = ind + 1;
						}
						else
						{
							int carInd = tx.CaretIndex;
							tx.Text = ret;
							tx.CaretIndex = carInd + 1;
						}
					}
					e.Handled = true;
					return;
				}
				string ret2 = this.AlterValue(e, tx, val, ind, false, out moveNext, 0);
				e.Handled = true;
				if (!moveNext)
				{
					tx.Text = ret2;
					tx.CaretIndex = ind + 1;
					return;
				}
				int carInd2 = tx.CaretIndex;
				tx.Text = ret2;
				tx.CaretIndex = carInd2 + 1;
				return;
			}
			else
			{
				int indexDiff2 = Math.Abs(this.FindPreviousIndexOf(".", tx.Text, ind - 1) - nextIndexOfDot);
				if (4 > indexDiff2)
				{
					this.Do(e, val, tx, ind, nextIndexOfDot, indexDiff2, dotCount);
					return;
				}
				this.HandleTextInput(e, ind + 1, val, tx);
				return;
			}
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0003AD44 File Offset: 0x00038F44
		private int FindNextIndexOf(string p, string text, int ind)
		{
			int index = text.IndexOf(p, ind);
			if (index == -1)
			{
				return text.Length;
			}
			return index;
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0003AD66 File Offset: 0x00038F66
		private int FindPreviousIndexOf(string p, string text, int ind)
		{
			if (ind < 0)
			{
				return ind;
			}
			return text.LastIndexOf(p, ind);
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0003AD78 File Offset: 0x00038F78
		private void HandleExcessDot(TextCompositionEventArgs e, int ind, TextBox textBox)
		{
			if (textBox.Text.Length > ind)
			{
				int findNextIndexOf = this.FindNextIndexOf(".", textBox.Text, ind);
				if (findNextIndexOf != textBox.Text.Length)
				{
					int count = findNextIndexOf - ind;
					textBox.Text = textBox.Text.Remove(ind, count);
					textBox.CaretIndex = ind + 1;
				}
				else
				{
					textBox.CaretIndex = findNextIndexOf;
				}
			}
			e.Handled = true;
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0003ADE4 File Offset: 0x00038FE4
		private void HandleTextInput(TextCompositionEventArgs e, int ind, string enteredValue, TextBox textBox)
		{
			if (!IPControl.ValidCharacter(enteredValue))
			{
				e.Handled = true;
				textBox.CaretIndex = ind;
				return;
			}
			int dotCount = IPControl.CountOfDot(textBox.Text);
			if (enteredValue == "." && dotCount >= 3)
			{
				this.HandleExcessDot(e, ind, textBox);
				return;
			}
			int previousIndexOfDot = this.FindPreviousIndexOf(".", textBox.Text, ind);
			int nextIndexOfDot = this.FindNextIndexOf(".", textBox.Text, ind);
			int indexDiff = nextIndexOfDot - previousIndexOfDot;
			if (4 >= indexDiff)
			{
				this.Do(e, enteredValue, textBox, ind, nextIndexOfDot, indexDiff, dotCount);
				return;
			}
			e.Handled = true;
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0003AE78 File Offset: 0x00039078
		private void IPControl_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			((TextBox)sender).Clear();
			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				if (!Regex.IsMatch((string)e.DataObject.GetData(typeof(string)), "([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([0-9]|[0-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]|[*])"))
				{
					e.CancelCommand();
					return;
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0003AEDC File Offset: 0x000390DC
		private string Manipulate(TextCompositionEventArgs e, TextBox text, string oldValue, string val, int insertIndex)
		{
			int previousIndexOfDot = this.FindPreviousIndexOf(".", oldValue, insertIndex);
			int nextIndexOfDot = this.FindNextIndexOf(".", oldValue, insertIndex);
			int indexDiff = nextIndexOfDot - previousIndexOfDot;
			int countOfDot = IPControl.CountOfDot(oldValue);
			if (0 < indexDiff)
			{
				if (insertIndex != nextIndexOfDot)
				{
					oldValue = oldValue.Remove(previousIndexOfDot + 1, indexDiff - 1);
					oldValue = oldValue.Insert(previousIndexOfDot + 1, val);
					text.CaretIndex = previousIndexOfDot + 1;
				}
				else if (countOfDot < 3)
				{
					this.HandleTextInput(e, text.CaretIndex, ".", text);
					this.HandleTextInput(e, text.CaretIndex, val, text);
					oldValue = text.Text;
				}
			}
			else
			{
				this.HandleTextInput(e, nextIndexOfDot + 1, val, text);
				text.CaretIndex--;
				oldValue = text.Text;
			}
			return oldValue;
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0003AF98 File Offset: 0x00039198
		private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			string enteredValue = e.Text;
			this.HandleTextInput(e, textBox.CaretIndex, enteredValue, textBox);
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0003AFC4 File Offset: 0x000391C4
		private static bool ValidIpFragment(string textVal, out bool moveToNext)
		{
			moveToNext = false;
			string[] array = textVal.Split(new char[] { '.' });
			int count = 0;
			foreach (string item in array)
			{
				if (!string.IsNullOrEmpty(item) && int.Parse(item) > 255)
				{
					if (count < 3)
					{
						moveToNext = true;
					}
					return false;
				}
				count++;
			}
			return true;
		}

		// Token: 0x04000351 RID: 849
		private const string IpRegex = "([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\\.([0-9]|[0-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]|[*])";
	}
}
