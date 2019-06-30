﻿using System;
using System.Security.Claims;
using StackExchange.Opserver.Models.Security;

namespace StackExchange.Opserver.Models
{
    public class User
    {
        public string AccountName { get; }
        public bool IsAnonymous { get; }
        public bool IsGlobalAdmin { get; }
        /// <summary>
        /// Returns this user's role on the current site.
        /// </summary>
        public Roles Roles { get; }

        public User(ClaimsPrincipal principle, Roles baseRoles)
        {
            var identity = principle?.Identity;
            if (identity == null)
            {
                IsAnonymous = true;
                return;
            }

            IsAnonymous = !identity.IsAuthenticated;
            if (identity.IsAuthenticated)
                AccountName = identity.Name;

            var roles = baseRoles;

            if (IsAnonymous)
            {
                roles |= Roles.Anonymous;
            }
            else
            {
                roles |= Roles.Authenticated;
                // TODO: Spin up user in middleware
                //var settings = OpserverSettings.Current;

                //if (Current.Security.IsAdmin)
                // TODO: Secure the shizzle
                roles |= Roles.GlobalAdmin;

                //result |= GetRoles(settings.CloudFlare, Roles.CloudFlare, Roles.CloudFlareAdmin);
                //result |= GetRoles(settings.Dashboard, Roles.Dashboard, Roles.DashboardAdmin);
                //result |= GetRoles(settings.Elastic, Roles.Elastic, Roles.ElasticAdmin);
                //result |= GetRoles(settings.Exceptions, Roles.Exceptions, Roles.ExceptionsAdmin);
                //result |= GetRoles(Current.Settings.HAProxy, Roles.HAProxy, Roles.HAProxyAdmin);
                //result |= GetRoles(Current.Settings.Redis, Roles.Redis, Roles.RedisAdmin);
                //result |= GetRoles(Current.Settings.SQL, Roles.SQL, Roles.SQLAdmin);
                //result |= GetRoles(Current.Settings.PagerDuty, Roles.PagerDuty, Roles.PagerDutyAdmin);
            }

            IsGlobalAdmin = (roles & Roles.GlobalAdmin) == Roles.GlobalAdmin;
            Roles = roles;
        }
        private Roles GetRoles(ISecurableModule module, Roles user, Roles admin)
        {
            if (module.IsAdmin()) return admin | user;
            if (module.HasAccess()) return user;
            return Roles.None;
        }

        public bool Is(Roles role) => (Roles & role) == role || IsGlobalAdmin;
        public bool IsInRole(string role) => Enum.TryParse(role, out Roles r) && Is(r);
    }
}