using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications.SessionSpec
{
    public class SessionJoinSpec : BaseSpecifications<SessionJoin>
    {
        // Find a specific join record by sessionId and userId
        public SessionJoinSpec(int sessionId, string userId)
            : base(sj => sj.SessionId == sessionId && sj.UserId == userId)
        {
            Includes.Add(sj => sj.User);
        }
        // Find all join records for a specific sessionId
        public SessionJoinSpec(int sessionId)
            : base(sj => sj.SessionId == sessionId)
        {
            Includes.Add(sj => sj.User);
        }
    }
}
