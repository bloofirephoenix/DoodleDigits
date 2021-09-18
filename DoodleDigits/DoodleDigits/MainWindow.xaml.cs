﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DoodleDigits.Core;
using MahApps.Metro.Controls;

namespace DoodleDigits {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {

        public PresentationProperties PresentationProperties { get; }
        
        public ResultPresenter ResultPresenter { get; } = new();

        private bool initialized = false;
        // If over 0, will block saving
        private int blockSaving = 0;

        private int failedSaves = 0;

        private readonly SettingsViewModel settings;

        private void SetCaretIndex(int index) {
            this.InputTextBox.CaretIndex = index;
            this.InputTextBox.Select(index, 0);
            this.InputTextBox.Focus();
        }

        public MainWindow() {
            settings = new SettingsViewModel(new Settings());
            settings.Load().Wait();
            PresentationProperties = new(this, settings);
            InitializeComponent();

            try {
                var state = SerializedState.Load().Result;
                if (state != null) {
                    blockSaving++;
                    this.Width = state.WindowDimensions.X;
                    this.Height = state.WindowDimensions.Y;
                    this.InputTextBox.Text = state.Content;
                    SetCaretIndex(state.CursorIndex);
                    PresentationProperties.ZoomTicks = state.Zoom;
                    blockSaving--;
                }
                PresentationProperties.GridScale = PresentationProperties.Zoom;
            }
            catch {
                // ignored
            }

            initialized = true;
        }

        private DateTime lastSaveTime = DateTime.Now;
        private bool startedSave = false;

        private void AutoSave() {
            if (startedSave) {
                return;
            }

            Task InvokeSave() {
                return Dispatcher.Invoke(async () => {
                    startedSave = true;
                    lastSaveTime = DateTime.Now;
                    await SaveState();
                    startedSave = false;
                });
            }

            TimeSpan diff = lastSaveTime + new TimeSpan(0, 0, 5) - DateTime.Now;

            if (diff < new TimeSpan(0)) {
                _ = InvokeSave();
            }
            else {
                startedSave = true;
                Task.Delay(diff).ContinueWith(o => InvokeSave());
            }
        }

        private async Task SaveState() {
            if (initialized == false || blockSaving > 0) {
                return;
            }

            try {
                var state = new SerializedState(
                    this.InputTextBox.Text,
                    this.InputTextBox.CaretIndex,
                    new() { X = this.Width, Y = this.Height },
                    PresentationProperties.ZoomTicks);

                await state.Save();
                failedSaves = 0;
            }
            catch (Exception ex) {
                failedSaves++;
                if (failedSaves >= 2) {
                    MessageBox.Show("Error saving the calculator state!\n" + ex.ToString());
                }
            }

            Debug.WriteLine("Saved");
        }
        

        private async void RichTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            AutoSave();
            if (initialized == false) {
                // Delay for a little bit because of wpf wonkyness if we just started the app
                await Task.Delay(100);
            }

            string text = InputTextBox.Text;
            TextMeasure measure = new TextMeasure(text, InputTextBox);
            var calculationResult = await RunExecution(text);

            ResultPresenter.ParseResults(measure, calculationResult);
        }

        private Task<CalculationResult> RunExecution(string input) {
            var task = Task.Run(() => {
                Calculator calculator = new Calculator(FunctionLibrary.Functions, ConstantLibrary.Constants);
                return calculator.Calculate(input);
            });
            return task;

            
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            AutoSave();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (startedSave) {
                SaveState().Wait();
            }
        }


        void ClickClear(object sender, RoutedEventArgs args) { InputTextBox.Clear(); }

        private async void ToggleDarkMode(object sender, RoutedEventArgs e) {
            settings.DarkMode = !settings.DarkMode;
            if (settings.UnsavedChanges) {
                await settings.Save();
            }
        }

        private void TextBoxKeyDownEvent(object sender, KeyEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                if (e.Key == Key.OemPlus) {
                    ZoomIn(sender, e);
                }
                if (e.Key == Key.OemMinus) {
                    ZoomOut(sender, e);
                }
            }
        }
        private void ZoomOut(object sender, RoutedEventArgs e) {
            PresentationProperties.ZoomTicks --;

            if (PresentationProperties.Zoom < 0) {
                PresentationProperties.ZoomTicks ++;
            } 
            else {
                PresentationProperties.GridScale = PresentationProperties.Zoom;
                AutoSave();
            }
        }

        private void ZoomIn(object sender, RoutedEventArgs e) {
            PresentationProperties.ZoomTicks++;
            PresentationProperties.GridScale = PresentationProperties.Zoom;
            AutoSave();
        }
    }
}
