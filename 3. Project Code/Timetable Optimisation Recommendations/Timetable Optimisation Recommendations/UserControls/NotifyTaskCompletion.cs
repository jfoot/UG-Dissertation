﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Timetable_Optimisation_Recommendations.UserControls
{
    /// <summary>
    /// Helper class to wrap around a Task to provide more information usable for UI databinding scenarios. As discussed in MSDN Magazine: https://msdn.microsoft.com/magazine/dn605875.
    /// </summary>
    /// <typeparam name="TResult">Type of result returned by task.</typeparam>
    public sealed class NotifyTaskCompletion<TResult> : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyTaskCompletion{TResult}"/> class.
        /// </summary>
        /// <param name="task">Task to wait on.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public NotifyTaskCompletion(Task<TResult> task)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Task = task;
            if (!task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync(task);
            }
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
            }

            if (PropertyChanged == null)
            {
                return;
            }

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));

            if (task.IsCanceled)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            else if (task.IsFaulted)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Exception)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(InnerException)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Result)));
            }
        }

        /// <summary>
        /// Gets the task that is being waited on.
        /// </summary>
        public Task<TResult> Task { get; private set; }

        /// <summary>
        /// Gets the task wrapper task.
        /// </summary>
        public Task TaskCompletion { get; private set; }

        /// <summary>
        /// Gets the result of the given task.
        /// </summary>
        public TResult? Result
        {
            get
            {
                return (Task.Status == TaskStatus.RanToCompletion) ?
                    Task.Result :
                    default;
            }
        }

        /// <summary>
        /// Gets the status of the task.
        /// </summary>
        public TaskStatus Status => Task.Status;

        /// <summary>
        /// Gets a value indicating whether the task is completed.
        /// </summary>
        public bool IsCompleted => Task.IsCompleted;

        /// <summary>
        /// Gets a value indicating whether the task is not completed.
        /// </summary>
        public bool IsNotCompleted => !Task.IsCompleted;

        /// <summary>
        /// Gets a value indicating whether the task was successfully completed.
        /// </summary>
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        /// <summary>
        /// Gets a value indicating whether the task was canceled.
        /// </summary>
        public bool IsCanceled => Task.IsCanceled;

        /// <summary>
        /// Gets a value indicating whether there was an error with the task.
        /// </summary>
        public bool IsFaulted => Task.IsFaulted;

        /// <summary>
        /// Gets the exception which occured on the task (if one occurred).
        /// </summary>
#pragma warning disable CS8603 // Possible null reference return.
        public AggregateException Exception => Task.Exception;
#pragma warning restore CS8603 // Possible null reference return.

        /// <summary>
        /// Gets the inner exception of the task.
        /// </summary>
#pragma warning disable CS8603 // Possible null reference return.
        public Exception InnerException => Exception?.InnerException;
#pragma warning restore CS8603 // Possible null reference return.

        /// <summary>
        /// Gets the error message of the task.
        /// </summary>
        public string ErrorMessage => InnerException?.Message ?? Exception.Message;

        /// <summary>
        /// PropertyChanged event.
        /// </summary>
#pragma warning disable CS8612 // Nullability of reference types in type doesn't match implicitly implemented member.
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS8612 // Nullability of reference types in type doesn't match implicitly implemented member.
    }
}
