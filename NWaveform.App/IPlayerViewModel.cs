using System;
using Caliburn.Micro;

namespace NWaveform.App
{
    public interface IPlayerViewModel : IScreen
    {
        string ToolTip { get; }
        Uri Source { get; set; }
    }
}