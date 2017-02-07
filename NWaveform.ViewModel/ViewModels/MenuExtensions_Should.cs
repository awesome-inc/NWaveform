using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof (MenuExtensions))]
    // ReSharper disable InconsistentNaming
    internal class MenuExtensions_Should
    {
        [Test]
        public void Test_MenuItem_IsEnabled()
        {
            IMenuItemViewModel menuItem = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            menuItem.IsEnabled().Should().BeFalse("menu item is null");

            menuItem = Substitute.For<IMenuItemViewModel>();
            
            menuItem.Command.Returns((ICommand) null);
            menuItem.IsEnabled().Should().BeFalse("command is null");

            menuItem = Substitute.For<IMenuItemViewModel>();
            menuItem.Command.CanExecute(Arg.Any<object>()).Returns(false);
            menuItem.IsEnabled().Should().BeFalse("cannot execute command");

            menuItem.Command.CanExecute(Arg.Any<object>()).Returns(true);
            menuItem.IsEnabled().Should().BeTrue("can execute command");
        }

        [Test]
        public void Test_Menu_IsEmpty()
        {
            IMenuViewModel menu = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            menu.IsEmpty().Should().BeTrue("menu is null");

            menu = Substitute.For<IMenuViewModel>();
            menu.Items.Returns((IEnumerable<IMenuItemViewModel>) null);
            menu.IsEmpty().Should().BeTrue("items are null");

            menu.Items.Returns(Enumerable.Empty<IMenuItemViewModel>());
            menu.IsEmpty().Should().BeTrue("items are empty");

            var menuItem = Substitute.For<IMenuItemViewModel>();
            menuItem.IsEnabled().Should().BeFalse();
            menu.Items.Returns(new []{menuItem});
            menu.IsEmpty().Should().BeTrue("item is not enabled");

            menuItem.Command.CanExecute(Arg.Any<object>()).Returns(true);
            menu.IsEmpty().Should().BeFalse("item is enabled");
        }
    }
}