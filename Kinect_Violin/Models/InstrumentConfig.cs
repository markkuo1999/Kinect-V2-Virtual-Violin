namespace WpfKinectV2CustomButton.Models
{
    public sealed class InstrumentConfig
    {
        public InstrumentConfig(string name, int program, int pitchOffset)
        {
            Name = name;
            Program = program;
            PitchOffset = pitchOffset;
        }

        public string Name { get; }

        public int Program { get; }

        public int PitchOffset { get; }
    }
}
