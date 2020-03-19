using AutoMapper;
using System;

namespace AzurePlay.Common
{
    public static class AzurePlayExtensions
    {
        private static Mapper mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies())));

        public static T MapTo<T>(this object source)
        {
            return mapper.Map<T>(source);
        }
    }
}