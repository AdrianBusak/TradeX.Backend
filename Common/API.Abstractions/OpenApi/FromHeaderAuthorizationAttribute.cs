using Microsoft.AspNetCore.Mvc;

namespace API.Abstractions.OpenApi
{
    public class FromHeaderAuthorizationAttribute : FromHeaderAttribute
    {
        public FromHeaderAuthorizationAttribute() => Name = "Authorization";
    }
}
