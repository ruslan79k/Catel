﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuditingHelper.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.MVVM.Auditing
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Reflection;

    /// <summary>
    /// Helper for auditing which handles the complete subscription of an <see cref="IViewModel"/> instance
    /// to the current auditing manager.
    /// </summary>
    public static class AuditingHelper
    {
        private static readonly HashSet<string> KnownIgnoredPropertyNames = new HashSet<string>();

        /// <summary>
        /// Initializes static members of the <see cref="AuditingHelper"/> class.
        /// </summary>
        static AuditingHelper()
        {
            KnownIgnoredPropertyNames.Add("IDataWarningInfo.Warning");
            KnownIgnoredPropertyNames.Add("INotifyDataWarningInfo.HasWarnings");
            KnownIgnoredPropertyNames.Add("IDataErrorInfo.Error");
            KnownIgnoredPropertyNames.Add("INotifyDataErrorInfo.HasErrors");
        }

        /// <summary>
        /// Registers the view model to the <see cref="AuditingManager"/>.
        /// <para />
        /// This helper will automatically unsubscribe from all events when the view model is closed.
        /// </summary>
        /// <param name="viewModel">The view model to register.</param>
        /// <remarks>
        /// This helper will call the <see cref="AuditingManager.OnViewModelCreating"/> and <see cref="AuditingManager.OnViewModelCreated"/>
        /// automatically.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="viewModel" /> is <c>null</c>.</exception>
        public static void RegisterViewModel(IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var isAuditingEnabled = AuditingManager.IsAuditingEnabled;
            if (isAuditingEnabled)
            {
                AuditingManager.OnViewModelCreating(viewModel.GetType());
            }

            SubscribeEvents(viewModel);

            if (isAuditingEnabled)
            {
                AuditingManager.OnViewModelCreated(viewModel);
            }
        }

        /// <summary>
        /// Subscribes to all events of the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="viewModel" /> is <c>null</c>.</exception>
        private static void SubscribeEvents(IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var viewModelAsPropertyChanging = viewModel as INotifyPropertyChanging;
            if (viewModelAsPropertyChanging != null)
            {
                viewModelAsPropertyChanging.PropertyChanging += OnViewModelPropertyChanging;
            }

            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.CommandExecuted += OnViewModelCommandExecuted;
            viewModel.Saving += OnViewModelSaving;
            viewModel.Saved += OnViewModelSaved;
            viewModel.Canceling += OnViewModelCanceling;
            viewModel.Canceled += OnViewModelCanceled;
            viewModel.Closing += OnViewModelClosing;
            viewModel.Closed += OnViewModelClosed;
        }

        /// <summary>
        /// Unsubscribes from all events of the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="viewModel" /> is <c>null</c>.</exception>
        private static void UnsubscribeEvents(IViewModel viewModel)
        {
            Argument.IsNotNull("viewModel", viewModel);

            var viewModelAsPropertyChanging = viewModel as INotifyPropertyChanging;
            if (viewModelAsPropertyChanging != null)
            {
                viewModelAsPropertyChanging.PropertyChanging -= OnViewModelPropertyChanging;
            }

            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            viewModel.CommandExecuted -= OnViewModelCommandExecuted;
            viewModel.Saving -= OnViewModelSaving;
            viewModel.Saved -= OnViewModelSaved;
            viewModel.Canceling -= OnViewModelCanceling;
            viewModel.Canceled -= OnViewModelCanceled;
            viewModel.Closing -= OnViewModelClosing;
            viewModel.Closed -= OnViewModelClosed;
        }

        private static void OnViewModelPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            var viewModel = (IViewModel)sender;

            object propertyValue = null;
            if (!string.IsNullOrEmpty(e.PropertyName))
            {
                PropertyHelper.TryGetPropertyValue(viewModel, e.PropertyName, out propertyValue);
            }

            AuditingManager.OnPropertyChanging(viewModel, e.PropertyName, propertyValue);
        }

        private static void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            var viewModel = (IViewModel)sender;

            object propertyValue = null;
            if (!string.IsNullOrEmpty(e.PropertyName) && !KnownIgnoredPropertyNames.Contains(e.PropertyName))
            {
                PropertyHelper.TryGetPropertyValue(viewModel, e.PropertyName, out propertyValue);
            }

            AuditingManager.OnPropertyChanged(viewModel, e.PropertyName, propertyValue);
        }

        private static void OnViewModelCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            AuditingManager.OnCommandExecuted((IViewModel)sender, e.CommandPropertyName, e.Command, e.CommandParameter);
        }

        private static void OnViewModelSaving(object sender, SavingEventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            AuditingManager.OnViewModelSaving((IViewModel)sender);
        }

        private static void OnViewModelSaved(object sender, EventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            AuditingManager.OnViewModelSaved((IViewModel)sender);
        }

        private static void OnViewModelCanceling(object sender, CancelingEventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            AuditingManager.OnViewModelCanceling((IViewModel)sender);
        }

        private static void OnViewModelCanceled(object sender, EventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            AuditingManager.OnViewModelCanceled((IViewModel)sender);
        }

        private static void OnViewModelClosing(object sender, EventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            AuditingManager.OnViewModelClosing((IViewModel)sender);
        }

        private static void OnViewModelClosed(object sender, EventArgs e)
        {
            if (!AuditingManager.IsAuditingEnabled)
            {
                return;
            }

            var viewModel = (IViewModel) sender;
            AuditingManager.OnViewModelClosed(viewModel);

            UnsubscribeEvents(viewModel);
        }
    }
}