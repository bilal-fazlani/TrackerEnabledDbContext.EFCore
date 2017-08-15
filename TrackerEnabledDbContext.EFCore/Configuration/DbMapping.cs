using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TrackerEnabledDbContext.EFCore.Interfaces;

namespace TrackerEnabledDbContext.EFCore.Configuration
{
    internal class DbMapping
    {
        private readonly ITrackerContext _context;
        private readonly Type _entityType;

        internal DbMapping(ITrackerContext context, Type entityType)
        {
            _context = context;
            _entityType = entityType;
        }

//        internal bool IsOwnedProperty()
//        {
//            return _context.Model.FindEntityType(_entityType)
//                .IsOwned();
//        }

        internal IEnumerable<PropertyConfiguerationKey> PrimaryKeys()
        {
            IEnumerable<string> keyNames = _context.Model.FindEntityType(_entityType)
                .FindPrimaryKey()
                .Properties
                .Select(x => x.Name);

            foreach (string keyName in keyNames)
            {
                yield return new PropertyConfiguerationKey(keyName, _entityType.FullName);
            }
        }
    }
}
