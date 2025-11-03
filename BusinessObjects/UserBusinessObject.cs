using Amazon.DynamoDBv2.DataModel;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;
using serverless_auth.Utilities;
using serverless_auth.ViewModels; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace serverless_auth.BusinessObjects
{
    public class UserBusinessObject
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly JWTUtilities _jwtService;
        private readonly IConfiguration _configuration;

        public UserBusinessObject(IDynamoDBContext dbContext, JWTUtilities jwtService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<AuthResponseViewModel> LoginUserAsync(LoginViewModel request)
        {
            var user = await _dbContext.LoadAsync<UserViewModel>(request.ApplicationID, request.Email);

            DateTime currentTimeUtc = DateTime.UtcNow.AddMinutes(330);

            var maxFailedLoginAttempts = _configuration.GetValue<int>("SecuritySettings:MaxFailedLoginAttempts");
            if (maxFailedLoginAttempts == 0) maxFailedLoginAttempts = 25;

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (user.IsLocked)
            {
                throw new UnauthorizedAccessException("Account is locked. Please contact support.");
            }

            if (user.IsActive == false || user.IsEnabled == false)
            {
                throw new UnauthorizedAccessException("Account is inactive or disabled.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                user.FailedAttemptOn = currentTimeUtc;

                if (user.FailedLoginAttempts >= maxFailedLoginAttempts)
                {
                    user.IsLocked = true;
                    user.LockedOn = currentTimeUtc;
                }
                user.ModifiedOn = currentTimeUtc;
                await _dbContext.SaveAsync(user);

                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            user.FailedLoginAttempts = 0;
            user.FailedAttemptOn = null;
            user.IsLocked = false;
            user.LockedOn = null;

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.LastLoggedOn = currentTimeUtc; 
            user.ModifiedOn = currentTimeUtc; 
            await _dbContext.SaveAsync(user);

            return new AuthResponseViewModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.UserID,
                ApplicationID = user.ApplicationID,
                ExpiresIn = _jwtService.GetAccessTokenExpirationSeconds()
            };
        }

        public async Task<UserViewModel> AddOrUpdateUserAsync(AddUserViewModel request)
        {
            var user = await _dbContext.LoadAsync<UserViewModel>(request.ApplicationID, request.Email);
            DateTime currentTimeUtc = DateTime.UtcNow.AddMinutes(330);

            if (user == null)
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    throw new ArgumentException("Password is required for new user creation.");
                }

                user = new UserViewModel
                {
                    ApplicationID = request.ApplicationID,
                    UserID = request.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedOn = currentTimeUtc, 
                    ModifiedOn = currentTimeUtc,
                    IsActive = request.IsActive ?? true,
                    IsLocked = request.IsLocked ?? false,
                    IsEnabled = request.IsEnabled ?? true,
                    FailedLoginAttempts = 0,
                    FailedAttemptOn = null,
                    LockedOn = null
                };
            }
            else
            {
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }
                user.ModifiedOn = currentTimeUtc;

                if (request.IsActive.HasValue) user.IsActive = request.IsActive;
                if (request.IsLocked.HasValue) user.IsLocked = request.IsLocked.Value;
                if (request.IsEnabled.HasValue) user.IsEnabled = request.IsEnabled;
            }

            await _dbContext.SaveAsync(user);
            return user;
        }
    }
}
