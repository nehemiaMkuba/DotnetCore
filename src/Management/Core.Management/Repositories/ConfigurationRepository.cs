using System.Linq;
using System.Linq.Dynamic.Core;

using Core.Domain.Enums;
using Core.Domain.Entities;
using Core.Management.Interfaces;
using Core.Domain.Infrastructure.Database;

namespace Core.Management.Repositories
{  

    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IPNContext context;

        public ConfigurationRepository(IPNContext context)
        {
            this.context = context;
        }

        private string instanceName;
        public string InstanceName
        {
            get
            {
                instanceName = !string.IsNullOrEmpty(instanceName) ? instanceName : context.Settings.FirstOrDefault(s => s.Key == nameof(InstanceName)).Value;
                return instanceName;
            }
            set
            {
                instanceName = value;
                Setting setting = context.Settings.FirstOrDefault(s => s.Key == nameof(InstanceName));
                if (setting is null)
                {
                    setting = new Setting { Key = nameof(InstanceName), Value = value };
                    context.Settings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }
                context.SaveChanges();
            }
        }

        private string timeZone;
        public string TimeZone
        {
            get
            {
                timeZone = !string.IsNullOrEmpty(timeZone) ? timeZone : context.Settings.FirstOrDefault(s => s.Key == nameof(TimeZone)).Value;
                return timeZone;
            }
            set
            {
                timeZone = value;
                Setting setting = context.Settings.FirstOrDefault(s => s.Key == nameof(TimeZone));
                if (setting is null)
                {
                    setting = new Setting { Key = nameof(TimeZone), Value = value };
                    context.Settings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }
                context.SaveChanges();
            }
        }

        private string helpline;
        public string Helpline
        {
            get
            {
                helpline = !string.IsNullOrEmpty(helpline) ? helpline : context.Settings.FirstOrDefault(a => a.Key == nameof(Helpline)).Value;
                return helpline;
            }
            set
            {
                helpline = value;
                Setting setting = context.Settings.FirstOrDefault(a => a.Key == nameof(Helpline));
                if (setting is null)
                {
                    setting = new Setting { Key = nameof(Helpline), Value = value.ToString() };
                    context.Settings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }
                context.SaveChanges();
            }
        }

        private string dateTimeFormat;
        public string DateTimeFormat
        {
            get
            {
                dateTimeFormat = !string.IsNullOrEmpty(dateTimeFormat) ? dateTimeFormat : context.Settings.FirstOrDefault(a => a.Key == nameof(DateTimeFormat))?.Value;
                return dateTimeFormat;
            }
            set
            {
                dateTimeFormat = value;

                Setting setting = context.Settings.FirstOrDefault(a => a.Key == nameof(DateTimeFormat));

                if (setting is null)
                {
                    setting = new Setting { Key = nameof(DateTimeFormat), Value = value.ToString() };
                    context.Settings.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }

                context.SaveChanges();
            }
        }

        private int? tokenLifetimeInMins;
        public int TokenLifetimeInMins
        {
            get
            {
                tokenLifetimeInMins ??= int.Parse(context.Settings.FirstOrDefault(a => a.Key == nameof(TokenLifetimeInMins)).Value);
                return tokenLifetimeInMins.Value;
            }
            set
            {
                tokenLifetimeInMins = value;

                Setting setting = context.Settings.FirstOrDefault(a => a.Key == nameof(TokenLifetimeInMins));

                if (setting is null)
                {
                    setting = new Setting { Key = nameof(TokenLifetimeInMins), Value = value.ToString() };
                    context.Settings.Add(setting);
                }
                else
                {
                    setting.Value = value.ToString();
                }
                context.SaveChanges();

            }
        }

        private int? codeLifetimeInMins;
        public int CodeLifetimeInMins
        {
            get
            {
                codeLifetimeInMins ??= int.Parse(context.Settings.FirstOrDefault(a => a.Key == nameof(CodeLifetimeInMins)).Value);
                return codeLifetimeInMins.Value;
            }
            set
            {
                codeLifetimeInMins = value;

                Setting setting = context.Settings.FirstOrDefault(a => a.Key == nameof(CodeLifetimeInMins));

                if (setting is null)
                {
                    setting = new Setting { Key = nameof(CodeLifetimeInMins), Value = value.ToString() };
                    context.Settings.Add(setting);
                }
                else
                {
                    setting.Value = value.ToString();
                }
                context.SaveChanges();

            }
        }

    }
}

