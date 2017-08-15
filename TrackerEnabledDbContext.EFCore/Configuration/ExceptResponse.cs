using System;
using System.Linq.Expressions;

namespace TrackerEnabledDbContext.EFCore.Configuration
{
    public class ExceptResponse<T>
    {
        public ExceptResponse<T> And(Expression<Func<T, object>> property)
        {
            OverrideTrackingResponse<T>.SkipProperty(property);
            return new ExceptResponse<T>();
        }
    }
}