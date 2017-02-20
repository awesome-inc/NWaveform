using System.Windows.Input;
using FontAwesome.Sharp;

namespace NWaveform
{
    public interface ICommand<in TContext> : ICommand
    {
        bool CanExecute(TContext context);
        void Execute(TContext context);
        int Order { get; set; }
        IconChar IconChar { get; set; }
    }
}