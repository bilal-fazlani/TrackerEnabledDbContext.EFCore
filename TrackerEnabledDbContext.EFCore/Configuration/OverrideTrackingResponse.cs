using System;
using System.Linq.Expressions;
using System.Reflection;
using TrackerEnabledDbContext.EFCore.Extensions;

namespace TrackerEnabledDbContext.EFCore.Configuration
{
    public class OverrideTrackingResponse<T>
    {
        public OverrideTrackingResponse<T> Enable()
        {
            //insert/update in entity table
            TrackingConfigurationValue newvalue = new TrackingConfigurationValue(true,
                TrackingConfigurationPriority.High);

            TrackingDataStore.EntityConfigStore.AddOrUpdate(
                typeof(T).FullName,
                (key) => newvalue,
                (key, existingValue) => newvalue);

            return this;
        }

        public OverrideTrackingResponse<T> Disable()
        {
            TrackingConfigurationValue newvalue = new TrackingConfigurationValue(false,
                TrackingConfigurationPriority.High);

            TrackingDataStore.EntityConfigStore.AddOrUpdate(
                typeof(T).FullName,
                (key) => newvalue,
                (key, existingValue) => newvalue);

            return this;
        }

        public OverrideTrackingResponse<T> Enable(Expression<Func<T, object>> property)
        {
            TrackProperty(property);
            return this;
        }

        public OverrideTrackingResponse<T> Disable(Expression<Func<T, object>> property)
        {
            SkipProperty(property);
            return this;
        }

        internal static void SkipProperty(Expression<Func<T, object>> property)
        {
            PropertyInfo info = property.GetPropertyInfo();
            TrackingConfigurationValue newValue = new TrackingConfigurationValue(false, TrackingConfigurationPriority.High);

            TrackingDataStore.PropertyConfigStore.AddOrUpdate(
                new PropertyConfiguerationKey(info.Name, info.DeclaringType.FullName),
                newValue,
                (existingKey, existingvalue) => newValue);
        }

        internal static void TrackProperty(Expression<Func<T, object>> property)
        {
            PropertyInfo info = property.GetPropertyInfo();
            TrackingConfigurationValue newValue = new TrackingConfigurationValue(true, TrackingConfigurationPriority.High);
            TrackingDataStore.PropertyConfigStore.AddOrUpdate(
                new PropertyConfiguerationKey(info.Name, info.DeclaringType.FullName),
                newValue,
                (key, value) => newValue);
        }

    }
}