﻿using System.Collections.Concurrent;

namespace TrackerEnabledDbContext.EFCore.Configuration
{
    internal static class TrackingDataStore
    {
        ////////////////////////// STORE /////////////////////////////

        internal static ConcurrentDictionary<string, TrackingConfigurationValue> EntityConfigStore = new ConcurrentDictionary<string, TrackingConfigurationValue>();
        internal static ConcurrentDictionary<PropertyConfiguerationKey, TrackingConfigurationValue> PropertyConfigStore = new ConcurrentDictionary<PropertyConfiguerationKey, TrackingConfigurationValue>();
    }
}
