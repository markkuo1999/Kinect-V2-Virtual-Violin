using System;
using System.Linq;
using RtMidi.Core;
using RtMidi.Core.Devices;
using RtMidi.Core.Enums;
using RtMidi.Core.Messages;
using WpfKinectV2CustomButton.Models;
using Key = RtMidi.Core.Enums.Key;

namespace WpfKinectV2CustomButton.Services
{
    public sealed class MidiService : IDisposable
    {
        private IMidiOutputDevice device;
        private int program = 40;
        private int pitchOffset;
        private string activeNoteId;

        public bool IsAvailable { get; private set; }

        public string StatusMessage { get; private set; }

        public bool TryOpen()
        {
            var outputDevices = MidiDeviceManager.Default.OutputDevices.ToList();
            if (outputDevices.Count == 0)
            {
                IsAvailable = false;
                StatusMessage = "No MIDI output device found. Install loopMIDI for sound output.";
                return false;
            }

            device = outputDevices[0].CreateDevice();
            device.Open();
            IsAvailable = true;
            StatusMessage = "MIDI ready.";
            return true;
        }

        public void SetInstrument(InstrumentConfig instrument)
        {
            if (!IsAvailable || instrument == null)
            {
                return;
            }

            program = instrument.Program;
            pitchOffset = instrument.PitchOffset;
            device.Send(new ProgramChangeMessage(Channel.Channel1, program));
        }

        public void PlayNote(string buttonId)
        {
            if (!IsAvailable || !MusicCatalog.TryGetMidiKey(buttonId, out int midiKey))
            {
                return;
            }

            device.Send(new ProgramChangeMessage(Channel.Channel1, program));
            device.Send(new NoteOnMessage(Channel.Channel1, (Key)midiKey + pitchOffset, 127));
            activeNoteId = buttonId;
        }

        public void StopNote(string buttonId)
        {
            if (!IsAvailable || !MusicCatalog.TryGetMidiKey(buttonId, out int midiKey))
            {
                return;
            }

            device.Send(new NoteOffMessage(Channel.Channel1, (Key)midiKey + pitchOffset, 127));

            if (activeNoteId == buttonId)
            {
                activeNoteId = null;
            }
        }

        public void StopActiveNote()
        {
            if (!string.IsNullOrEmpty(activeNoteId))
            {
                StopNote(activeNoteId);
            }
        }

        public void Dispose()
        {
            if (device != null)
            {
                StopActiveNote();
                device.Close();
                device = null;
            }
        }
    }
}
