// <copyright company="SIX Networks GmbH" file="QueueViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Input;
using ReactiveUI;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Applications.ViewModels.Main.Queue
{
    public class QueueViewModel : ViewModel, IQueueViewModel, IHaveDisplayName
    {
        // TODO
        IReactiveCommand _clearCompleted;
        // TODO
        IReactiveCommand _pauseAll;
        public IReactiveList<QueueItemViewModel> QueueItems { get; } = new ReactiveList<QueueItemViewModel> {
            new QueueItemViewModel("Queue Item 1", 15*FileSizeUnits.MB) {State = QueueItemState.Completed},
            new QueueItemViewModel("Queue Item 2", 88*FileSizeUnits.MB) {State = QueueItemState.Completed},
            new QueueItemViewModel("Queue Item 3", 32*FileSizeUnits.MB) {
                State = QueueItemState.Downloading,
                Progress = 33,
                Speed = 2*FileSizeUnits.MB
            },
            new QueueItemViewModel("Queue Item 4", 15*FileSizeUnits.MB) {
                State = QueueItemState.Downloading,
                Progress = 66,
                Speed = 3.33*FileSizeUnits.MB
            },
            new QueueItemViewModel("Queue Item 5", 24*FileSizeUnits.MB) {State = QueueItemState.Scheduled},
            new QueueItemViewModel("Queue Item 6", 66*FileSizeUnits.MB) {State = QueueItemState.Scheduled}
        };
        public ICommand ClearCompleted => _clearCompleted;
        public ICommand PauseAll => _pauseAll;
        public string DisplayName { get; } = "Queue";
    }

    public interface IQueueViewModel : IViewModel
    {
        string DisplayName { get; }
        IReactiveList<QueueItemViewModel> QueueItems { get; }
        ICommand ClearCompleted { get; }
        ICommand PauseAll { get; }
    }
}