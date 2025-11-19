using System.Collections.Generic;

namespace StudentPhotoTaker.Models
{
    public class FotolarViewModel
    {
        public string CurrentFolder { get; set; }
        public List<string> Folders { get; set; }
        public List<string> Photos { get; set; }
        public Dictionary<string, string> StudentNames { get; set; }
    }
}
