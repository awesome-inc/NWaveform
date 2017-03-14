using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Documents;
using FluentAssertions;
using FontAwesome.Sharp;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NWaveform.ViewModels
{
    [TestFixtureFor(typeof(LabelVievModel))]
    // ReSharper disable InconsistentNaming
    internal class LabelVievModel_Should
    {
        [Test]
        public void Have_defaults()
        {
            var sut = new LabelVievModel();
            sut.Text.Should().BeNullOrEmpty();
            sut.Tooltip.Should().BeNullOrEmpty();
            sut.FontSize.Should().Be( Convert.ToInt32(TextElement.FontSizeProperty.DefaultMetadata.DefaultValue) );
            sut.FontWeight.Should().Be(TextElement.FontWeightProperty.DefaultMetadata.DefaultValue);

            sut.Icon.Should().Be(IconChar.None);
            sut.IconFlipOrientation.Should().Be(FlipOrientation.Normal);
            sut.IconSpin.Should().Be(false);
            sut.IconSpinDuration.Should().Be(Convert.ToDouble(Awesome.SpinDurationProperty.DefaultMetadata.DefaultValue));
        }

        [Test]
        public void Suppress_empty_context_menus()
        {
            var sut = new LabelVievModel();
            sut.Menu.Should().BeNull();

            var ctor = typeof(ContextMenuEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            var args = (ContextMenuEventArgs)ctor.Invoke(new object[] { sut, true });

            sut.SuppressEmptyContextMenu(args);

            args.Handled.Should().Be(sut.Menu.IsEmpty());
        }
    }
}