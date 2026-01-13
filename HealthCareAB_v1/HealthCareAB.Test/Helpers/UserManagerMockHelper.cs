using HealthCareAB_v1.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public static class UserManagerMockHelper
{
    public static Mock<UserManager<ApplicationUser>> Create()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var userValidators = new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() };
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() };
        var normalizer = new Mock<ILookupNormalizer>();
        normalizer.Setup(n => n.NormalizeEmail(It.IsAny<string>())).Returns<string>(s => s?.ToUpperInvariant());
        normalizer.Setup(n => n.NormalizeName(It.IsAny<string>())).Returns<string>(s => s?.ToUpperInvariant());
        var describer = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer.Object,
            describer,
            services.Object,
            logger.Object);
    }
}
