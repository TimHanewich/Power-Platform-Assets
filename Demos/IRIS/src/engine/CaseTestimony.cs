using System;

namespace PSJ
{
    public class CaseTestimony
    {
        public string Name {get; set;}
        public string Testimony {get; set;}

        public CaseTestimony()
        {
            Name = string.Empty;
            Testimony = string.Empty;
        }

        public CaseTestimony(string name, string testimony)
        {
            Name = name;
            Testimony = testimony;
        }
    }
}