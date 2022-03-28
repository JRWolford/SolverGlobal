using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace Api.Unit.Tests.TestUtils
{
  /// <summary>
  ///   Creates an easy way for us to auto-moq different dependencies.
  ///   Makes Unit Testing a lot faster.
  /// </summary>
  public class AutoMoqAutoDataAttribute : AutoDataAttribute
  {
    public AutoMoqAutoDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
      var fixture = new Fixture();
      fixture.Customize(new AutoMoqCustomization());

      return fixture;
    }
  }
}