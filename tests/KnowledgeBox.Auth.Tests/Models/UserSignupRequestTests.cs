using System.ComponentModel.DataAnnotations;
using KnowledgeBox.Auth.Models;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Models
{
    public class UserSignupRequestTests
    {
        [Fact]
        public void UserSignupRequest_WithValidData_PassesValidation()
        {
            // Arrange
            var model = new UserSignupRequest
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com"
            };
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(
                model, 
                new ValidationContext(model), 
                validationResults, 
                true);
            
            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Theory]
        [InlineData("", "Password123!", "test@example.com", "Username")]
        [InlineData("te", "Password123!", "test@example.com", "Username")]
        [InlineData("testuser", "", "test@example.com", "Password")]
        [InlineData("testuser", "pass", "test@example.com", "Password")]
        [InlineData("testuser", "Password123!", "", "Email")]
        [InlineData("testuser", "Password123!", "invalidemail", "Email")]
        public void UserSignupRequest_WithInvalidData_FailsValidation(
            string username, string password, string email, string expectedInvalidField)
        {
            // Arrange
            var model = new UserSignupRequest
            {
                Username = username,
                Password = password,
                Email = email
            };
            var validationResults = new List<ValidationResult>();
            
            // Act
            var isValid = Validator.TryValidateObject(
                model, 
                new ValidationContext(model), 
                validationResults, 
                true);
            
            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => 
                vr.MemberNames.Any(mn => mn == expectedInvalidField));
        }
    }
} 