
using System.Collections.Generic;

namespace Rodgort.Data.Tables
{
    public class DbUserActionType
    {
        public const int REMOVED_TAG = 1;
        public const int ADDED_TAG = 2;
        public const int CLOSED = 3;
        public const int REOPENED = 4;
        public const int DELETED = 5;
        public const int UNDELETED = 6;
        public const int UNKNOWN_DELETION = 7;

        public int Id { get; set; }

        public string Name { get; set; }

        public List<DbUserAction> UserActions { get; set; }
    }
}
