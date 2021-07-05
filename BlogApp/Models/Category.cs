using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public ICollection<BlogPost> BlogPosts { get; set; }
    }
}
