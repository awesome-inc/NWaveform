using System;
using Caliburn.Micro;

namespace NWaveform.App
{
    public interface IPlayerViewModel : IHaveDisplayName
    {
        string ToolTip { get; }
        Uri Source { get; set; }
    }
}