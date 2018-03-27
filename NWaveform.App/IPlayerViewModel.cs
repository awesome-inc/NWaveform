using System;
using Caliburn.Micro;

namespace NWaveform.App
{
    public interface IPlayerViewModel : IScreen, IHandle<ActivateChannel>
    {
        string ToolTip { get; }
        Uri Source { get; set; }
    }
}
