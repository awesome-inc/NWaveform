using FluentAssertions;
using FontAwesome.Sharp;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(MenuItemViewModel))]
    // ReSharper disable InconsistentNaming
    internal class MenuItemViewModel_Should
    {
        [Test]
        public void Be_Creatable()
        {
            var sut = new MenuItemViewModel();
            sut.Should().BeAssignableTo<IMenuItemViewModel>();
        }

        [Test]
        public void Guess_properties_from_command()
        {
            var sut = new MenuItemViewModel();

            sut.Header.Should().BeNullOrWhiteSpace();
            sut.Description.Should().BeNullOrWhiteSpace();
            sut.Icon.Should().Be(IconChar.None);

            var command = new DelegateCommand(() => { }, () => false)
            {
                Title = "title",
                Description = "description",
                IconChar = IconChar.AddressBook
            };

            sut.Command = command;
            sut.Command.Should().Be(command);
            sut.Header.Should().Be(command.Title);
            sut.Description.Should().Be(command.Description);
            sut.Icon.Should().Be(command.IconChar);
        }
    }
}