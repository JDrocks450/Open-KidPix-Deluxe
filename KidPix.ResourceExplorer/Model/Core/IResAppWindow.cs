using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KidPix.ResourceExplorer.Model.Core
{
    public enum IResAppNavEventResult
    {
        None,
        /// <summary>
        /// Cannot navigate to the given page at this time due to internal logic/error
        /// </summary>
        Rejected,
        /// <summary>
        /// The user cancelled this navigation event
        /// <para/>Useful for a "Unsaved changes" message box in which the user selects "Go back"
        /// </summary>
        Canceled,
        /// <summary>
        /// Navigation is accepted but not completed when control is returned to the caller
        /// <para/>An async navigation event that is not awaited would be applicable in this case
        /// </summary>
        Navigating,
        /// <summary>
        /// Navigation completed successfully to the new content
        /// </summary>
        Navigated
    }

    /// <summary>
    /// An interface that exposes functions to interact with the <see cref="MainWindow"/> of the application that is portable to apply to any window in the application
    /// </summary>
    public interface IResAppWindow
    {       
        /// <summary>
        /// Requests to navigate to a new <see cref="IResAppPage"/> and returns a status reason
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="NewPage"></param>
        /// <returns></returns>
        public Task<IResAppNavEventResult> NavigateNewPrimaryPageAsync<T>(T NewPage) where T : Page, IResAppPage;
        /// <summary>
        /// See: <see cref="NavigateNewPrimaryPageAsync{T}(T)"/> blocks caller and returns status reason
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="NewPage"></param>
        public IResAppNavEventResult NavigateNewPrimaryPage<T>(T NewPage) where T : Page, IResAppPage => NavigateNewPrimaryPageAsync(NewPage).Result;

        public bool TryRegisterMenuItem(string Path, System.Windows.RoutedEventHandler ActionCallback, out MenuItem? CreatedItem);
        public bool TryDeregisterMenuItem(MenuItem Item);
        public void ClearAllMenuItems();
    }

    /// <summary>
    /// An interface that exposes general functions to interact with an 
    /// </summary>
    public interface IResAppPage
    {
        public IResAppWindow ParentResWindow { get; set; }

        public Task<IResAppNavEventResult> NotifyResAppNavigatingAway();
        public Task<IResAppNavEventResult> GetNavAccept() => Task.Run(() => IResAppNavEventResult.Navigated);

        public bool TryRegisterMenuItem(string Path, System.Windows.RoutedEventHandler ActionCallback, out MenuItem? CreatedItem) =>
            ParentResWindow.TryRegisterMenuItem(Path, ActionCallback, out CreatedItem);
        public bool TryDeregisterMenuItem(MenuItem Item) => ParentResWindow.TryDeregisterMenuItem(Item);
        public void ClearAllMenuItems() => ParentResWindow.ClearAllMenuItems();
    }
}
