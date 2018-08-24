using System;
using System.Windows.Input;

namespace FileSlideShow.ViewModels
{
    class RelayCommand : ICommand
    {
        private readonly Action Action;
        private readonly Func<bool> Predicate;

        public RelayCommand(Action action, Func<bool> predicate)
        {
            Action = action;
            Predicate = predicate;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => Predicate();

        public void Execute(object parameter) => Action();
    }
}
