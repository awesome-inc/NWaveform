using System;
using System.Diagnostics;
using System.Windows.Input;
using FontAwesome.Sharp;
using NEdifis.Attributes;

namespace NWaveform
{
    public class DelegateCommand : ICommand
    {
        protected Predicate<object> CanExecutePredicate = context => true;
        protected Action<object> DoExecuteAction = context => { };

        public string Title { get; set; }
        public string Description { get; set; }
        public IconChar IconChar { get; set; }

        #region constructor

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            CanExecutePredicate = canExecute ?? (context => true);
            DoExecuteAction = execute;

            Title = string.Empty;
            Description = string.Empty;
        }

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
            : this(execute == null ? (Action<object>)(context => { }) : o => execute(),
                canExecute == null ? (Predicate<object>)(context => true) : o => canExecute())
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
        }

        protected DelegateCommand()
        {
        }

        #endregion

        #region ICommand

        // cf.: http://stackoverflow.com/questions/2587916/wpf-viewmodel-commands-canexecute-issue
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected static void OnCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return CanExecutePredicate == null || CanExecutePredicate(parameter);
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            DoExecuteAction(parameter);
        }

        #endregion
    }

    [ExcludeFromConventions("tested by" + nameof(DelegateCommand_Should))]
    public class DelegateCommand<TContext>
        : DelegateCommand
        , ICommand<TContext>
        where TContext : class
    {
        #region constructor

        public DelegateCommand(
            Action<TContext> execute,
            Predicate<TContext> canExecute = null)
            : base(ToExecute(execute), ToCanExecute(canExecute))
        { }

        protected DelegateCommand() { }

        private static Predicate<object> ToCanExecute(Predicate<TContext> canExecute)
        {
            if (canExecute == null) return null;
            return obj => (obj == null || obj is TContext) && canExecute((TContext)obj);
        }

        private static TContext SafeCast(object context)
        {
            if (context is TContext) return (TContext)context;
            return default(TContext);
        }

        private static Action<object> ToExecute(Action<TContext> execute)
        {
            if (execute == null) return null;
            // for lengthy actions context menu commands should
            // use Dispatcher.BeginInvoke so the ContextMenu closes but the UI still blocks
            return context => Caliburn.Micro.Execute.BeginOnUIThread(() => execute(SafeCast(context)));
        }

        #endregion

        /// <summary>
        /// sets the <see cref="DelegateCommand"/> CanExecute predicate and Execute action
        /// of the more general baseclass. 
        /// The generic implementation is a typesafe wrapper for the delegate command.
        /// </summary>
        protected void DelegateTo(Action<TContext> execute, Predicate<TContext> canExecute = null)
        {
            DoExecuteAction = ToExecute(execute);
            CanExecutePredicate = ToCanExecute(canExecute);
        }

        #region ICommand<TContext>

        public bool CanExecute(TContext model)
        {
            return base.CanExecute(model);
        }

        public void Execute(TContext model)
        {
            base.Execute(model);
        }

        public int Order { get; set; }

        #endregion
    }
}