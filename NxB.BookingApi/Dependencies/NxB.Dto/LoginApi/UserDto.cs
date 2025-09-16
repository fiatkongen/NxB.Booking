using System;
using System.Collections.Generic;

namespace NxB.Dto.LoginApi
{
    public class BaseUserDto
    {
        public string Username { get; set; }
        public string Login { get; set; }
        public string CountryId { get; set; }
        public string LanguageId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AccessCardId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDisabled { get; set; }
        public List<string> Roles { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class CreateUserDto : BaseUserDto
    {
        public string Password { get; set; }
    }

    public class ResetPasswordDto
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserDto: BaseUserDto
    {
        public Guid Id { get; set; }
    }

    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangePassword2Dto
    {
        public string ClientId { get; set; }
        public string Login { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
