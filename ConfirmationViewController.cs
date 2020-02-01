using System;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using SongCore;

namespace MassSongRemover
{
    internal class ConfirmationViewController : BSMLResourceViewController
    {
        public override string ResourceName => "MassSongRemover.ConfirmationView.bsml";

        private bool _buttonInteractivity = false;
        [UIValue("button-interactivity")]
        public bool ButtonInteractivity
        {
            get => _buttonInteractivity;
            set
            {
                _buttonInteractivity = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("removed-folder-text")]
        public string RemovedFolder
        {
            get => SongRemover.RemovedFolder;
        }

#pragma warning disable CS0649
        [UIComponent("removed-folder-clickable-text")]
        private ClickableText _removedFolderText;
#pragma warning restore CS0649

        [UIValue("removed-folder-text-color")]
        private const string RemovedFolderTextColor = "#FFAAAA";

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);

            if (firstActivation)
            {
                this.name = "ConfirmationViewController";

                if (_removedFolderText != null && ColorUtility.TryParseHtmlString(RemovedFolderTextColor, out Color color))
                    _removedFolderText.defaultColor = color;
            }

            ButtonInteractivity = true;
        }

        [UIAction("remove-button-clicked")]
        private void RemoveButtonClicked()
        {
            SongRemover.DeleteNonFavouritedSongs();
            Loader.Instance.RefreshSongs();

            CancelButtonClicked();
        }

        [UIAction("cancel-button-clicked")]
        private void CancelButtonClicked()
        {
            ButtonInteractivity = false;
            BeatSaberUI.MainFlowCoordinator.InvokeMethod("DismissViewController", this, null, false);
        }

        [UIAction("removed-folder-clicked")]
        private void RemovedFolderClicked()
        {
            Process.Start(Directory.Exists(RemovedFolder) ? RemovedFolder : Environment.CurrentDirectory);
        }
    }
}
