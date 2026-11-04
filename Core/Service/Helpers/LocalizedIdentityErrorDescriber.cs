using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Shared.SharedResources;

namespace Service.Helpers;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer<SharedResources> _localizer;

    public LocalizedIdentityErrorDescriber(
        IStringLocalizer<SharedResources> localizer)
    {
        _localizer = localizer;
    }

    public override IdentityError PasswordTooShort(int length)
        => new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = _localizer[SharedResourcesKeys.PasswordTooShort, length]
        };

    public override IdentityError PasswordRequiresDigit()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = _localizer[SharedResourcesKeys.PasswordRequiresDigit]
        };

    public override IdentityError PasswordRequiresLower()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = _localizer[SharedResourcesKeys.PasswordRequiresLower]
        };

    public override IdentityError PasswordRequiresUpper()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = _localizer[SharedResourcesKeys.PasswordRequiresUpper]
        };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = _localizer[SharedResourcesKeys.PasswordRequiresSymbol]
        };
}
