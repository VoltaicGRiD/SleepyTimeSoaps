using System;

namespace SleepyTimeSoaps.Models
{
    public class RegistrationView
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public Guid ActivationCode { get; set; }
    }
}