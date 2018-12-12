﻿namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionTag
    {
        public int MetaQuestionId { get; set; }
        public string TagName { get; set; }
        public int RequestTypeId { get; set; }

        public string SecondaryTagName { get; set; }

        public DbMetaQuestion MetaQuestion { get; set; }
        public DbTag Tag { get; set; }
        public DbRequestType RequestType { get; set; }

        public DbTag SecondaryTag { get; set; }
    }
}