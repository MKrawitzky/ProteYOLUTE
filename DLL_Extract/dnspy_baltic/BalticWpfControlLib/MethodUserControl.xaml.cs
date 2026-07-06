// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Microsoft.CSharp.RuntimeBinder;

namespace BalticWpfControlLib
{
	// Token: 0x0200002C RID: 44
	public partial class MethodUserControl : UserControl
	{
		// Auto-generated callsite cache class
		private static class _co_66
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
			public static dynamic _cp_4;
			public static dynamic _cp_5;
			public static dynamic _cp_6;
			public static dynamic _cp_7;
		}

		// Token: 0x1400002D RID: 45
		// (add) Token: 0x0600026E RID: 622 RVA: 0x00010180 File Offset: 0x0000E380
		// (remove) Token: 0x0600026F RID: 623 RVA: 0x000101B8 File Offset: 0x0000E3B8
		public event MethodUserControl.ValidateInputUpdate ValidateInputUpdateEvent;

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x06000270 RID: 624 RVA: 0x000101F0 File Offset: 0x0000E3F0
		// (remove) Token: 0x06000271 RID: 625 RVA: 0x00010228 File Offset: 0x0000E428
		public event MethodUserControl.EnableMethodComplete EnableMethodCompleteEvent;

		// Token: 0x1400002F RID: 47
		// (add) Token: 0x06000272 RID: 626 RVA: 0x00010260 File Offset: 0x0000E460
		// (remove) Token: 0x06000273 RID: 627 RVA: 0x00010298 File Offset: 0x0000E498
		public event MethodUserControl.TrapSelectionWarning TrapSelectionWarningEvent;

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000274 RID: 628 RVA: 0x000102CD File Offset: 0x0000E4CD
		private bool IsMethodDataValid
		{
			get
			{
				return this._isTrapColEquilValid && this._isAnalColEquilValid && this._isSampleLoadValid && this._isGradTableValid && this._isGradMainValid && this._isAdvSettingsValid;
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000275 RID: 629 RVA: 0x000102FF File Offset: 0x0000E4FF
		private bool IsOverrideSettingsValid
		{
			get
			{
				return this._isTrapColEquilValid && this._isAnalColEquilValid && this._isSampleLoadValid && this._isAdvSettingsValid;
			}
		}

		// Token: 0x06000276 RID: 630 RVA: 0x00010324 File Offset: 0x0000E524
		public MethodUserControl(Window owner, BalticMethod method, BalticInstrumentFacade facade, BalticColumnList columns, ColumnSelections columnSelections, bool isPressurePSI = false, bool isOvenDetected = false)
		{
			this._owner = owner;
			this._method = new BindableBalticMethod(method, facade);
			this._facade = facade;
			this._columns = columns;
			this._columnSelections = columnSelections;
			this._isPressurePSI = isPressurePSI;
			this._isOvenDetected = isOvenDetected;
			this._isService = BalticInstrumentFacade.IsService;
			this.InitializeComponent();
			this._myResDictionary.Source = new Uri("pack://application:,,,/BalticWpfControlLib;component/Resources/BrukerIcons.xaml", UriKind.RelativeOrAbsolute);
			if (this._method.ElutionName != null)
			{
				this.UpdateExperimentInfo();
				this.CheckExistingMethod();
			}
			this.UpdateMethodDisplay();
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0001041C File Offset: 0x0000E61C
		private void UpdateExperimentInfo()
		{
			this._experiment = new ExperimentInfo
			{
				ElutionName = this._method.ElutionName,
				AnalysisTime = TimeSpan.FromMinutes(this._method.GradientTime),
				OvenTemperature = this._method.OvenTemperature,
				AppKey = this._facade.AppKey
			};
			if (this._method.UsesTrapColumn)
			{
				Column column = this._columns.Find((Column item) => item.Name == this._method.TrapColumnName);
				if (column != null)
				{
					this._experiment.Trap = new ColumnAdapter(column);
				}
				else
				{
					MessageBox.Show(this._owner, "Method trap column \"" + this._method.TrapColumnName + "\" cannot be found in the system column list - Reset Method, or Select a column and Adapt !", "Error - Unknown Column Name ", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
			if (this._method.UsesSepColumn)
			{
				Column column2 = this._columns.Find((Column item) => item.Name == this._method.SeparationColumnName);
				if (column2 != null)
				{
					this._experiment.Separator = new ColumnAdapter(column2);
					return;
				}
				MessageBox.Show(this._owner, "Method separation column \"" + this._method.SeparationColumnName + "\" cannot be found in the system column list - Reset Method, or Select a column and Adapt !", "Error - Unknown Column Name ", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		// Token: 0x06000278 RID: 632 RVA: 0x00010550 File Offset: 0x0000E750
		private void UpdateMethodDisplay()
		{
			this._ucGradMain = new GradientMainUserControl(this._method, this._facade, this._columns, this._columnSelections, this._isOvenDetected);
			this._ucGradMain.GetInitialMethodParamEvent += this.ucGradMain_GetInitialMethodParamEvent;
			this._ucGradMain.GenerateMethodEvent += this.ucGradMain_GenerateMethodEvent;
			this._ucGradMain.EnableMethodControlsEvent += this.ucGradMain_EnableMethodControlsEvent;
			this._ucGradMain.ValidationUpdateEvent += this.ucGradMain_UpdateInputValidation;
			this._ucGradMain.TrapSelectionWarningEvent += this.ucGradMain_TrapSelectionWarning;
			Canvas.SetLeft(this._ucGradMain, 0.0);
			Canvas.SetTop(this._ucGradMain, 0.0);
			this.cvMethodEditor.Children.Add(this._ucGradMain);
			this._ucGradTable = new GradientTableUserControl(this._method, this._facade, this._experiment);
			this._ucGradTable.Width = (double)this._ucGradTable.DesignWidth;
			this._ucGradTable.Height = this._ucGradMain.Height;
			this._ucGradTable.GradientMainUpdateEvent += this.ucGradTable_GradientMainUpdateEvent;
			this._ucGradTable.ValidationUpdateEvent += this.ucGradTable_UpdateInputValidation;
			Canvas.SetLeft(this._ucGradTable, this._ucGradMain.Width - 5.0);
			Canvas.SetTop(this._ucGradTable, 0.0);
			this.cvMethodEditor.Children.Add(this._ucGradTable);
			this._expSettings = new Expander();
			StackPanel expPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal
			};
			TextBlock tbHeader = new TextBlock
			{
				Foreground = new SolidColorBrush(Colors.SteelBlue),
				FontSize = 10.0,
				Text = "OVERRIDE DEFAULT SETTINGS"
			};
			this._tbModifiedAll = new TextBlock
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Text = "",
				Foreground = new SolidColorBrush(Colors.Red),
				FontSize = 8.0,
				FontStyle = FontStyles.Italic
			};
			this._imgAllErrorNotify = new Image
			{
				Height = 12.0,
				Width = 12.0,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
				Source = (this._myResDictionary["IconErrorRed"] as DrawingImage),
				ToolTip = "Override Settings Error(s)",
				Visibility = Visibility.Hidden
			};
			expPanel.Children.Add(tbHeader);
			expPanel.Children.Add(this._tbModifiedAll);
			expPanel.Children.Add(this._imgAllErrorNotify);
			this._expSettings.Header = expPanel;
			this._expSettings.Width = this._ucGradMain.Width + this._ucGradTable.Width - 9.0;
			this._expSettings.BorderBrush = new SolidColorBrush(Colors.LightGray);
			this._expGrid = new Grid
			{
				Margin = new Thickness(4.0, 4.0, 0.0, 4.0)
			};
			ColumnDefinition column0 = new ColumnDefinition
			{
				Width = new GridLength(this._expSettings.Width * 0.45, GridUnitType.Pixel)
			};
			this._expGrid.ColumnDefinitions.Add(column0);
			ColumnDefinition column = new ColumnDefinition
			{
				Width = new GridLength(this._expSettings.Width * 0.55 - 12.0, GridUnitType.Pixel)
			};
			this._expGrid.ColumnDefinitions.Add(column);
			for (int i = 0; i < 5; i++)
			{
				RowDefinition newRow = new RowDefinition
				{
					Height = new GridLength(38.0)
				};
				this._expGrid.RowDefinitions.Add(newRow);
			}
			this.UpdateOverrideControls(false);
			this.UpdateGradientTableControls();
			this._expSettings.Content = this._expGrid;
			this._expSettings.Collapsed += this.expSettings_Collapsed;
			this._expSettings.Expanded += this.expSettings_Expanded;
			Canvas.SetLeft(this._expSettings, 2.0);
			Canvas.SetTop(this._expSettings, this._ucGradMain.Height + 5.0);
			this.cvMethodEditor.Children.Add(this._expSettings);
			base.Height = 505.0;
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00010A39 File Offset: 0x0000EC39
		private void UpdateGradientTableControls()
		{
			this._ucGradTable.UpdateGradientTableControls();
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00010A48 File Offset: 0x0000EC48
		private void UpdateOverrideControls(bool isKeepGradient = false)
		{
			if (isKeepGradient)
			{
				TrapColEquilUserControl trapUc = this._activeUc as TrapColEquilUserControl;
				if (trapUc != null)
				{
					trapUc.RefreshParameters(this._experiment, this._method);
					return;
				}
				AnalyticalColEquilUserControl analyticalColEquilUserControl = this._activeUc as AnalyticalColEquilUserControl;
				if (analyticalColEquilUserControl != null)
				{
					analyticalColEquilUserControl.RefreshParameters(this._experiment, this._method);
					return;
				}
				SampleLoadingUserControl sampleLoadingUserControl = this._activeUc as SampleLoadingUserControl;
				if (sampleLoadingUserControl != null)
				{
					sampleLoadingUserControl.RefreshParameters(this._experiment, this._method);
					return;
				}
				AdvParamSettingsUserControl advUc = this._activeUc as AdvParamSettingsUserControl;
				if (advUc != null)
				{
					advUc.RefreshParameters(this._experiment, this._method);
					return;
				}
			}
			else
			{
				if (this._method.ElutionName == null)
				{
					this._expGrid.Children.Clear();
					this._expSettings.Visibility = Visibility.Hidden;
					return;
				}
				this._activeUc = null;
				if (this._method.UsesTrapColumn)
				{
					StackPanel trapPanel = new StackPanel();
					TextBlock tbTrapLeft = new TextBlock();
					trapPanel.Orientation = Orientation.Horizontal;
					tbTrapLeft.Text = "Trap Column Equilibration";
					this._tbTrapModified = new TextBlock
					{
						Text = "",
						Foreground = new SolidColorBrush(Colors.Red),
						FontSize = 8.0,
						FontStyle = FontStyles.Italic,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center
					};
					this._imgTrapErrorNotify = new Image
					{
						Height = 12.0,
						Width = 12.0,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
						Source = (this._myResDictionary["IconErrorRed"] as DrawingImage),
						ToolTip = "Trap Column Equilibration Error(s)",
						Visibility = Visibility.Hidden
					};
					trapPanel.Children.Add(tbTrapLeft);
					trapPanel.Children.Add(this._tbTrapModified);
					trapPanel.Children.Add(this._imgTrapErrorNotify);
					this._btnTrapColEquil.GroupName = "Overrides";
					this._btnTrapColEquil.Content = trapPanel;
					this._btnTrapColEquil.HorizontalContentAlignment = HorizontalAlignment.Left;
					this._btnTrapColEquil.Width = this._expSettings.Width * 0.45;
					this._btnTrapColEquil.Height = 38.0;
					this._btnTrapColEquil.Margin = new Thickness(0.0);
					this._btnTrapColEquil.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
					this._btnTrapColEquil.Click += this.gridChild_Click;
					Grid.SetRow(this._btnTrapColEquil, this._expGrid.Children.Count);
					Grid.SetColumn(this._btnTrapColEquil, 0);
					this._expGrid.Children.Add(this._btnTrapColEquil);
					if (this._activeUc == null)
					{
						TrapColEquilUserControl userControl = new TrapColEquilUserControl(this._method, this._facade, this._isPressurePSI, this._experiment);
						userControl.ValidationUpdateEvent += this.ucTrapColEquil_UpdateInputValidation;
						userControl.ModificationUpdateEvent += this.ucTrapColEquil_ModificationUpdate;
						this._activeUc = userControl;
						this._activeBtn = this._btnTrapColEquil;
					}
				}
				if (this._method.UsesSepColumn)
				{
					StackPanel sepPanel = new StackPanel();
					TextBlock tbSepLeft = new TextBlock();
					sepPanel.Orientation = Orientation.Horizontal;
					tbSepLeft.Text = "Separation Column Equilibration";
					this._tbSepModified = new TextBlock
					{
						Text = "",
						Foreground = new SolidColorBrush(Colors.Red),
						FontSize = 8.0,
						FontStyle = FontStyles.Italic,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center
					};
					this._imgSepErrorNotify = new Image
					{
						Height = 12.0,
						Width = 12.0,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
						Source = (this._myResDictionary["IconErrorRed"] as DrawingImage),
						ToolTip = "Separation Column Equilibration Error(s)",
						Visibility = Visibility.Hidden
					};
					sepPanel.Children.Add(tbSepLeft);
					sepPanel.Children.Add(this._tbSepModified);
					sepPanel.Children.Add(this._imgSepErrorNotify);
					this._btnSepColEquil.GroupName = "Overrides";
					this._btnSepColEquil.Content = sepPanel;
					this._btnSepColEquil.HorizontalContentAlignment = HorizontalAlignment.Left;
					this._btnSepColEquil.Width = this._expSettings.Width * 0.45;
					this._btnSepColEquil.Height = 38.0;
					this._btnSepColEquil.Margin = new Thickness(0.0);
					this._btnSepColEquil.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
					this._btnSepColEquil.Click += this.gridChild_Click;
					Grid.SetRow(this._btnSepColEquil, this._expGrid.Children.Count);
					Grid.SetColumn(this._btnSepColEquil, 0);
					this._expGrid.Children.Add(this._btnSepColEquil);
					if (this._activeUc == null)
					{
						AnalyticalColEquilUserControl userControl2 = new AnalyticalColEquilUserControl(this._method, this._facade, this._isPressurePSI, this._experiment);
						userControl2.ValidationUpdateEvent += this.ucAnalyticalColEquil_UpdateInputValidation;
						userControl2.ModificationUpdateEvent += this.ucAnalyticalColEquil_ModificationUpdate;
						this._activeUc = userControl2;
						this._activeBtn = this._btnSepColEquil;
					}
				}
				if (this._method.UsesTrapColumn | this._method.UsesSepColumn)
				{
					StackPanel loadPanel = new StackPanel();
					TextBlock tbLoadLeft = new TextBlock();
					loadPanel.Orientation = Orientation.Horizontal;
					tbLoadLeft.Text = "Sample Loading";
					this._tbLoadModified = new TextBlock
					{
						Text = "",
						Foreground = new SolidColorBrush(Colors.Red),
						FontSize = 8.0,
						FontStyle = FontStyles.Italic,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center
					};
					this._imgLoadErrorNotify = new Image
					{
						Height = 12.0,
						Width = 12.0,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
						Source = (this._myResDictionary["IconErrorRed"] as DrawingImage),
						ToolTip = "Sample Loading Errors(s)",
						Visibility = Visibility.Hidden
					};
					loadPanel.Children.Add(tbLoadLeft);
					loadPanel.Children.Add(this._tbLoadModified);
					loadPanel.Children.Add(this._imgLoadErrorNotify);
					this._btnSampleLoad.GroupName = "Overrides";
					this._btnSampleLoad.Content = loadPanel;
					this._btnSampleLoad.HorizontalContentAlignment = HorizontalAlignment.Left;
					this._btnSampleLoad.Width = this._expSettings.Width * 0.45;
					this._btnSampleLoad.Height = 38.0;
					this._btnSampleLoad.Margin = new Thickness(0.0);
					this._btnSampleLoad.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
					this._btnSampleLoad.Click += this.gridChild_Click;
					Grid.SetRow(this._btnSampleLoad, this._expGrid.Children.Count);
					Grid.SetColumn(this._btnSampleLoad, 0);
					this._expGrid.Children.Add(this._btnSampleLoad);
				}
				StackPanel advPanel = new StackPanel();
				TextBlock tbAdvLeft = new TextBlock();
				advPanel.Orientation = Orientation.Horizontal;
				tbAdvLeft.Text = "Advanced Settings";
				this._tbAdvModified = new TextBlock
				{
					Text = "",
					Foreground = new SolidColorBrush(Colors.Red),
					FontSize = 8.0,
					FontStyle = FontStyles.Italic,
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center
				};
				this._imgAdvErrorNotify = new Image
				{
					Height = 12.0,
					Width = 12.0,
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center,
					Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
					Source = (this._myResDictionary["IconErrorRed"] as DrawingImage),
					ToolTip = "Advanced Settings Error(s)",
					Visibility = Visibility.Hidden
				};
				advPanel.Children.Add(tbAdvLeft);
				advPanel.Children.Add(this._tbAdvModified);
				advPanel.Children.Add(this._imgAdvErrorNotify);
				this._btnAdvancedSett.GroupName = "Overrides";
				this._btnAdvancedSett.Content = advPanel;
				this._btnAdvancedSett.HorizontalContentAlignment = HorizontalAlignment.Left;
				this._btnAdvancedSett.Width = this._expSettings.Width * 0.45;
				this._btnAdvancedSett.Height = 38.0;
				this._btnAdvancedSett.Margin = new Thickness(0.0);
				this._btnAdvancedSett.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
				this._btnAdvancedSett.Click += this.gridChild_Click;
				Grid.SetRow(this._btnAdvancedSett, this._expGrid.Children.Count);
				Grid.SetColumn(this._btnAdvancedSett, 0);
				this._expGrid.Children.Add(this._btnAdvancedSett);
				Border fill = new Border
				{
					Width = this._expSettings.Width * 0.45,
					Margin = new Thickness(0.0),
					BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0),
					BorderBrush = new SolidColorBrush(Colors.SlateGray),
					Height = double.NaN
				};
				Grid.SetRow(fill, this._expGrid.Children.Count);
				Grid.SetColumn(fill, 0);
				Grid.SetRowSpan(fill, this._expGrid.RowDefinitions.Count - this._expGrid.Children.Count);
				this._expGrid.Children.Add(fill);
				if (this._activeUc != null)
				{
					this._ucWidth = this._expSettings.Width - this._btnSampleLoad.Width - 12.0;
					this._ucHeight = this._btnSampleLoad.Height * 5.0;
					this._activeUc.Width = this._ucWidth;
					this._activeUc.Height = this._ucHeight;
					this._activeUc.HorizontalAlignment = HorizontalAlignment.Center;
					Grid.SetRow(this._activeUc, 0);
					Grid.SetColumn(this._activeUc, 1);
					Grid.SetRowSpan(this._activeUc, 5);
					this._expGrid.Children.Add(this._activeUc);
					this.SetOverridePressedState();
				}
				if (this._method.UsesTrapColumn | this._method.UsesSepColumn)
				{
					this._expSettings.Visibility = Visibility.Visible;
				}
				else
				{
					this._ucWidth = this._expSettings.Width - 12.0;
					this._ucHeight = 38.0;
					this._expSettings.IsExpanded = false;
					this._expSettings.Visibility = Visibility.Hidden;
				}
				this.InitialCheckForMethodModification();
			}
		}

		// Token: 0x0600027B RID: 635 RVA: 0x000116FC File Offset: 0x0000F8FC
		private void InitialCheckForMethodModification()
		{
			if (this._method.UsesSepColumn)
			{
				this.ucAnalyticalColEquil_ModificationUpdate(Math.Abs(this._method.SeparationColumnEquil.Scale - this._method.SeparationColumnEquil.DefaultScale) > 0.0001 || (int)(this._method.SeparationColumnEquil.Pressure * 100.0) != (int)(this._method.SeparationColumnEquil.DefaultPressure * 100.0));
			}
			if (this._method.UsesTrapColumn)
			{
				this.ucTrapColEquil_ModificationUpdate(Math.Abs(this._method.TrapColumnEquil.Scale - this._method.TrapColumnEquil.DefaultScale) > 0.0001 || (int)(this._method.TrapColumnEquil.Pressure * 100.0) != (int)(this._method.TrapColumnEquil.DefaultPressure * 100.0));
			}
			if (this._method.UsesSepColumn || this._method.UsesTrapColumn)
			{
				this.ucSampleLoading_ModificationUpdate(Math.Abs(this._method.SampleLoading.Scale - this._method.SampleLoading.DefaultScale) > 0.0001 || (int)(this._method.SampleLoading.Pressure * 100.0) != (int)(this._method.SampleLoading.DefaultPressure * 100.0) || this._method.SampleLoading.InjectionMethod != this._method.SampleLoading.DefaultInjectionMethod || this._method.SampleLoading.IsBottomSense != this._method.SampleLoading.DefaultIsBottomSense);
			}
			bool isMod = false;
			foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter item in this._method.AdvancedSettings.Parameters)
			{
				object obj = item.Value;
				if (obj is bool)
				{
					bool b = (bool)obj;
					if (b != (bool)item.DefaultValue)
					{
						isMod = true;
						break;
					}
				}
				else
				{
					obj = item.Value;
					if (obj is double)
					{
						double d = (double)obj;
						if ((int)(d * 1000.0) != (int)((double)item.DefaultValue * 1000.0))
						{
							isMod = true;
							break;
						}
					}
					else
					{
						obj = item.Value;
						if (obj is int)
						{
							int i = (int)obj;
							if (i != (int)item.DefaultValue)
							{
								isMod = true;
								break;
							}
						}
						else
						{
							string s = item.Value as string;
							if (s != null && s != (string)item.DefaultValue)
							{
								isMod = true;
								break;
							}
						}
					}
				}
			}
			foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item2 in this._method.AdvancedSettings.ChildParameters)
			{
				object obj = item2.Value;
				if (obj is bool)
				{
					bool b2 = (bool)obj;
					if (b2 != (bool)item2.DefaultValue)
					{
						isMod = true;
						break;
					}
				}
				else
				{
					obj = item2.Value;
					if (obj is double)
					{
						double d2 = (double)obj;
						if ((int)(d2 * 1000.0) != (int)((double)item2.DefaultValue * 1000.0))
						{
							isMod = true;
							break;
						}
					}
					else
					{
						obj = item2.Value;
						if (obj is int)
						{
							int j = (int)obj;
							if (j != (int)item2.DefaultValue)
							{
								isMod = true;
								break;
							}
						}
						else
						{
							string s2 = item2.Value as string;
							if (s2 != null && s2 != (string)item2.DefaultValue)
							{
								isMod = true;
								break;
							}
						}
					}
				}
			}
			this.ucAdvancedSett_ModificationUpdate(isMod);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00011B2C File Offset: 0x0000FD2C
		private void CheckModifiedAll()
		{
			bool trapModified = this._method.UsesTrapColumn && this._tbTrapModified.Text == "(MODIFIED)";
			bool separatorModified = this._method.UsesSepColumn && this._tbSepModified.Text == "(MODIFIED)";
			bool loadModified = (this._method.UsesSepColumn || this._method.UsesTrapColumn) && this._tbLoadModified.Text == "(MODIFIED)";
			bool advModified = this._tbAdvModified.Text == "(MODIFIED)";
			this._tbModifiedAll.Text = ((trapModified || separatorModified || loadModified || advModified) ? "(MODIFIED)" : "");
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00011BF0 File Offset: 0x0000FDF0
		private void gridChild_Click(object sender, RoutedEventArgs e)
		{
			if (this._activeUc != null)
			{
				this._expGrid.Children.Remove(this._activeUc);
			}
			RadioButton childButton = sender as RadioButton;
			if (childButton == this._btnTrapColEquil)
			{
				TrapColEquilUserControl userControl = new TrapColEquilUserControl(this._method, this._facade, this._isPressurePSI, this._experiment)
				{
					Height = this._ucHeight,
					Width = this._ucWidth
				};
				userControl.ValidationUpdateEvent += this.ucTrapColEquil_UpdateInputValidation;
				userControl.ModificationUpdateEvent += this.ucTrapColEquil_ModificationUpdate;
				this._activeBtn = this._btnTrapColEquil;
				this._activeUc = userControl;
				Grid.SetRow(userControl, 0);
				Grid.SetColumn(userControl, 1);
				Grid.SetRowSpan(userControl, 5);
				this._expGrid.Children.Add(userControl);
				this.SetOverridePressedState();
				return;
			}
			if (childButton == this._btnSepColEquil)
			{
				AnalyticalColEquilUserControl userControl2 = new AnalyticalColEquilUserControl(this._method, this._facade, this._isPressurePSI, this._experiment)
				{
					Height = this._ucHeight,
					Width = this._ucWidth
				};
				userControl2.ValidationUpdateEvent += this.ucAnalyticalColEquil_UpdateInputValidation;
				userControl2.ModificationUpdateEvent += this.ucAnalyticalColEquil_ModificationUpdate;
				this._activeBtn = this._btnSepColEquil;
				this._activeUc = userControl2;
				Grid.SetRow(userControl2, 0);
				Grid.SetColumn(userControl2, 1);
				Grid.SetRowSpan(userControl2, 5);
				this._expGrid.Children.Add(userControl2);
				this.SetOverridePressedState();
				return;
			}
			if (childButton == this._btnSampleLoad)
			{
				SampleLoadingUserControl userControl3 = new SampleLoadingUserControl(this._method, this._facade, this._isPressurePSI, this._experiment)
				{
					Height = this._ucHeight,
					Width = this._ucWidth
				};
				userControl3.ValidationUpdateEvent += this.ucSampleLoading_UpdateInputValidation;
				userControl3.ModificationUpdateEvent += this.ucSampleLoading_ModificationUpdate;
				this._activeBtn = this._btnSampleLoad;
				this._activeUc = userControl3;
				Grid.SetRow(userControl3, 0);
				Grid.SetColumn(userControl3, 1);
				Grid.SetRowSpan(userControl3, 5);
				this._expGrid.Children.Add(userControl3);
				this.SetOverridePressedState();
				return;
			}
			if (childButton == this._btnAdvancedSett)
			{
				AdvParamSettingsUserControl userControl4 = new AdvParamSettingsUserControl(this._method, this._facade, this._experiment, this._isService)
				{
					Height = this._ucHeight,
					Width = this._ucWidth
				};
				userControl4.ModificationUpdateEvent += this.ucAdvancedSett_ModificationUpdate;
				userControl4.ValidationUpdateEvent += this.ucAdvancedSett_ValidationUpdateEvent;
				this._activeBtn = this._btnAdvancedSett;
				this._activeUc = userControl4;
				Grid.SetRow(userControl4, 0);
				Grid.SetColumn(userControl4, 1);
				Grid.SetRowSpan(userControl4, 5);
				this._expGrid.Children.Add(userControl4);
				this.SetOverridePressedState();
			}
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00011EC8 File Offset: 0x000100C8
		private void CheckExistingMethod()
		{
			ProcedureInfo info = this._facade.GetElutionProcedure(this._method.ElutionName);
			if (info != null)
			{
				ProcedureArguments advArguments = info.CreateAdvancedArguments();
				ChildProcedureArguments advChildArguments = info.CreateAdvancedChildArguments();
				using (IEnumerator<ProcedureArgument> enumerator = advArguments.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ProcedureArgument item2 = enumerator.Current;
						if (this._method.AdvancedSettings.Parameters.Find((BindableBalticMethod.AdvancedSett.AdvancedParameter x) => x.Name == item2.Name) == null)
						{
							this._method.AdvancedSettings.Parameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedParameter(item2.Name, item2.Value, item2.Value, item2.Unit));
						}
					}
				}
				using (List<ChildProcedureArgument>.Enumerator enumerator2 = advChildArguments.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ChildProcedureArgument item = enumerator2.Current;
						BindableBalticMethod.AdvancedSett.AdvancedChildParameter childArgument = this._method.AdvancedSettings.ChildParameters.Find((BindableBalticMethod.AdvancedSett.AdvancedChildParameter x) => x.Name == item.Name && x.Header == item.Header);
						if (childArgument == null)
						{
							this._method.AdvancedSettings.ChildParameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedChildParameter(item.Header, item.ProcArg.Name, item.ProcArg.Value, item.ProcArg.Value, item.ProcArg.Unit, item.IsService));
						}
						else
						{
							childArgument.IsService = item.IsService;
						}
					}
				}
				this._method.ToBalticMethod(this._columns);
			}
		}

		// Token: 0x0600027F RID: 639 RVA: 0x000120C4 File Offset: 0x000102C4
		private void GenerateMethod(bool isKeepGradient = false, bool isKeepAdvancedSettings = false)
		{
			ProcedureInfo elutionProcedure = this._facade.GetElutionProcedure(this._method.ElutionName);
			ProcedureArguments arguments = elutionProcedure.CreateArguments();
			ProcedureArguments advArguments = elutionProcedure.CreateAdvancedArguments();
			ChildProcedureArguments advChildArguments = elutionProcedure.CreateAdvancedChildArguments();
			string advHeader = elutionProcedure.AdvHeader;
			int[] advHeaderFgColor = elutionProcedure.AdvHeaderFgColor;
			this._experiment = new ExperimentInfo
			{
				ElutionName = this._method.ElutionName,
				AnalysisTime = TimeSpan.FromMinutes(this._method.GradientTime),
				OvenTemperature = this._method.OvenTemperature,
				IsKeepGradient = isKeepGradient,
				IsKeepAdvancedSettings = isKeepAdvancedSettings,
				AppKey = this._facade.AppKey
			};
			if ((bool)arguments["uses_trap"].Value)
			{
				Column column = this._columns.Find((Column item) => item.Name == this._method.TrapColumnName);
				this._experiment.Trap = new ColumnAdapter(column);
			}
			else
			{
				Column column2 = this._columns.Find((Column item) => item.Name == this._method.TrapColumnName) ?? this._columns.Find(delegate(Column item)
				{
					if (MethodUserControl._co_66._cp_3 == null)
					{
						MethodUserControl._co_66._cp_3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(MethodUserControl)));
					}
					Func<CallSite, object, bool> target = MethodUserControl._co_66._cp_3.Target;
					CallSite _cpl = MethodUserControl._co_66._cp_3;
					if (MethodUserControl._co_66._cp_2 == null)
					{
						MethodUserControl._co_66._cp_2 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(MethodUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, string, object, object> target2 = MethodUserControl._co_66._cp_2.Target;
					CallSite _cp_2 = MethodUserControl._co_66._cp_2;
					string name = item.Name;
					if (MethodUserControl._co_66._cp_1 == null)
					{
						MethodUserControl._co_66._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(MethodUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target3 = MethodUserControl._co_66._cp_1.Target;
					CallSite _cp_3 = MethodUserControl._co_66._cp_1;
					if (MethodUserControl._co_66._cp_0 == null)
					{
						MethodUserControl._co_66._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(MethodUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					return target(_cpl, target2(_cp_2, name, target3(_cp_3, MethodUserControl._co_66._cp_0.Target(MethodUserControl._co_66._cp_0, this._facade.BalticConfiguration))));
				});
				this._experiment.Trap = new ColumnAdapter(column2);
			}
			if ((bool)arguments["uses_separator"].Value)
			{
				Column column3 = this._columns.Find((Column item) => item.Name == this._method.SeparationColumnName);
				this._experiment.Separator = new ColumnAdapter(column3);
			}
			else
			{
				Column column4 = this._columns.Find((Column item) => item.Name == this._method.SeparationColumnName) ?? this._columns.Find(delegate(Column item)
				{
					if (MethodUserControl._co_66._cp_7 == null)
					{
						MethodUserControl._co_66._cp_7 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(MethodUserControl)));
					}
					Func<CallSite, object, bool> target4 = MethodUserControl._co_66._cp_7.Target;
					CallSite _cp_4 = MethodUserControl._co_66._cp_7;
					if (MethodUserControl._co_66._cp_6 == null)
					{
						MethodUserControl._co_66._cp_6 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(MethodUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, string, object, object> target5 = MethodUserControl._co_66._cp_6.Target;
					CallSite _cp_5 = MethodUserControl._co_66._cp_6;
					string name2 = item.Name;
					if (MethodUserControl._co_66._cp_5 == null)
					{
						MethodUserControl._co_66._cp_5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(MethodUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target6 = MethodUserControl._co_66._cp_5.Target;
					CallSite _cp_6 = MethodUserControl._co_66._cp_5;
					if (MethodUserControl._co_66._cp_4 == null)
					{
						MethodUserControl._co_66._cp_4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(MethodUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					return target4(_cp_4, target5(_cp_5, name2, target6(_cp_6, MethodUserControl._co_66._cp_4.Target(MethodUserControl._co_66._cp_4, this._facade.BalticConfiguration))));
				});
				this._experiment.Separator = new ColumnAdapter(column4);
			}
			this._ucGradTable.Experiment = this._experiment;
			arguments = this._facade.GenerateArguments(this._experiment);
			advArguments = this._facade.GenerateAdvArguments(this._experiment);
			this._method.ToBalticMethod(this._columns).Initialize(this._experiment, arguments, advArguments, advChildArguments, advHeader, advHeaderFgColor);
			this._method.Refresh(advChildArguments);
			this.UpdateOverrideControls(isKeepGradient);
			this.UpdateGradientTableControls();
		}

		// Token: 0x06000280 RID: 640 RVA: 0x000122F0 File Offset: 0x000104F0
		public void ToBalticMethod()
		{
			this._method.ToBalticMethod(this._columns);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x00012304 File Offset: 0x00010504
		private void expSettings_Expanded(object sender, RoutedEventArgs e)
		{
			base.Height = 705.0;
			this.SetOverridePressedState();
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0001231C File Offset: 0x0001051C
		private void SetOverridePressedState()
		{
			if (this._activeBtn == this._btnTrapColEquil)
			{
				this._btnTrapColEquil.IsChecked = new bool?(true);
				return;
			}
			if (this._activeBtn == this._btnSepColEquil)
			{
				this._btnSepColEquil.IsChecked = new bool?(true);
				return;
			}
			if (this._activeBtn == this._btnSampleLoad)
			{
				this._btnSampleLoad.IsChecked = new bool?(true);
				return;
			}
			if (this._activeBtn == this._btnAdvancedSett)
			{
				this._btnAdvancedSett.IsChecked = new bool?(true);
			}
		}

		// Token: 0x06000283 RID: 643 RVA: 0x000123A8 File Offset: 0x000105A8
		private void expSettings_Collapsed(object sender, RoutedEventArgs e)
		{
			base.Height = 505.0;
		}

		// Token: 0x06000284 RID: 644 RVA: 0x000123B9 File Offset: 0x000105B9
		private void ucGradMain_TrapSelectionWarning(bool isShow, string message = "")
		{
			MethodUserControl.TrapSelectionWarning trapSelectionWarningEvent = this.TrapSelectionWarningEvent;
			if (trapSelectionWarningEvent == null)
			{
				return;
			}
			trapSelectionWarningEvent(isShow, message);
		}

		// Token: 0x06000285 RID: 645 RVA: 0x000123D0 File Offset: 0x000105D0
		private void ucGradMain_EnableMethodControlsEvent(bool isEnable, bool isReset)
		{
			if (isReset)
			{
				this._expGrid.Children.Clear();
				this._expSettings.Visibility = Visibility.Hidden;
				this._imgAllErrorNotify.Visibility = Visibility.Hidden;
				this._isTrapColEquilValid = (this._isAnalColEquilValid = (this._isSampleLoadValid = (this._isGradTableValid = (this._isGradMainValid = (this._isAdvSettingsValid = true)))));
				if (this._imgTrapErrorNotify != null)
				{
					this._imgTrapErrorNotify.Visibility = Visibility.Collapsed;
				}
				if (this._imgLoadErrorNotify != null)
				{
					this._imgLoadErrorNotify.Visibility = Visibility.Collapsed;
				}
				if (this._imgSepErrorNotify != null)
				{
					this._imgSepErrorNotify.Visibility = Visibility.Collapsed;
				}
				if (this._imgAdvErrorNotify != null)
				{
					this._imgAdvErrorNotify.Visibility = Visibility.Collapsed;
				}
				this.UpdateOverrideDefaultErrorIcon();
			}
			this._methodStateEnabled = isEnable;
			MethodUserControl.EnableMethodComplete enableMethodCompleteEvent = this.EnableMethodCompleteEvent;
			if (enableMethodCompleteEvent != null)
			{
				enableMethodCompleteEvent(isEnable);
			}
			this._expSettings.IsEnabled = isEnable;
			if (!isEnable)
			{
				this._expSettings.IsExpanded = false;
			}
			this.UpdateGradientTableControls();
		}

		// Token: 0x06000286 RID: 646 RVA: 0x000124D0 File Offset: 0x000106D0
		private void ucGradMain_GetInitialMethodParamEvent()
		{
			ProcedureArguments arguments = this._facade.GetElutionProcedure(this._method.ElutionName).CreateArguments();
			this._method.IsIsocratic = (bool)arguments["is_isocratic"].Value;
			this._method.UsesTrapColumn = (bool)arguments["uses_trap"].Value;
			this._method.UsesSepColumn = (bool)arguments["uses_separator"].Value;
			this._method.GradientTime = (double)((int)arguments["analysis_time"].Value) / 60.0;
			this._method.OvenTemperature = (double)arguments["oven_temperature"].Value;
		}

		// Token: 0x06000287 RID: 647 RVA: 0x000125A4 File Offset: 0x000107A4
		private void ucGradMain_GenerateMethodEvent(bool isKeepGradient = false, bool isKeepAdvancedSettings = false)
		{
			this.GenerateMethod(isKeepGradient, isKeepAdvancedSettings);
		}

		// Token: 0x06000288 RID: 648 RVA: 0x000125AE File Offset: 0x000107AE
		private void ucGradTable_GradientMainUpdateEvent(WPFConstants.UpdateType updateType)
		{
			this._ucGradMain.UpdateFromGradientTable(updateType);
		}

		// Token: 0x06000289 RID: 649 RVA: 0x000125BC File Offset: 0x000107BC
		private void ucTrapColEquil_UpdateInputValidation(bool isValid)
		{
			this._isTrapColEquilValid = isValid;
			this._imgTrapErrorNotify.Visibility = (this._isTrapColEquilValid ? Visibility.Collapsed : Visibility.Visible);
			this.UpdateOverrideDefaultErrorIcon();
			MethodUserControl.ValidateInputUpdate validateInputUpdateEvent = this.ValidateInputUpdateEvent;
			if (validateInputUpdateEvent == null)
			{
				return;
			}
			validateInputUpdateEvent(this.IsMethodDataValid);
		}

		// Token: 0x0600028A RID: 650 RVA: 0x000125F8 File Offset: 0x000107F8
		private void ucAnalyticalColEquil_UpdateInputValidation(bool isValid)
		{
			this._isAnalColEquilValid = isValid;
			this._imgSepErrorNotify.Visibility = (this._isAnalColEquilValid ? Visibility.Collapsed : Visibility.Visible);
			this.UpdateOverrideDefaultErrorIcon();
			MethodUserControl.ValidateInputUpdate validateInputUpdateEvent = this.ValidateInputUpdateEvent;
			if (validateInputUpdateEvent == null)
			{
				return;
			}
			validateInputUpdateEvent(this.IsMethodDataValid && this._methodStateEnabled);
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0001264C File Offset: 0x0001084C
		private void ucSampleLoading_UpdateInputValidation(bool isValid)
		{
			this._isSampleLoadValid = isValid;
			this._imgLoadErrorNotify.Visibility = (this._isSampleLoadValid ? Visibility.Collapsed : Visibility.Visible);
			this.UpdateOverrideDefaultErrorIcon();
			MethodUserControl.ValidateInputUpdate validateInputUpdateEvent = this.ValidateInputUpdateEvent;
			if (validateInputUpdateEvent == null)
			{
				return;
			}
			validateInputUpdateEvent(this.IsMethodDataValid && this._methodStateEnabled);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0001269E File Offset: 0x0001089E
		private void ucGradMain_UpdateInputValidation(bool isValid)
		{
			this._isGradMainValid = isValid;
			MethodUserControl.ValidateInputUpdate validateInputUpdateEvent = this.ValidateInputUpdateEvent;
			if (validateInputUpdateEvent == null)
			{
				return;
			}
			validateInputUpdateEvent(this.IsMethodDataValid && this._methodStateEnabled);
		}

		// Token: 0x0600028D RID: 653 RVA: 0x000126C8 File Offset: 0x000108C8
		private void UpdateOverrideDefaultErrorIcon()
		{
			this._imgAllErrorNotify.Visibility = (this.IsOverrideSettingsValid ? Visibility.Collapsed : Visibility.Visible);
		}

		// Token: 0x0600028E RID: 654 RVA: 0x000126E4 File Offset: 0x000108E4
		private void ucGradTable_UpdateInputValidation(bool isValid, string header = "", string subject = "", string message = "")
		{
			this._isGradTableValid = isValid;
			if (header != "")
			{
				this.ucAdvancedSett_ValidationUpdateEvent(false);
				AdvParamSettingsUserControl uc = this._activeUc as AdvParamSettingsUserControl;
				if (uc != null)
				{
					uc.ValidateParameters();
					return;
				}
			}
			else
			{
				this.ucAdvancedSett_ValidationUpdateEvent(true);
				this.UpdateActiveUserControl();
				MethodUserControl.ValidateInputUpdate validateInputUpdateEvent = this.ValidateInputUpdateEvent;
				if (validateInputUpdateEvent == null)
				{
					return;
				}
				validateInputUpdateEvent(this.IsMethodDataValid && this._methodStateEnabled);
			}
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00012750 File Offset: 0x00010950
		private void UpdateActiveUserControl()
		{
			AdvParamSettingsUserControl advUc = this._activeUc as AdvParamSettingsUserControl;
			if (advUc != null)
			{
				advUc.ValidateParameters();
				return;
			}
			TrapColEquilUserControl trapUc = this._activeUc as TrapColEquilUserControl;
			if (trapUc != null)
			{
				trapUc.ValidateParameters();
				return;
			}
			AnalyticalColEquilUserControl analyticalUc = this._activeUc as AnalyticalColEquilUserControl;
			if (analyticalUc != null)
			{
				analyticalUc.ValidateParameters();
				return;
			}
			SampleLoadingUserControl sampleLoadingUc = this._activeUc as SampleLoadingUserControl;
			if (sampleLoadingUc != null)
			{
				sampleLoadingUc.ValidateParameters();
			}
		}

		// Token: 0x06000290 RID: 656 RVA: 0x000127B4 File Offset: 0x000109B4
		private void ucAdvancedSett_ValidationUpdateEvent(bool isValid)
		{
			this._isAdvSettingsValid = isValid;
			this._imgAdvErrorNotify.Visibility = (this._isAdvSettingsValid ? Visibility.Collapsed : Visibility.Visible);
			this.UpdateOverrideDefaultErrorIcon();
			MethodUserControl.ValidateInputUpdate validateInputUpdateEvent = this.ValidateInputUpdateEvent;
			if (validateInputUpdateEvent == null)
			{
				return;
			}
			validateInputUpdateEvent(this.IsMethodDataValid && this._methodStateEnabled);
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00012806 File Offset: 0x00010A06
		private void ucTrapColEquil_ModificationUpdate(bool isModified)
		{
			this._tbTrapModified.Text = (isModified ? "(MODIFIED)" : "");
			this.CheckModifiedAll();
			this.UpdateGradientTableControls();
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0001282E File Offset: 0x00010A2E
		private void ucAnalyticalColEquil_ModificationUpdate(bool isModified)
		{
			this._tbSepModified.Text = (isModified ? "(MODIFIED)" : "");
			this.CheckModifiedAll();
			this.UpdateGradientTableControls();
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00012856 File Offset: 0x00010A56
		private void ucSampleLoading_ModificationUpdate(bool isModified)
		{
			this._tbLoadModified.Text = (isModified ? "(MODIFIED)" : "");
			this.CheckModifiedAll();
			this.UpdateGradientTableControls();
		}

		// Token: 0x06000294 RID: 660 RVA: 0x0001287E File Offset: 0x00010A7E
		private void ucAdvancedSett_ModificationUpdate(bool isModified)
		{
			this._tbAdvModified.Text = (isModified ? "(MODIFIED)" : "");
			this.CheckModifiedAll();
			this.UpdateGradientTableControls();
		}

		// Token: 0x0400017A RID: 378
		private const int _heightCollapsed = 505;

		// Token: 0x0400017B RID: 379
		private const int _heightExpanded = 705;

		// Token: 0x0400017C RID: 380
		private bool _methodStateEnabled;

		// Token: 0x0400017D RID: 381
		private readonly bool _isPressurePSI;

		// Token: 0x0400017E RID: 382
		private readonly bool _isOvenDetected;

		// Token: 0x0400017F RID: 383
		private bool _isTrapColEquilValid = true;

		// Token: 0x04000180 RID: 384
		private bool _isAnalColEquilValid = true;

		// Token: 0x04000181 RID: 385
		private bool _isSampleLoadValid = true;

		// Token: 0x04000182 RID: 386
		private bool _isGradTableValid = true;

		// Token: 0x04000183 RID: 387
		private bool _isGradMainValid = true;

		// Token: 0x04000184 RID: 388
		private bool _isAdvSettingsValid = true;

		// Token: 0x04000185 RID: 389
		private readonly bool _isService;

		// Token: 0x04000186 RID: 390
		private readonly BindableBalticMethod _method;

		// Token: 0x04000187 RID: 391
		private readonly BalticInstrumentFacade _facade;

		// Token: 0x04000188 RID: 392
		private Expander _expSettings;

		// Token: 0x04000189 RID: 393
		private GradientMainUserControl _ucGradMain;

		// Token: 0x0400018A RID: 394
		private GradientTableUserControl _ucGradTable;

		// Token: 0x0400018B RID: 395
		private readonly RadioButton _btnTrapColEquil = new RadioButton();

		// Token: 0x0400018C RID: 396
		private readonly RadioButton _btnSepColEquil = new RadioButton();

		// Token: 0x0400018D RID: 397
		private readonly RadioButton _btnSampleLoad = new RadioButton();

		// Token: 0x0400018E RID: 398
		private readonly RadioButton _btnAdvancedSett = new RadioButton();

		// Token: 0x0400018F RID: 399
		private TextBlock _tbTrapModified;

		// Token: 0x04000190 RID: 400
		private TextBlock _tbSepModified;

		// Token: 0x04000191 RID: 401
		private TextBlock _tbLoadModified;

		// Token: 0x04000192 RID: 402
		private TextBlock _tbAdvModified;

		// Token: 0x04000193 RID: 403
		private TextBlock _tbModifiedAll;

		// Token: 0x04000194 RID: 404
		private Image _imgTrapErrorNotify;

		// Token: 0x04000195 RID: 405
		private Image _imgSepErrorNotify;

		// Token: 0x04000196 RID: 406
		private Image _imgLoadErrorNotify;

		// Token: 0x04000197 RID: 407
		private Image _imgAdvErrorNotify;

		// Token: 0x04000198 RID: 408
		private Image _imgAllErrorNotify;

		// Token: 0x04000199 RID: 409
		private UserControl _activeUc;

		// Token: 0x0400019A RID: 410
		private RadioButton _activeBtn;

		// Token: 0x0400019B RID: 411
		private Grid _expGrid;

		// Token: 0x0400019C RID: 412
		private double _ucWidth;

		// Token: 0x0400019D RID: 413
		private double _ucHeight;

		// Token: 0x0400019E RID: 414
		private readonly Window _owner;

		// Token: 0x0400019F RID: 415
		private ExperimentInfo _experiment;

		// Token: 0x040001A0 RID: 416
		private readonly BalticColumnList _columns;

		// Token: 0x040001A1 RID: 417
		private readonly ColumnSelections _columnSelections;

		// Token: 0x040001A2 RID: 418
		private readonly ResourceDictionary _myResDictionary = new ResourceDictionary();

		// Token: 0x020000EF RID: 239
		// (Invoke) Token: 0x06000784 RID: 1924
		public delegate void ValidateInputUpdate(bool isValid);

		// Token: 0x020000F0 RID: 240
		// (Invoke) Token: 0x06000788 RID: 1928
		public delegate void EnableMethodComplete(bool isEnabled);

		// Token: 0x020000F1 RID: 241
		// (Invoke) Token: 0x0600078C RID: 1932
		public delegate void TrapSelectionWarning(bool isShow, string message = "");
	}
}
