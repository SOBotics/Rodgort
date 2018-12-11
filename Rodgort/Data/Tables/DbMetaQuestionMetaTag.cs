namespace Rodgort.Data.Tables
{
    public class DbMetaQuestionMetaTag
    {
        public int MetaQuestionId { get; set; }

        public string TagName { get; set; }

        public DbMetaQuestion MetaQuestion { get; set; }
        public DbMetaTag MetaTag { get; set; }
    }
}
