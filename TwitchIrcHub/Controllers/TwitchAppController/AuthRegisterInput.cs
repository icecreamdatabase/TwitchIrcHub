using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace TwitchIrcHub.Controllers.TwitchAppController;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AuthRegisterInput
{
    [FromQuery(Name = "code")]
    public string? Code { get; set; }

    [FromQuery(Name = "state")]
    public string? State { get; set; }

    [FromQuery(Name = "scope")]
    public string? Scope { get; set; }

    [FromQuery(Name = "error")]
    public string? Error { get; set; }

    [FromQuery(Name = "error_description")]
    public string? ErrorDescription { get; set; }
}
