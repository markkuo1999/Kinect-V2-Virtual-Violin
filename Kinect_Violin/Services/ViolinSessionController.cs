using System;
using WpfKinectV2CustomButton.Models;

namespace WpfKinectV2CustomButton.Services
{
    public sealed class ViolinSessionController
    {
        private readonly MidiService midiService;
        private string hoveredButtonId;
        private bool isArmed;
        private bool isGestureActive;
        private bool isNotePlaying;

        public ViolinSessionController(MidiService midiService)
        {
            this.midiService = midiService;
        }

        public event EventHandler<bool> GestureIndicatorChanged;

        public event EventHandler<ButtonVisualStateChangedEventArgs> ButtonVisualStateChanged;

        public void OnButtonEntered(string buttonId, object buttonElement)
        {
            hoveredButtonId = buttonId;

            if (!isArmed)
            {
                return;
            }

            if (MusicCatalog.TryGetInstrument(buttonId, out InstrumentConfig instrument))
            {
                midiService.SetInstrument(instrument);
                RaiseButtonVisualState(buttonId, buttonElement, true);
                return;
            }

            if (isGestureActive && MusicCatalog.IsNoteButton(buttonId))
            {
                StartNote(buttonId, buttonElement);
            }
        }

        public void OnButtonLeft(string buttonId, object buttonElement)
        {
            if (MusicCatalog.IsNoteButton(buttonId))
            {
                midiService.StopNote(buttonId);
                RaiseButtonVisualState(buttonId, buttonElement, false);
            }
            else if (MusicCatalog.TryGetInstrument(buttonId, out _))
            {
                RaiseButtonVisualState(buttonId, buttonElement, false);
            }

            isArmed = true;
            isNotePlaying = false;
            hoveredButtonId = null;
        }

        public void OnGestureStateChanged(bool isDetected)
        {
            isGestureActive = isDetected;
            GestureIndicatorChanged?.Invoke(this, isDetected);

            if (!isArmed || string.IsNullOrEmpty(hoveredButtonId))
            {
                if (!isDetected)
                {
                    StopCurrentNote();
                }

                return;
            }

            if (isDetected)
            {
                if (MusicCatalog.IsNoteButton(hoveredButtonId))
                {
                    StartNote(hoveredButtonId, null);
                }
            }
            else
            {
                StopCurrentNote();
            }
        }

        public void ResetGestureLatch()
        {
            isGestureActive = false;
        }

        private void StartNote(string buttonId, object buttonElement)
        {
            midiService.PlayNote(buttonId);
            isNotePlaying = true;
            RaiseButtonVisualState(buttonId, buttonElement, true);
        }

        private void StopCurrentNote()
        {
            if (!isNotePlaying || string.IsNullOrEmpty(hoveredButtonId))
            {
                return;
            }

            midiService.StopNote(hoveredButtonId);
            RaiseButtonVisualState(hoveredButtonId, null, false);
            isNotePlaying = false;
        }

        private void RaiseButtonVisualState(string buttonId, object buttonElement, bool isActive)
        {
            ButtonVisualStateChanged?.Invoke(this, new ButtonVisualStateChangedEventArgs(buttonId, buttonElement, isActive));
        }
    }

    public sealed class ButtonVisualStateChangedEventArgs : EventArgs
    {
        public ButtonVisualStateChangedEventArgs(string buttonId, object buttonElement, bool isActive)
        {
            ButtonId = buttonId;
            ButtonElement = buttonElement;
            IsActive = isActive;
        }

        public string ButtonId { get; }

        public object ButtonElement { get; }

        public bool IsActive { get; }
    }
}
