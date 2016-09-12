using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using Uco.Infrastructure.Livecycle;
using Uco.Models;

namespace Uco.Infrastructure.Providers
{
    public class SUMembershipProvider : MembershipProvider
    {
        #region members
        private const int newPasswordLength = 8;
        private string applicationName;

        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private MachineKeySection machineKey; // Used when determining encryption key values.
        private int maxInvalidPasswordAttempts;
        private int minRequiredNonAlphanumericCharacters;
        private int minRequiredPasswordLength;
        private int passwordAttemptWindow;
        private MembershipPasswordFormat passwordFormat;
        private string passwordStrengthRegularExpression;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;
        #endregion

        #region properties

        private static Db _db
        {
            get { return LS.CurrentEntityContext; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return enablePasswordRetrieval; }
        }
        public override bool EnablePasswordReset
        {
            get { return enablePasswordReset; }
        }
        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }
        public override string ApplicationName { get; set; }
        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }
        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }
        public override bool RequiresUniqueEmail
        {
            get { return requiresUniqueEmail; }
        }
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }
        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }
        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        public string ConnectionString { get; set; }
        #endregion

        #region public methods

        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null) throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name)) name = "SUMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Uco Membership Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            applicationName = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);
            maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));

            string temp_format = config["passwordFormat"] ?? "Hashed";

            switch (temp_format)
            {
                case "Hashed":
                    passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            // Initialize SqlConnection.
            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }
            ConnectionString = ConnectionStringSettings.ConnectionString;

            // Get encryption and decryption key information from the configuration.
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
                if (PasswordFormat != MembershipPasswordFormat.Clear) throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");
        }

        public override MembershipUser CreateUser(string UserName, string password, string email,
                                                  string passwordQuestion, string passwordAnswer, bool isApproved,
                                                  object providerUserKey, out MembershipCreateStatus status)
        {
            // Validate UserName/password
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(UserName, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            // Check whether user with passed UserName already exists
            MembershipUser u = GetUser(UserName, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null) providerUserKey = Guid.NewGuid();
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                User user = new User
                {
                    ID = (Guid)providerUserKey,
                    UserName = UserName,
                    ApplicationName = applicationName,
                    Email = email,
                    Password = EncodePassword(password),
                    PasswordQuestion = passwordQuestion,
                    PasswordAnswer = passwordAnswer,
                    IsApproved = isApproved,
                    LastActivityDate = createDate,
                    LastLoginDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    LastPasswordChangedDate = createDate,
                    CreationDate = createDate,
                    IsOnline = false,
                    IsLockedOut = false,
                    LastLockedOutDate = createDate,
                    FailedPasswordAttemptCount = 0,
                    FailedPasswordAttemptWindowStart = createDate,
                    FailedPasswordAnswerAttemptCount = 0,
                    FailedPasswordAnswerAttemptWindowStart = createDate,
                    Phone = string.Empty,
                };
                user.Roles = "|Anonymous|";
                try
                {
                    _db.Users.Add(user);
                    _db.SaveChanges();
                    status = MembershipCreateStatus.Success;
                }
                catch
                {
                    status = MembershipCreateStatus.UserRejected;
                }

                return GetUser(UserName, false);
            }
            status = MembershipCreateStatus.DuplicateUserName;

            return null;
        }

        public MembershipUser CreateUser(string UserName, string password, string email, out MembershipCreateStatus status)
        {
            return this.CreateUser(UserName, password, email, "", "", true, null, out status);
        }
        public MembershipUser CreateUser(string UserName, string password, string email)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            return this.CreateUser(UserName, password, email, "", "", true, null, out status);
        }

        public override bool ChangePasswordQuestionAndAnswer(string UserName, string password,
                                                             string newPasswordQuestion, string newPasswordAnswer)
        {
            //check if user is authenticated
            if (!ValidateUser(UserName, password)) return false;


            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) throw new ProviderException("Change password question and answer failed. No unique user found.");

            User user = users.First();
            user.PasswordAnswer = EncodePassword(newPasswordAnswer);
            user.PasswordQuestion = newPasswordQuestion;

            try
            {
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public override string GetPassword(string UserName, string answer)
        {
            if (!EnablePasswordRetrieval) throw new ProviderException("Password Retrieval Not Enabled.");
            if (PasswordFormat == MembershipPasswordFormat.Hashed) throw new ProviderException("Cannot retrieve Hashed passwords.");

            string password = string.Empty;


            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) throw new ProviderException("Get password failed. No unique user found.");

            User user = users.First();
            if (user != null)
            {
                if (Convert.ToBoolean(user.IsLockedOut)) throw new MembershipPasswordException("The supplied user is locked out.");
            }
            else throw new MembershipPasswordException("The supplied user name is not found.");

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, user.PasswordAnswer))
            {
                UpdateFailureCount(UserName, "passwordAnswer");
                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Encrypted) password = UnEncodePassword(user.Password);

            return password;
        }

        public override bool ChangePassword(string UserName, string oldPassword, string newPassword)
        {
            //check if user is authenticated
            if (!ValidateUser(UserName, oldPassword)) return false;

            //notify that password is going to change
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(UserName, newPassword, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null) throw args.FailureInformation;
                throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
            }



            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) throw new ProviderException("Change password failed. No unique user found.");

            User user = users.First();
            user.Password = EncodePassword(newPassword);
            user.LastPasswordChangedDate = DateTime.Now;

            try
            {
                _db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public override string ResetPassword(string UserName, string answer)
        {
            if (!EnablePasswordReset) throw new NotSupportedException("Password reset is not enabled.");

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(UserName, "passwordAnswer");
                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword = Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(UserName, newPassword, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null) throw args.FailureInformation;
                throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
            }


            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) throw new ProviderException("Reset password failed. No unique user found.");

            User user = users.First();
            if (user != null)
            {
                if (Convert.ToBoolean(user.IsLockedOut)) throw new MembershipPasswordException("The supplied user is locked out.");
            }
            else
            {
                throw new MembershipPasswordException("The supplied user name is not found.");
            }

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, user.PasswordAnswer))
            {
                UpdateFailureCount(UserName, "passwordAnswer");
                throw new MembershipPasswordException("Incorrect password answer.");
            }

            try
            {
                user.Password = EncodePassword(newPassword);
                user.LastPasswordChangedDate = DateTime.Now;
                _db.SaveChanges();
            }
            catch
            {
                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }

            return newPassword;
        }

        public override void UpdateUser(MembershipUser membershipUser)
        {
            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == membershipUser.UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) throw new ProviderException("Update user failed. No unique user found.");

            User user = users.First();
            if (user == null) return;

            user.Email = user.Email;
            user.Comment = user.Comment;
            user.IsApproved = user.IsApproved;
            _db.SaveChanges();
        }

        public override bool ValidateUser(string UserName, string password)
        {
            bool isValid = false;


            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) return false;

            User user = users.First();
            if (user == null) return false;

            if (user.IsLockedOut)
            {
                DateTime LastLockedOutDate = Convert.ToDateTime(user.LastLockedOutDate);
                if (LastLockedOutDate > DateTime.Now.AddMinutes(-PasswordAttemptWindow)) throw new ProviderException("User IsLockedOut.");
                user.IsLockedOut = false;
                user.FailedPasswordAttemptCount = 0;
            }

            if (CheckPassword(password, user.Password))
            {
                if (user.IsApproved)
                {
                    isValid = true;
                    user.LastLoginDate = DateTime.Now;
                    _db.SaveChanges();
                }
            }
            else
            {
                UpdateFailureCount(UserName, "password");
            }

            return isValid;
        }

        public override bool UnlockUser(string UserName)
        {
            try
            {

                IQueryable<User> users = from u in _db.Users
                                         where u.UserName == UserName && u.ApplicationName == applicationName
                                         select u;

                if (users.Count() != 1) return false;

                User user = users.First();
                if (user == null) return false;

                user.LastLockedOutDate = DateTime.Now;
                _db.SaveChanges();

                return true;

            }
            catch
            {
                return false;
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser membershipUser = null;


            IQueryable<User> users = from u in _db.Users
                                     where u.ID == (Guid)providerUserKey && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() == 1)
            {
                User user = users.First();
                if (user != null)
                {
                    membershipUser = GetMembershipUserFromPersitentObject(user);

                    if (userIsOnline)
                    {
                        user.LastActivityDate = DateTime.Now;
                        _db.SaveChanges();
                    }
                }
            }

            return membershipUser;
        }

        public override MembershipUser GetUser(string UserName, bool userIsOnline)
        {
            MembershipUser membershipUser = null;

            User user;
            if (SF.UseCurrentUserByUserName(UserName)) user = LS.CurrentUser;
            else user = _db.Users.FirstOrDefault(r => r.ApplicationName == applicationName && r.UserName == UserName);

            if (user == null) return null;
            membershipUser = GetMembershipUserFromPersitentObject(user);

            if (userIsOnline)
            {
                user.LastActivityDate = DateTime.Now;
                _db.SaveChanges();
            }

            if (!SF.UseCurrentUserByUserName(UserName) && user != null) _db.Entry(user).State = EntityState.Detached;

            return membershipUser;
        }

        public override string GetUserNameByEmail(string email)
        {
            User user;
            if (SF.UseCurrentUserByEmail(email)) user = LS.CurrentUser;
            else user = _db.Users.FirstOrDefault(r => r.ApplicationName == applicationName && r.Email == email);

            if (!SF.UseCurrentUserByEmail(email) && user != null) _db.Entry(user).State = EntityState.Detached;

            if (user == null) return string.Empty;
            else return user.UserName;
        }

        public override bool DeleteUser(string UserName, bool deleteAllRelatedData)
        {
            User user = _db.Users.FirstOrDefault(r => r.ApplicationName == applicationName && r.UserName == UserName);
            if (user == null) return false;
            _db.Users.Remove(user);
            _db.SaveChanges();

            if (deleteAllRelatedData)
            {
                // TODO: delete user related data
            }
            return true;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();

            //retrieve all users for the current application name from the database


            totalRecords = (from u in _db.Users
                            where u.ApplicationName == applicationName
                            select u).Distinct().Count();
            if (totalRecords <= 0) return users;

            IQueryable<User> efUsers = (from u in _db.Users
                                        where u.ApplicationName == applicationName
                                        orderby u.UserName
                                        select u).Skip(pageIndex * pageSize).Take(pageSize);

            foreach (User user in efUsers)
            {
                users.Add(GetMembershipUserFromPersitentObject(user));
            }


            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);


            return (from u in _db.Users
                    where u.ApplicationName == applicationName && u.LastActivityDate > compareTime
                    select u).Distinct().Count();
        }

        public override MembershipUserCollection FindUsersByName(string UserNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            IQueryable<User> users = from u in _db.Users
                                     where u.UserName.Contains(UserNameToMatch) && u.ApplicationName == applicationName
                                     orderby u.UserName
                                     select u;

            totalRecords = users.Count();
            if (users.Count() <= 0) return membershipUsers;

            foreach (User user in users.Skip(pageIndex * pageSize).Take(pageSize))
            {
                membershipUsers.Add(GetMembershipUserFromPersitentObject(user));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            IQueryable<User> users = from u in _db.Users
                                     where u.Email.Contains(emailToMatch) && u.ApplicationName == applicationName
                                     select u;

            totalRecords = users.Count();
            if (users.Count() <= 0) return membershipUsers;

            foreach (User user in users.Skip(pageIndex * pageSize).Take(pageSize))
            {
                membershipUsers.Add(GetMembershipUserFromPersitentObject(user));
            }

            return membershipUsers;
        }

        #endregion

        #region private methods
        private MembershipUser GetMembershipUserFromPersitentObject(User user)
        {
            return new MembershipUser(Name,
                                      user.UserName,
                                      user.ID,
                                      user.Email,
                                      user.PasswordQuestion,
                                      user.Comment,
                                      user.IsApproved,
                                      Convert.ToBoolean(user.IsLockedOut),
                                      Convert.ToDateTime(user.CreationDate),
                                      Convert.ToDateTime(user.LastLoginDate),
                                      Convert.ToDateTime(user.LastActivityDate),
                                      Convert.ToDateTime(user.LastPasswordChangedDate),
                                      Convert.ToDateTime(user.LastLockedOutDate));
        }

        private void UpdateFailureCount(string UserName, string failureType)
        {

            IQueryable<User> users = from u in _db.Users
                                     where u.UserName == UserName && u.ApplicationName == applicationName
                                     select u;

            if (users.Count() != 1) throw new ProviderException("Update failure count failed. No unique user found.");

            User user = users.First();

            DateTime windowStart = new DateTime();
            int failureCount = 0;

            if (failureType == "password")
            {
                failureCount = Convert.ToInt32(user.FailedPasswordAttemptCount);
                windowStart = Convert.ToDateTime(user.FailedPasswordAttemptWindowStart);
            }

            if (failureType == "passwordAnswer")
            {
                failureCount = Convert.ToInt32(user.FailedPasswordAnswerAttemptCount);
                windowStart = Convert.ToDateTime(user.FailedPasswordAnswerAttemptWindowStart);
            }

            DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

            if (failureCount == 0 || DateTime.Now > windowEnd)
            {
                // First password failure or outside of PasswordAttemptWindow. 
                // Start a new password failure count from 1 and a new window starting now.
                if (failureType == "password")
                {
                    user.FailedPasswordAttemptCount = 1;
                    user.FailedPasswordAttemptWindowStart = DateTime.Now;
                }
                if (failureType == "passwordAnswer")
                {
                    user.FailedPasswordAnswerAttemptCount = 1;
                    user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                }

                try
                {
                    _db.SaveChanges();
                }
                catch
                {
                    throw new ProviderException("Unable to update failure count and window start.");
                }
            }
            else
            {
                if (failureCount++ >= MaxInvalidPasswordAttempts)
                {
                    // Max password attempts have exceeded the failure threshold. Lock out the user.
                    user.IsLockedOut = true;
                    user.LastLockedOutDate = DateTime.Now;

                    try
                    {
                        _db.SaveChanges();
                    }
                    catch
                    {
                        throw new ProviderException("Unable to lock out user.");
                    }
                }
                else
                {
                    // Max password attempts have not exceeded the failure threshold. Update
                    // the failure counts. Leave the window the same.
                    if (failureType == "password")
                    {
                        user.FailedPasswordAttemptCount = failureCount;
                    }
                    if (failureType == "passwordAnswer")
                    {
                        user.FailedPasswordAnswerAttemptCount = failureCount;
                    }

                    try
                    {
                        _db.SaveChanges();
                    }
                    catch
                    {
                        throw new ProviderException("Unable to update failure count.");
                    }
                }
            }
        }

        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            return pass1 == pass2;
        }

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword = Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1 { Key = HexToByte(machineKey.ValidationKey) };
                    encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        private static string GetConfigValue(string configValue, string defaultValue)
        {
            return String.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }

        #endregion
    }
}
