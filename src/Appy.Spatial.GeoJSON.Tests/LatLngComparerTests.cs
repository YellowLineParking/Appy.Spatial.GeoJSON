using FluentAssertions;
using Xunit;

namespace Appy.Spatial.GeoJSON.Tests;

public class LatLngComparerTests
{
    readonly LatLngComparer _comparer = new();

    [Fact]
    public void ShouldBeEqualWhenCoordinatesMatch()
    {
        var x = new LatLng { Latitude = 51.5, Longitude = -0.12 };
        var y = new LatLng { Latitude = 51.5, Longitude = -0.12 };

        _comparer.Equals(x, y).Should().BeTrue();
        _comparer.GetHashCode(x).Should().Be(_comparer.GetHashCode(y));
    }

    [Fact]
    public void ShouldNotBeEqualWhenLatitudeDiffers()
    {
        var x = new LatLng { Latitude = 51.5, Longitude = -0.12 };
        var y = new LatLng { Latitude = 52.5, Longitude = -0.12 };

        _comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void ShouldNotBeEqualWhenLongitudeDiffers()
    {
        var x = new LatLng { Latitude = 51.5, Longitude = -0.12 };
        var y = new LatLng { Latitude = 51.5, Longitude = 1.5 };

        _comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void ShouldNotBeEqualWhenOtherLongitudeMatchesLatitude()
    {
        // Regression: longitude was compared against the other point's latitude
        var x = new LatLng { Latitude = 10, Longitude = 10 };
        var y = new LatLng { Latitude = 10, Longitude = 20 };

        _comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void ShouldBeEqualWhenBothNull() =>
        _comparer.Equals(null, null).Should().BeTrue();

    [Fact]
    public void ShouldNotBeEqualWhenOneIsNull()
    {
        var x = new LatLng { Latitude = 51.5, Longitude = -0.12 };

        _comparer.Equals(x, null).Should().BeFalse();
        _comparer.Equals(null, x).Should().BeFalse();
    }
}
