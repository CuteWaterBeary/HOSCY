﻿using Hoscy.Services.Speech;
using Hoscy.Services.Speech.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hoscy.Ui.Pages
{
    /// <summary>
    /// Interaction logic for PageOutput.xaml
    /// </summary>
    internal partial class PageOutput : Page
    {
        private bool _changedValuesSynth = false;

        public PageOutput()
        {
            InitializeComponent();
            UpdateTimeoutBoxes();
            LoadBoxes();
            UpdateVolumeText();
            UpdateChangeIndicatorSynth();
        }

        #region Loading
        private void LoadBoxes()
        {
            //Speakers
            speechSpeakerBox.Load(Devices.Speakers.Select(x => x.ProductName), Devices.GetSpeakerIndex(Config.Speech.SpeakerId));
            //Windows Synths
            speechWindowsSynthBox.Load(Devices.WindowsVoices.Select(x => x.Description), Devices.GetWindowsVoiceIndex(Config.Speech.TtsId));

            UpdateAzureVoiceBox();
        }

        private void UpdateTimeoutBoxes()
        {
            optionDefaultTimeout.IsEnabled = !textboxDynamicTimeout.IsChecked ?? false;
            optionDynamicTimeout.IsEnabled = textboxDynamicTimeout.IsChecked ?? false;
        }

        private void UpdateAzureVoiceBox()
        {
            var voices = Config.Api.AzureVoices;

            //Checking for availability of current model in dropdown
            int index = -1;
            var keyArray = voices.Keys.ToArray();
            for (int i = 0; i < keyArray.Length; i++)
            {
                if (Config.Api.AzureVoiceCurrent == keyArray[i])
                {
                    index = i;
                    break;
                }
            }

            //Clearing, very cool
            azureVoiceBox.ItemsSource = null;
            foreach (var item in azureVoiceBox.Items)
                azureVoiceBox.Items.Remove(item);
            azureVoiceBox.Items.Refresh();

            azureVoiceBox.Load(voices.Keys, index, true);
        }

        private void UpdateVolumeText()
        {
            if (volumeLabel != null)
                volumeLabel.Content = $"Speech volume ({(int)Math.Round(volumeSlider.Value * 100)}%)";
        }

        private void UpdateChangeIndicatorSynth()
            => changeIndicatorSynth.Visibility = _changedValuesSynth ? Visibility.Visible : Visibility.Hidden;
        #endregion

        #region Buttons
        private void Button_SkipBox(object sender, RoutedEventArgs e)
            => Textbox.Clear();

        private void Button_SkipSpeech(object sender, RoutedEventArgs e)
            => Synthesizing.Skip();

        private void Button_ResetDevice(object sender, RoutedEventArgs e)
        {
            Config.Speech.SpeakerId = string.Empty;
            LoadBoxes();
        }

        private void Button_ReloadSynthesizer(object sender, RoutedEventArgs e)
        {
            Synthesizing.ReloadSynth();
            _changedValuesSynth = false;
            UpdateChangeIndicatorSynth();
        }

        private void Button_EditAzureVoices(object sender, RoutedEventArgs e)
        {
            UiHelper.OpenDictionaryEditor("Edit Azure Voices", "Voice Identifier", "Voice Name", Config.Api.AzureVoices);
            UpdateAzureVoiceBox();
        }
        #endregion

        #region SelectionChanged
        private void SpeechSpeakerBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Speech.SpeakerId = Devices.Speakers[speechSpeakerBox.SelectedIndex].ProductName;
            Synthesizing.ChangeSpeakers();
        }

        private void SpeechWindowsSynthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldId = Config.Speech.TtsId;
            Config.Speech.TtsId = Devices.WindowsVoices[speechWindowsSynthBox.SelectedIndex].Id;

            if (oldId != Config.Speech.TtsId)
            {
                _changedValuesSynth = true;
                UpdateChangeIndicatorSynth();
            }
        }

        private void AzureVoiceBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string oldVoiceName = Config.Api.AzureVoiceCurrent;

            var index = azureVoiceBox.SelectedIndex;
            if (index != -1 && index < Config.Api.AzureVoices.Count)
                Config.Api.AzureVoiceCurrent = Config.Api.AzureVoices.Keys.ToArray()[index];

            if (oldVoiceName != Config.Api.AzureVoiceCurrent)
            {
                _changedValuesSynth = true;
                UpdateChangeIndicatorSynth();
            }
        }
        #endregion

        #region Other
        private void Slider_Volume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volumeSlider != null)
            {
                UpdateVolumeText();
                Synthesizing.ChangeVolume();
            }
        }

        private void TextboxDynamicTimeout_Checked(object sender, RoutedEventArgs e)
            => UpdateTimeoutBoxes();

        private void SynthTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) //todo: synth status
        {
            _changedValuesSynth = true;
            UpdateChangeIndicatorSynth();
        }
        #endregion
    }
}
