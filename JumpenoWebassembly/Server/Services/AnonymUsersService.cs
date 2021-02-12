using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JumpenoWebassembly.Server.Services
{
    /// <summary>
    /// Servis pre registraciu anonymnych hracov.
    /// </summary>
    public class AnonymUsersService
    {
        private readonly List<AnonymUser> _anonymUsers;

        public AnonymUsersService()
        {
            _anonymUsers = new List<AnonymUser>();
        }

        public AnonymUser GetNewAnonym()
        {
            var rnd = new Random();
            var user = new AnonymUser {
                Id = _anonymUsers.Count == 0 ? -1 : _anonymUsers[^1].Id - 1,
                Name = Usernames.UserNames[rnd.Next(Usernames.UserNames.Length)]
            };
            _anonymUsers.Add(user);
            return user;
        }

        public void RemoveAnonym(int id)
        {
            var usr = _anonymUsers.First(usr => usr.Id == id);
            _anonymUsers.Remove(usr);
        }
    }
}
