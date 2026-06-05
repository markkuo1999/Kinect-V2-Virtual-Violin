namespace WpfKinectV2CustomButton.Models
{
    public sealed class NoteButtonDefinition
    {
        public NoteButtonDefinition(string id, int midiKey)
        {
            Id = id;
            MidiKey = midiKey;
        }

        public string Id { get; }

        public int MidiKey { get; }
    }
}
