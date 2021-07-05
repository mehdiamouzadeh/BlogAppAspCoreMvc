using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Models
{
    public class BlogDetailViewModel
    {
        public BlogPost blogPost { get; set; }
        public BlogComment Comment { get; set; }
    }
}
