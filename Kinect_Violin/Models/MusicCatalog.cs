using System.Collections.Generic;

namespace WpfKinectV2CustomButton.Models
{
    public static class MusicCatalog
    {
        public static readonly IReadOnlyList<NoteButtonDefinition> NoteButtons = new[]
        {
            new NoteButtonDefinition("G0", 55),
            new NoteButtonDefinition("G1", 57),
            new NoteButtonDefinition("G2", 59),
            new NoteButtonDefinition("G3", 60),
            new NoteButtonDefinition("D0", 62),
            new NoteButtonDefinition("D1", 64),
            new NoteButtonDefinition("D2", 66),
            new NoteButtonDefinition("D3", 67),
            new NoteButtonDefinition("A0", 69),
            new NoteButtonDefinition("A1", 71),
            new NoteButtonDefinition("A2", 72),
            new NoteButtonDefinition("A3", 74),
            new NoteButtonDefinition("E0", 76),
            new NoteButtonDefinition("E1", 78),
            new NoteButtonDefinition("E2", 80),
            new NoteButtonDefinition("E3", 81),
        };

        public static readonly IReadOnlyList<InstrumentConfig> Instruments = new[]
        {
            new InstrumentConfig("Violin", 40, 0),
            new InstrumentConfig("Ensem", 49, 0),
            new InstrumentConfig("Cello", 42, -20),
            new InstrumentConfig("Bass", 43, -32),
        };

        private static readonly Dictionary<string, int> NoteMidiKeys = BuildNoteLookup();

        private static readonly Dictionary<string, InstrumentConfig> InstrumentLookup = BuildInstrumentLookup();

        public static bool TryGetMidiKey(string buttonId, out int midiKey)
        {
            return NoteMidiKeys.TryGetValue(buttonId, out midiKey);
        }

        public static bool IsNoteButton(string buttonId)
        {
            return NoteMidiKeys.ContainsKey(buttonId);
        }

        public static bool TryGetInstrument(string buttonId, out InstrumentConfig instrument)
        {
            return InstrumentLookup.TryGetValue(buttonId, out instrument);
        }

        private static Dictionary<string, int> BuildNoteLookup()
        {
            var lookup = new Dictionary<string, int>();
            foreach (NoteButtonDefinition note in NoteButtons)
            {
                lookup[note.Id] = note.MidiKey;
            }

            return lookup;
        }

        private static Dictionary<string, InstrumentConfig> BuildInstrumentLookup()
        {
            var lookup = new Dictionary<string, InstrumentConfig>();
            foreach (InstrumentConfig instrument in Instruments)
            {
                lookup[instrument.Name] = instrument;
            }

            return lookup;
        }
    }
}
